using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChanceNET;

namespace cosmosdb_graph_test
{
    public class DataCreator
    {
        private Chance _chance = new Chance();
        private IDatabase _database;

        private Random _random = new Random();
        private string _rootNodeId;        
        private int _numberOfNodesOnEachLevel;
        private int _numberOfTraversals;

        private long _totalGraphElements = 0;

        public DataCreator(IDatabase database)
        {
            _database = database;
        }

        public async Task InitializeAsync()
        {
            await _database.InitializeAsync();
        }

        public async Task<long> StartAsync(string rootNodeId, int numberOfNodesOnEachLevel, int numberOfTraversals)
        {
            _rootNodeId = rootNodeId;
            _numberOfNodesOnEachLevel = numberOfNodesOnEachLevel;
            _numberOfTraversals = numberOfTraversals;

            // Insert main hierarchy of nodes and edges as a tree
            await InsertNodeAsync(_rootNodeId, string.Empty, string.Empty, 1);

            // Add random edges to nodes
            await InsertRandomEdgesAsync(_rootNodeId, _numberOfTraversals);

            // Import remaining vertices and edges
            await _database.FlushAsync();
            return _totalGraphElements;
        }

        private async Task InsertNodeAsync(string id, string parentId, string parentLabel, int level)
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

            properties.Add("level", level);
            properties.Add("createdAt", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            properties.Add("name", id);
            properties.Add("parentId", parentId);

            var padding = new StringBuilder().Append('-', level).ToString();
            Console.WriteLine($"{padding} {id}");

            var partitionKey = CreatePartitionKey(id);
            _totalGraphElements++;
            await _database.InsertVertexAsync(id, label, properties, partitionKey);

            if (parentId != string.Empty)
            {
                _totalGraphElements++;
                await _database.InsertEdgeAsync("child", parentId, id, parentLabel, label, 
                    CreatePartitionKey(parentId), partitionKey);                
            }

            for (var i = 0; i < numberOfNodesToCreate; i++)
            {
                await InsertNodeAsync($"{id}-{i}", id, label, level + 1);
            }
        }

        private async Task InsertRandomEdgesAsync(string rootNodeId, int numberOfProcessToInsert)
        {
            for (int i = 0; i < numberOfProcessToInsert; i++)
            {
                Console.WriteLine($"Inserting process {i} of {numberOfProcessToInsert}");
                for (int j = 0; j < 10; j++)
                {
                    var sourceId = GenerateRandomId(rootNodeId, 5, _numberOfNodesOnEachLevel);
                    var destinationId = GenerateRandomId(rootNodeId, 5, _numberOfNodesOnEachLevel);

                    _totalGraphElements++;
                    await _database.InsertEdgeAsync("process_" + i.ToString(), sourceId, destinationId, 
                        "asset", "asset", CreatePartitionKey(sourceId), CreatePartitionKey(destinationId), 
                        " - p_" + i.ToString() + "_" + j.ToString());
                }
            }
        }

        public static string CreatePartitionKey(string id)
        {
            return id.Split('-')[0];
        }

        public string GenerateRandomId(string rootNodeId, int levelsInGraph, int numberOfNodesOnEachLevel)
        {
            var sb = new StringBuilder(rootNodeId);
            var levelsIdShouldBeCreatedFor = _random.Next(levelsInGraph);
            for (int i = 0; i < levelsIdShouldBeCreatedFor; i++)
            {
                sb.Append("-" + _random.Next(numberOfNodesOnEachLevel).ToString());
            }

            return sb.ToString();
        }
    }
}
