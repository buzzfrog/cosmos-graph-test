using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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

            if (cosmosDbKey == null)
            {
                Assert.Inconclusive("Is not executed in the build context");
            }
            else
            {
                var connectionPolicy = new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                };

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, PartitionKey = new Microsoft.Azure.Documents.PartitionKey("1") };

                var documentClient = new DocumentClient(new Uri("https://localhost:8081"),
                    cosmosDbKey, connectionPolicy);

                var result = documentClient.CreateDocumentQuery(
                    UriFactory.CreateDocumentCollectionUri("dbtest", "coltest"),
                    "SELECT VALUE COUNT(c.id) FROM c WHERE c.partitionId = '1'", queryOptions).ToList();

                Assert.AreEqual(57911, result[0].Value);
            }

        }
    }
}
