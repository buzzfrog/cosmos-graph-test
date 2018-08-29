using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChanceNET;

namespace cosmosdb_graph_test
{
    class DataCreator
    {
        private static Chance _chance = new Chance();
        private IDatabase _database;
        private IExecutor _executor;
        private IDocumentClient _documentClient;
        private IBulkExecutor _bulkExecutor;
        private string _rootNodeId;
        private int _batchSize;
        private int _numberOfNodesOnEachLevel;
        private int _numberOfTraversals;
        private string _partitionKey;
        private long _totalElements = 0;
        private IList<object> _graphElementsToAdd = new List<object>();

        public DataCreator(IDatabase database, IExecutor executor)
        {
            _database = database;
            _executor = executor;

        }

        public async Task InitializeAsync(string accountEndpoint, string accountKey, string database, string collection)
        {
            _documentClient = _database.Initialize(accountEndpoint, accountKey);

            var documentCollections = _documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(database), null);
            var documentCollection = documentCollections.Where(c => c.Id == collection).AsEnumerable().FirstOrDefault();

            _partitionKey = documentCollection.PartitionKey.Paths.First().Replace("/", string.Empty);

            // Set retry options high during initialization (default values).
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            _bulkExecutor = _executor.Initialize(_documentClient, documentCollection);

            // Set retries to 0 to pass complete control to bulk executor.
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
        }

        public async Task<long> StartAsync(string RootNodeIt, int BatchSize, int NumberOfNodesOnEachLevel, int NumberOfTraversals)
        {
            _rootNodeId = RootNodeIt;
            _batchSize = BatchSize;
            _numberOfNodesOnEachLevel = NumberOfNodesOnEachLevel;
            _numberOfTraversals = NumberOfTraversals;

            // Insert main hierarchy of nodes and edges as a tree
            InsertNodeAsync(_rootNodeId, string.Empty, string.Empty, 1).GetAwaiter().GetResult();

            // Add random edges to nodes
            InsertRandomEdgesAsync(_rootNodeId, _numberOfTraversals).Wait();

            // Import remaining vertices and edges
            BulkImportToCosmosDbAsync().Wait();

            return _totalElements;
        }

        internal async Task InsertNodeAsync(string id, string parentId, string parentLabel, int level)
        {
            var numberOfNodesToCreate = 0;
            var properties = new Dictionary<string, object>();
            var label = "asset";

            switch (level)
            {
                case 1:
                case 2:
                case 3:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    break;
                case 4:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    //label = "asset";
                    properties = new Dictionary<string, object>() {
                        {"manufacturer", _chance.PickOne(new string[] {"fiemens", "babb", "vortex", "mulvo", "ropert"})},
                        {"installedAt", _chance.Timestamp()},
                        {"serial", _chance.Guid().ToString()},
                        {"comments", _chance.Sentence(30)}
                    };
                    break;
                case 5:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    //label = "asset";
                    properties = new Dictionary<string, object>() {
                        {"manufacturer", _chance.PickOne(new string[] {"fiemens", "babb", "vortex", "mulvo", "ropert"})},
                        {"installedAt", _chance.Timestamp()},
                        {"serial", _chance.Guid().ToString()},
                        {"comments", _chance.Sentence(30)}
                    };
                    break;
                default:
                    numberOfNodesToCreate = 0;
                    break;
            }

            properties.Add(_partitionKey, Utils.CreatePartitionKey(id));
            properties.Add("level", level);
            properties.Add("createdAt", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            properties.Add("name", id);
            properties.Add("parentId", parentId);

            var padding = new StringBuilder().Append('-', level).ToString();
            Console.WriteLine($"{padding} {id}");

            var vertex = Utils.CreateGremlinVertex(id, label, properties);
            await BulkInsertAsync(vertex);

            if (parentId != string.Empty)
            {
                var edge = Utils.CreateGremlinEdge("child", parentId, id, parentLabel, label);
                await BulkInsertAsync(edge);
            }

            for (var i = 0; i < numberOfNodesToCreate; i++)
            {
                await InsertNodeAsync($"{id}-{i}", id, label, level + 1);
            }
        }

        internal async Task BulkInsertAsync(object graphElement)
        {
            _totalElements++;
            _graphElementsToAdd.Add(graphElement);
            if (_graphElementsToAdd.Count >= _batchSize)
            {
                await BulkImportToCosmosDbAsync();
            }
        }

        internal async Task BulkImportToCosmosDbAsync()
        {
            Console.WriteLine($"Graph elements inserted until now: {_totalElements}");

            await _executor.BulkImportAsync(_graphElementsToAdd);

            _graphElementsToAdd.Clear();
        }

        internal async Task InsertRandomEdgesAsync(string rootNodeId, int numberOfProcessToInsert)
        {
            for (int i = 0; i < numberOfProcessToInsert; i++)
            {
                Console.WriteLine($"Inserting process {i} of {numberOfProcessToInsert}");
                for (int j = 0; j < 10; j++)
                {
                    var sourceId = Utils.GenerateRandomId(rootNodeId, 5, _numberOfNodesOnEachLevel);
                    var destinationId = Utils.GenerateRandomId(rootNodeId, 5, _numberOfNodesOnEachLevel);

                    var edge = Utils.CreateGremlinEdge("process_" + i.ToString(), sourceId, destinationId, "asset", "asset", " - p_" + i.ToString());
                    await BulkInsertAsync(edge);

                }
            }
        }

    }
}
