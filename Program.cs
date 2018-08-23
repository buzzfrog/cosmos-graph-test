using ChanceNET;
using CommandLine;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph.Element;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    class Program
    {
        private static Random _random = new Random();
        private static Chance _chance = new Chance();

        private static string _unparsedConnectionString;
        private static string _rootNodeId;
        private static int _batchSize;
        private static int _numberOfNodesOnEachLevel;

        private static string _accountEndpoint;
        private static string _accountKey;
        private static string _apiKind;
        private static string _database;
        private static string _collection;
        private static string _partitionKey;

        private static DocumentClient _documentClient;
        private static readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp
        };
        private static IBulkExecutor _graphBulkExecutor;
        private static IList<object> _graphElementsToAdd = new List<object>();
        private static long _totalElements = 0;

        private static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resultFromParsing = Parser.Default.ParseArguments<CommandLineOptions>(args);
            if (resultFromParsing.Tag != ParserResultType.Parsed)
                return;

            var result = (Parsed<CommandLineOptions>)resultFromParsing;

            _unparsedConnectionString = result.Value.ConnectionString;
            _rootNodeId = result.Value.RootNode.Trim();
            _batchSize = result.Value.BatchSize;
            _numberOfNodesOnEachLevel = result.Value.NumberOfNodesOnEachLevel;

            ParseConnectionString(_unparsedConnectionString);            

            if (DoWeHaveAllParameters())
            {
                InitializeCosmosDbAsync().Wait();
                InsertNodeAsync(_rootNodeId, string.Empty, string.Empty, 1).Wait();

                // Import remaining vertices and edges
                BulkImportToCosmosDbAsync().Wait();
            }
            else
            {
                Console.WriteLine("Check the parameters");
            }

            stopwatch.Stop();
            Console.WriteLine($"Added {_totalElements} graph elements in {stopwatch.ElapsedMilliseconds} ms");
        }

        private static async Task InitializeCosmosDbAsync()
        {
            _documentClient = new DocumentClient(new Uri(_accountEndpoint), _accountKey, _connectionPolicy);
            var dataCollection = _documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(_database))
                .Where(c => c.Id == _collection).AsEnumerable().FirstOrDefault();

            _partitionKey = dataCollection.PartitionKey.Paths.First().Replace("/", string.Empty);

            // Set retry options high during initialization (default values).
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            _graphBulkExecutor = new GraphBulkExecutor(_documentClient, dataCollection);
            await _graphBulkExecutor.InitializeAsync();

            // Set retries to 0 to pass complete control to bulk executor.
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
        }

        private static bool DoWeHaveAllParameters()
        {
            // ApiKind needs to be Gremlin
            if (_apiKind.ToLower() != "gremlin")
                return false;

            if (_accountEndpoint != string.Empty && _accountKey != string.Empty && 
                _database != string.Empty && _collection != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ParseConnectionString(string unparsedConnectionString)
        {
            foreach (var part in unparsedConnectionString.Trim().Split(';'))
            {
                switch (GetKeyFromPart(part.ToLower()))
                {
                    case "accountendpoint":
                        _accountEndpoint = GetValueFromPart(part);
                        break;
                    case "accountkey":
                        _accountKey = GetValueFromPart(part);
                        break;
                    case "apikind":
                        _apiKind = GetValueFromPart(part);
                        break;
                    case "database":
                        _database = GetValueFromPart(part);
                        break;
                    case "collection":
                        _collection = GetValueFromPart(part);
                        break;
                    default:
                        break;
                }
            }
        }

        private static string GetValueFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[1];
        }

        private static string GetKeyFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[0];
        }

        private static async Task InsertNodeAsync(string id, string parentId, string parentLabel, int level)
        {
            var numberOfNodesToCreate = 0;
            var properties = new Dictionary<string, object>();
            var label = "node";            

            switch (level)
            {
                case 1:
                case 2:
                case 3:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    break;
                case 4:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    label = "asset";
                    properties = new Dictionary<string, object>() {
                        {"manufacturer", _chance.PickOne(new string[] {"fiemens", "babb", "vortex", "mulvo", "ropert"})},
                        {"installedAt", _chance.Timestamp()},
                        {"serial", _chance.Guid().ToString()},
                        {"comments", _chance.Sentence(30)}                          
                    };
                    break;
                case 5:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    label = "asset";
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

            properties.Add(_partitionKey, _rootNodeId);
            properties.Add("level", level);
            properties.Add("createdAt", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            properties.Add("name", id);
            properties.Add("parentId", parentId);

            var padding = new StringBuilder().Append('-', level).ToString();
            Console.WriteLine($"{padding} {id}");

            var vertex = CreateGremlinVertex(id, label, properties);
            await BulkInsertAsync(vertex);

            if (parentId != string.Empty)
            {
                var edge = CreateGremlinEdge(parentId, id, parentLabel, label);
                await BulkInsertAsync(edge);
            }

            for (var i = 0; i < numberOfNodesToCreate; i++)
            {
                await InsertNodeAsync($"{id}-{i}", id, label, level + 1);
            }
        }

        private static GremlinVertex CreateGremlinVertex(string id, string label, 
            Dictionary<string, object> properties)
        {
            var vertex = new GremlinVertex(id, label);

            foreach (var property in properties)
            {
                vertex.AddProperty(property.Key, property.Value);
            }

            return vertex;
        }

        private static GremlinEdge CreateGremlinEdge(string sourceId, string destinationId, 
            string sourceLabel, string destinationLabel)
        {
            var edgeId = $"{sourceId} -> {destinationId}";
            var edge = new GremlinEdge(edgeId, "child", sourceId, destinationId, 
                sourceLabel, destinationLabel, _rootNodeId, _rootNodeId);

            edge.AddProperty("model", "primary");
            return edge;
        }

        private static async Task BulkInsertAsync(object graphElement)
        {
            _totalElements++;
            _graphElementsToAdd.Add(graphElement);
            if (_graphElementsToAdd.Count >= _batchSize)
            {                
                await BulkImportToCosmosDbAsync();                
            }
        }

        private static async Task BulkImportToCosmosDbAsync()
        {
            Console.WriteLine($"Graph elements inserted until now: {_totalElements}");

            var response = await _graphBulkExecutor.BulkImportAsync(
                 documents: _graphElementsToAdd);

            if (response.BadInputDocuments.Any())
                throw new Exception($"BulkExecutor found {response.BadInputDocuments.Count} bad input graph element(s)!");

            _graphElementsToAdd.Clear();
        }
    }
}