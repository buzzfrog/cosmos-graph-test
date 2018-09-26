using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChanceNET;

namespace graph_db_test
{
    public class DataCreator
    {
        private Chance _chance = new Chance();
        private IDatabase _database;
        private Random _random = new Random();
        private int _numberOfNodesOnEachLevel;

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
            _numberOfNodesOnEachLevel = numberOfNodesOnEachLevel;

            // Insert main hierarchy of nodes and edges as a tree
            await InsertNodeAsync(rootNodeId, string.Empty, string.Empty, 1);

            // Add random edges to nodes
            await InsertVertexThatConnectsToOtherVertexesAsync(rootNodeId, numberOfTraversals);

            // Import remaining vertices and edges
            await _database.FlushAsync();

            return _totalGraphElements;
        }

        private async Task InsertNodeAsync(string id, string parentId, string parentLabel, int level)
        {
            var numberOfNodesToCreate = 0;
            var optionalProperties = new Dictionary<string, object>();
            var label = "asset";

            // NOTE: IF YOU MODIFIED VERTEX PROPERTIES, YOU MUST ADJUST THE SQL DB SCHEMA ACCORDINGLY!!!
            switch (level)
            {
                // level 1 to 4
                case int i when i <= 4:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    break;
                // level 5 and 6
                case int i when i >= 5 && i <= 6:
                    numberOfNodesToCreate = _numberOfNodesOnEachLevel;
                    optionalProperties = new Dictionary<string, object>() {
                        {"manufacturer", _chance.PickOne(new string[] {"fiemens", "babb", "vortex", "mulvo", "ropert"})},
                        {"installedAt", _chance.Timestamp()},
                        {"serial", _chance.Guid().ToString()},
                        {"comments", _chance.Sentence(30)}
                    };
                    break;
            }

            // check if leaf node, then no new nodes should be created
            if (level == 6)
            {
                numberOfNodesToCreate = 0;
            }

            var mandatoryProperties = new Dictionary<string, object>
            {
                { "level", level },
                { "createdAt", DateTimeOffset.Now.ToUnixTimeMilliseconds() },
                { "name", id },
                { "parentId", parentId }
            };

            var padding = new StringBuilder().Append('-', level).ToString();
            Console.WriteLine($"{padding} {id}");

            var partitionKey = CreatePartitionKey(id);

            _totalGraphElements++;
            await _database.InsertVertexAsync(id, label, mandatoryProperties, optionalProperties, partitionKey);

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

        private async Task InsertVertexThatConnectsToOtherVertexesAsync(string rootNodeId, int numberOfVertexesToInsert)
        {
            for (int i = 0; i < numberOfVertexesToInsert; i++)
            {
                await _database.InsertVertexAsync($"document-{i}", "document", new Dictionary<string, object>(), new Dictionary<string, object>(), rootNodeId);

                Console.WriteLine($"Inserting document {i} of {numberOfVertexesToInsert}");

                var nodesToAttachToDocumentNode = new List<string>();
                for (int j = 0; j < 500; j++)
                {
                    string id;
                    do
                    {
                        id = GenerateRandomId(rootNodeId, 5, _numberOfNodesOnEachLevel);
                    } while (nodesToAttachToDocumentNode.Contains(id));
                    nodesToAttachToDocumentNode.Add(id);

                    await _database.InsertEdgeAsync($"tags-in-document", $"document-{i}", id, "document", "asset", rootNodeId, rootNodeId);

                    _totalGraphElements++;
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