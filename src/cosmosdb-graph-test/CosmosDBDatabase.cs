using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph.Element;
using Microsoft.Azure.Documents.Client;

namespace cosmosdb_graph_test
{
    public class CosmosDbDatabase : IDatabase
    {
        private (string accountEndpoint,
                         string accountKey,
                         string apiKind,
                         string database,
                         string collection) _cosmosDbConnectionString;

        private DocumentClient _documentClient;
        private GraphBulkExecutor _graphBulkExecutor;
        public string _partitionKeyName;

        private int _batchSize;

        private IList<object> _graphElementsToAdd = new List<object>();

        public CosmosDbDatabase((string accountEndpoint,
                         string accountKey,
                         string apiKind,
                         string database,
                         string collection) cosmosDbConnectionString, int batchSize)
        {
            _cosmosDbConnectionString = cosmosDbConnectionString;
            _batchSize = batchSize;
        }

        public async Task InitializeAsync()
        {
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };

            _documentClient = new DocumentClient(new Uri(_cosmosDbConnectionString.accountEndpoint), 
                _cosmosDbConnectionString.accountKey, connectionPolicy);

            var documentCollections = _documentClient.CreateDocumentCollectionQuery(
                UriFactory.CreateDatabaseUri(_cosmosDbConnectionString.database), null);

            var documentCollection = documentCollections.Where(c => c.Id == _cosmosDbConnectionString.collection)
                .AsEnumerable().FirstOrDefault();

            _partitionKeyName = documentCollection.PartitionKey.Paths.First().Replace("/", string.Empty);

            // Set retry options high during initialization (default values).
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            _graphBulkExecutor = new GraphBulkExecutor(_documentClient, documentCollection);

            // Set retries to 0 to pass complete control to bulk executor.
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
        }

        public async Task InsertVertexAsync(string id, string label,
            Dictionary<string, object> properties, string partitionKeyValue = "")
        {
            var vertex = new GremlinVertex(id, label);

            foreach (var property in properties)
            {
                vertex.AddProperty(property.Key, property.Value);
            }

            if (!string.IsNullOrWhiteSpace(partitionKeyValue))
            {
                vertex.AddProperty(_partitionKeyName, partitionKeyValue);
            }

            await InsertGraphElementAsync(vertex);
        }

        public async Task InsertEdgeAsync(string edgeLabel, string sourceId, string destinationId,
            string sourceLabel, string destinationLabel, string sourcePartitionKey, 
            string destinationPartitionKey, string edgeIdSuffix = "")
        {
            var edgeId = $"{sourceId} -> {destinationId}{edgeIdSuffix}";
            var edge = new GremlinEdge(edgeId, edgeLabel, sourceId, destinationId,
                sourceLabel, destinationLabel, sourcePartitionKey, destinationPartitionKey);

            await InsertGraphElementAsync(edge);
        }

        private async Task InsertGraphElementAsync(object graphElement)
        {
            _graphElementsToAdd.Add(graphElement);
            if (_graphElementsToAdd.Count >= _batchSize)
            {
                await FlushAsync();
            }
        }

        public async Task FlushAsync()
        {
            await BulkImportAsync(_graphElementsToAdd);
            _graphElementsToAdd.Clear();
        }

        private async Task BulkImportAsync(IEnumerable<object> documents)
        {
            var response = await _graphBulkExecutor.BulkImportAsync(documents);

            if (response.BadInputDocuments.Any())
                throw new Exception($"BulkExecutor found {response.BadInputDocuments.Count} bad input graph element(s)!");
        }
    }
}
