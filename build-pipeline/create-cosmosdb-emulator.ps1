& 'C:\Program Files\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe' /noui
Install-Module -Name CosmosDB -Force
$cosmosDbContext = New-CosmosDbContext -Emulator -Database 'dbtest'
New-CosmosDbDatabase -Context $cosmosDbContext -Id 'dbtest'
New-CosmosDbCollection -Context $cosmosDbContext -Id 'coltest' -PartitionKey 'partitionId' -OfferThroughput 50000 
