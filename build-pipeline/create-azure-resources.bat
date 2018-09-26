set RESOURCE_GROUP_NAME=%1
set COSMOSDB_ACCOUNT_NAME=%2
set DATABASE=dbtest
set COLLECTION=coltest

az cosmosdb create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --capabilities EnableGremlin

az cosmosdb database create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --db-name %DATABASE%

az cosmosdb collection create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% ^
        --db-name %DATABASE% --collection-name %COLLECTION% ^
        --throughput 10000 --partition-key-path "/partitionId"