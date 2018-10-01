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

            var cosmosDbName = Environment.GetEnvironmentVariable("COSMOS_DB_NAME");
            var cosmosDbKey = Environment.GetEnvironmentVariable("COSMOS_DB_KEY");

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

                var documentClient = new DocumentClient(new Uri($"https://{cosmosDbName}.documents.azure.com:443"),
                    cosmosDbKey, connectionPolicy);

                var result = documentClient.CreateDocumentQuery(
                    UriFactory.CreateDocumentCollectionUri("dbtest", "coltest"),
                    "SELECT VALUE COUNT(c.id) FROM c WHERE c.partitionId = '1'", queryOptions).ToList();

                Assert.AreEqual(57911, result[0].Value);
                Assert.AreEqual("cos-test-build-8977", cosmosDbName);
            }

        }
    }
}
