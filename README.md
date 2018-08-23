# cosmos-graph-test

This application is built to load test Industrial IoT assets hierarchy with Azure Cosmos DB Graph API. It will attempt to generate millions of assets as vertices and the relationships between them as edges and divide these into 6 levels (currently configured in code).

> This only works for Cosmos Db databases with partitions.

## Configuration

- Cosmos DB Connection String.
```
-c AccountEndpoint=https://<cosmosdb-name>.documents.azure.com:443/;AccountKey=[...]==;ApiKind=Gremlin;Database=graphdb;Collection=graphcoll
```
- The batch size of graph elements which should be inserted at the same time to Cosmos DB.
```
-b 1000
```
- Name of the root node (e.g. Industrial Plant Id). This value will serve as the **partition key** of the Cosmos DB collection. It will also be prefixed in all children Vertex Ids for convenience.
```
-r plant13
```

## Scale Out
We can scale out the load generation process by spawning multiple instances of this app and each time provide a different `-r` parameter. In order to not throttle CPU, memory and network badwidth by running on a single machine, we've containerized this application. Now it can be provisioned very easily with Azure Container Instances.
>[Azure CLI >= 2.0.44](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) is required.

```
./run.sh <RESOURCE_GROUP> <COSMOSDB_ACCOUNT> <DATABASE> <COLLECTION>
```

The above script will perform the following actions;

1. Create the resource group in Western Europe if needed.
2. Create a Cosmos DB account with Gremlin API if needed.
3. Create Cosmos DB database if needed.
4. Recreate Cosmos DB collection with specified RU throughput and partition key. This can be prevented by passing an additional flag to the script.
5. Create an Azure Container Registry if needed.
6. Use [ACR Build](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tutorial-quick-build) to create and publish docker image of the application if needed. This can also be forced with an additional flag to the script. **Note:** Since the [Windows Server Core base images](https://hub.docker.com/r/microsoft/dotnet-framework/tags/) are considerably large, it takes several minutes for the build to complete.
7. Based on the provisioned Cosmos DB RU, create `Min(RU/10000, 20)` Azure Container Instances. Each ACI Id will be passed to the application's `-r` parameter.

## Resources
- [.NET sample of Bulk Executor Utility for Azure Cosmos DB Gremlin API](https://github.com/Azure-Samples/azure-cosmosdb-graph-bulkexecutor-dotnet-getting-started)
- [Maximizing The Throughput of Azure Cosmos DB Bulk Import Library](https://medium.com/@jayanta.mondal/azure-cosmos-db-bulk-import-tool-realizing-the-full-potential-722bb4f98476)
- [How the CosmosDB Bulk Executor works under the hood](http://chapsas.com/how-the-cosmosdb-bulk-executor-works-under-the-hood/)
