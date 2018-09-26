set RESOURCE_GROUP_NAME=%1
set COSMOSDB_ACCOUNT_NAME=%2
az cosmosdb create -g %RESOURCE_GROUP_NAME% -n %COSMOSDB_ACCOUNT_NAME% --capabilities EnableGremlin