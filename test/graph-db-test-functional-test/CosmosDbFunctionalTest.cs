using System;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
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
                var gremlinServer = new GremlinServer($"{cosmosDbName}.gremlin.cosmosdb.azure.com", 443, enableSsl: true,
                                            username: "/dbs/dbtest/colls/coltest",
                                            password: cosmosDbKey);

                using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
                {
                    var result = gremlinClient.SubmitAsync<int>("g.v().count()");
                    Assert.AreEqual(8778, result);
                }

                Assert.AreEqual("cos-test-build-8977", cosmosDbName);
            }

        }
    }
}
