SET RESOURCE_GROUP_NAME=%1
SET COSMOSDB_ACCOUNT_NAME=%2
SET DATABASE=dbtest
SET COLLECTION=coltest

call az cosmosdb create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --capabilities EnableGremlin

call az cosmosdb database create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --db-name %DATABASE%

call az cosmosdb collection create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% ^
        --db-name %DATABASE% --collection-name %COLLECTION% ^
        --throughput 10000 --partition-key-path "/partitionId"

FOR /F "usebackq" %%i IN (`az cosmosdb list-keys -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --query primaryMasterKey -o tsv`) DO SET COSMOSDB_KEY=%%i