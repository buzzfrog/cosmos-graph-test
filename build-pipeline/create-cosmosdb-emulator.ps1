& 'C:\Program Files\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe' /noui
Install-Module -Name CosmosDB -Force
$cosmosDbContext = New-CosmosDbContext -Emulator -Database 'db001'
New-CosmosDbDatabase -Context $cosmosDbContext -Id 'db001'
New-CosmosDbCollection -Context $cosmosDbContext -Id 'col001' -PartitionKey 'partitionId' -OfferThroughput 50000 
