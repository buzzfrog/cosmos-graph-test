using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace graph_db_test_functional_test
{
    [TestClass]
    public class CosmosDbFunctionalTest
    {
        [TestMethod]
        public void DoDatabaseCheck()
        {
            var cosmosDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };

            var queryOptions = new FeedOptions { MaxItemCount = -1, PartitionKey = new PartitionKey("1") };

            var documentClient = new DocumentClient(new Uri("https://localhost:8081"),
                cosmosDbKey, connectionPolicy);

            var baseQuery = "SELECT VALUE COUNT(c.id) FROM c WHERE c.partitionId = '1'";

            var vertices = documentClient.CreateDocumentQuery(
                UriFactory.CreateDocumentCollectionUri("dbtest", "coltest"),
                baseQuery + " AND IS_DEFINED(c._isEdge) = false", queryOptions).ToList();

            Assert.AreEqual(3916, vertices[0].Value);

            var edges = documentClient.CreateDocumentQuery(
                UriFactory.CreateDocumentCollectionUri("dbtest", "coltest"),
                baseQuery + " AND c._isEdge", queryOptions).ToList();

            Assert.AreEqual(8905, edges[0].Value);
        }
    }
}
