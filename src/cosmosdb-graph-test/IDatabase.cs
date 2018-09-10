using System.Collections.Generic;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    public interface IDatabase
    {
        Task InitializeAsync();

        Task InsertVertexAsync(string id, string label, Dictionary<string, object> properties, 
            string partitionKeyValue = "");

        Task InsertEdgeAsync(string edgeLabel, string sourceId, string destinationId,
            string sourceLabel, string destinationLabel, string sourcePartitionKey,
            string destinationPartitionKey, string edgeIdSuffix = "");

        Task FlushAsync();
    }
}
