#!/bin/bash

set -e
set -o pipefail

if [ -z "$1" ]; then echo "RESOURCE_GROUP was not supplied"; exit 1; fi && RESOURCE_GROUP=$1
if [ -z "$2" ]; then echo "COSMOSDB_ACCOUNT was not supplied"; exit 1; fi && COSMOSDB_ACCOUNT=$2
if [ -z "$3" ]; then echo "DATABASE was not supplied"; exit 1; fi && DATABASE=$3
if [ -z "$3" ]; then echo "COLLECTION was not supplied"; exit 1; fi && COLLECTION=$4

EMPTY_COLLECTION=${5:-true}
REBUILD_IMAGE=${6:-false}

RU_THROUGHPUT=100000

# One Cosmos DB partition is 10GB or 10k RUs. Total Container Instances shouldn't exceed 20.
INSTANCES=$((RU_THROUGHPUT/10000))
INSTANCES=$(($INSTANCES>20?20:$INSTANCES))

PARTITION_KEY="_partitionKey"
BATCH_SIZE=5000
NUMBEROFNODESONEACHLEVEL=18
ADDITIONALTRAVERSALS=500000
ACR_NAME=${RESOURCE_GROUP//[-_]/}
IMAGE_NAME="cosmos-graph-test"
IMAGE_TAG="1.0"

RG_EXISTS=$(az group exists -n $RESOURCE_GROUP -o tsv)
if [[ "$RG_EXISTS" != true ]]; then
  az group create -l westeurope -n $RESOURCE_GROUP
fi

az container list -g $RESOURCE_GROUP --query "[?starts_with(name, '$IMAGE_NAME')].name" -o tsv | while read container
do
    az container delete -g $RESOURCE_GROUP -n $container -y
done

COSMOSDB_EXISTS=$(az cosmosdb check-name-exists -n $COSMOSDB_ACCOUNT -o tsv)
if [[ "$COSMOSDB_EXISTS" != true ]]; then
  az cosmosdb create -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --capabilities EnableGremlin
fi

DATABASE_EXISTS=$(az cosmosdb database exists -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --db-name $DATABASE -o tsv)
if [[ "$DATABASE_EXISTS" != true ]]; then
  az cosmosdb database create -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --db-name $DATABASE
fi

if [[ "$EMPTY_COLLECTION" = true ]]; then
  az cosmosdb collection delete -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --db-name $DATABASE --collection-name $COLLECTION
fi

COLLECTION_EXISTS=$(az cosmosdb collection exists -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --db-name $DATABASE --collection-name $COLLECTION -o tsv)
if [[ "$COLLECTION_EXISTS" != true ]]; then
    az cosmosdb collection create -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT \
        --db-name $DATABASE --collection-name $COLLECTION \
        --throughput $RU_THROUGHPUT --partition-key-path "/$PARTITION_KEY" \
		--indexing-policy '{
      "indexingMode": "lazy",
      "automatic": true,
      "includedPaths": [
        {
          "path": "/*",
          "indexes": [
            {
              "kind": "Range",
              "dataType": "String",
              "precision": -1
            },
            {
              "kind": "Range",
              "dataType": "Number",
              "precision": -1
            }
          ]
        }
      ]
    }'
fi

ACR_EXISTS=$(az acr list -g $RESOURCE_GROUP --query "[].contains(name, '$ACR_NAME')" -o tsv)
if [[ "$ACR_EXISTS" != true ]]; then
    az acr create -g $RESOURCE_GROUP -n $ACR_NAME --sku Standard --admin-enabled true
fi

ACR_SERVER=$(az acr show -n $ACR_NAME --query loginServer -o tsv)
ACR_IMAGE="$ACR_SERVER/$IMAGE_NAME:$IMAGE_TAG"
IMAGE_EXISTS=$(az acr repository list -n $ACR_NAME --query "[].contains(@, '$IMAGE_NAME')" -o tsv)

if [[ "$REBUILD_IMAGE" = true || "$IMAGE_EXISTS" != true ]]; then
    az acr build --registry $ACR_NAME --image $ACR_IMAGE --os windows .
fi

ACR_USERNAME=$(az acr credential show -n $ACR_NAME --query username -o tsv)
ACR_PASSWORD=$(az acr credential show -n $ACR_NAME --query "passwords[0].value" -o tsv)

COSMOSDB_KEY=$(az cosmosdb list-keys -g $RESOURCE_GROUP -n $COSMOSDB_ACCOUNT --query primaryMasterKey -o tsv)
CONNECTION_STRING="AccountEndpoint=https://$COSMOSDB_ACCOUNT.documents.azure.com:443/;AccountKey=$COSMOSDB_KEY;ApiKind=Gremlin;database=$DATABASE;collection=$COLLECTION"

for (( i=0; i<$INSTANCES; i++ ))
do
    az container create -g $RESOURCE_GROUP -n "$IMAGE_NAME$i" --image $ACR_IMAGE \
        --restart-policy Never --os-type Windows --cpu 4 --memory 14 \
        --registry-login-server $ACR_SERVER \
        --registry-username $ACR_USERNAME --registry-password $ACR_PASSWORD \
        --command-line "cosmosdb-graph-test.exe -b $BATCH_SIZE -r $i -c $CONNECTION_STRING -n $NUMBEROFNODESONEACHLEVEL -a $ADDITIONALTRAVERSALS"
done