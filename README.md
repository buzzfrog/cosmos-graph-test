# graph-db-test

[![Build status](https://vjrantal.visualstudio.com/cosmos-graph-test/_apis/build/status/cosmos-graph-test-.NET%20Desktop-CI)](https://vjrantal.visualstudio.com/cosmos-graph-test/_build/latest?definitionId=2)

This application is built to load test Industrial IoT assets hierarchy with Graph Databases such as Azure Cosmos DB and SQL Server 2017. It will attempt to generate millions of assets as vertices and the relationships between them as edges and divide these into 6 levels (currently configured in code).

## Requirements

Azure Cosmos DB Gremlin API with a partitioned Collection

OR

Azure SQL Database/SQL Server 2017

> SQL Database requires `ddl.sql` script to be executed on the database prior to running this application.

## Configuration

- Database Connection String
```
-c AccountEndpoint=https://{cosmosdb_name}.documents.azure.com:443/;AccountKey={primary_key};ApiKind=Gremlin;Database={database_name};Collection={collection_name}
```
OR
```
-c Server=tcp:{server_name}.database.windows.net,1433;Initial Catalog={database_name};Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```
- The batch size of graph elements which should be inserted together.
```
-b 1000
```
- Name of the root node (e.g. Industrial Plant Id). This value will serve as the **partition key** of the Cosmos DB collection. It will also be prefixed in all children Vertex Ids for convenience.
```
-r plant13
```
- Nodes on each level
```
-n 18
```
- Addition traversals to add. Each traversal creates ten edges between ten random selected nodes
```
-a 200000
```
- Warmup period (in ms) before the program starts. (This is mainly used in ACI where we sometimes need to wait for network access)
```
-w 30000
```

## Cosmos DB Scale Out

We can scale out the load generation process by spawning multiple instances of this app and each time provide a different `-r` parameter. In order to not throttle CPU, memory and network badwidth by running on a single machine, we've containerized this application. Now it can be provisioned very easily with Azure Container Instances.
> [Azure CLI >= 2.0.44](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) is required.

```
./run-cosmos-db.sh <RESOURCE_GROUP> <COSMOSDB_ACCOUNT> <DATABASE> <COLLECTION>
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

### Cosmos DB

- [.NET sample of Bulk Executor Utility for Azure Cosmos DB Gremlin API](https://github.com/Azure-Samples/azure-cosmosdb-graph-bulkexecutor-dotnet-getting-started)
- [Maximizing The Throughput of Azure Cosmos DB Bulk Import Library](https://medium.com/@jayanta.mondal/azure-cosmos-db-bulk-import-tool-realizing-the-full-potential-722bb4f98476)
- [How the CosmosDB Bulk Executor works under the hood](http://chapsas.com/how-the-cosmosdb-bulk-executor-works-under-the-hood/)

### SQL Server 2017

- [Graph processing with SQL Server and Azure SQL Database](https://cloudblogs.microsoft.com/sqlserver/2017/04/20/graph-data-processing-with-sql-server-2017/)
- [Analyzing Flight Data with the SQL Server 2017 Graph Database](https://bytefish.de/blog/sql_server_2017_graph_database/)
