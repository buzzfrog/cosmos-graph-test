& "C:\Program Files\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe" /noui
Install-Module -Name CosmosDB -Force

$DATABASE = "dbtest"
$COLLECTION = "coltest"

$COSMOSDB_CONTEXT = New-CosmosDbContext -Emulator -Database $DATABASE
New-CosmosDbDatabase -Context $COSMOSDB_CONTEXT -Id $DATABASE
New-CosmosDbCollection -Context $COSMOSDB_CONTEXT -Id $COLLECTION -PartitionKey "partitionId" -OfferThroughput 1000

$CONNECTION_STRING = "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;ApiKind=Gremlin;database=$DATABASE;collection=$COLLECTION"

dotnet run -c Release -p .\src\graph-db-test\ --no-launch-profile -- -b 500 -r 1 -c $CONNECTION_STRING -n 5 -a 10 -w 0