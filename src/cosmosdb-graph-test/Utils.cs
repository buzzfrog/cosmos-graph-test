using Microsoft.Azure.CosmosDB.BulkExecutor.Graph.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    internal class Utils
    {
        private static Random _random = new Random();

        internal static GremlinVertex CreateGremlinVertex(string id, string label,
            Dictionary<string, object> properties)
        {
            var vertex = new GremlinVertex(id, label);

            foreach (var property in properties)
            {
                vertex.AddProperty(property.Key, property.Value);
            }

            return vertex;
        }

        internal static GremlinEdge CreateGremlinEdge(string edgeLabel, string sourceId, string destinationId,
            string sourceLabel, string destinationLabel, string edgeIdSuffix = null)
        {
            var edgeId = $"{sourceId} -> {destinationId}{edgeIdSuffix}";
            var edge = new GremlinEdge(edgeId, edgeLabel, sourceId, destinationId,
                sourceLabel, destinationLabel, CreatePartitionKey(sourceId), CreatePartitionKey(destinationId));

            edge.AddProperty("model", "primary");
            return edge;
        }

        internal static string GenerateRandomId(string rootNodeId, int levelsInGraph, int numberOfNodesOnEachLevel, IRandom random)
        {
            var sb = new StringBuilder(rootNodeId);
            var howManyLevelsShouldWeCreateIdFor = random.Next(levelsInGraph);
            for (int i = 0; i < howManyLevelsShouldWeCreateIdFor; i++)
            {
                sb.Append("-" + random.Next(numberOfNodesOnEachLevel).ToString());
            }

            return sb.ToString();
        }

        internal static string CreatePartitionKey(string id)
        {
            return id.Split('-')[0];
        }
    }
}
