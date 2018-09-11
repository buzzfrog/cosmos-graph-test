using graph_db_test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace graph_db_test_tests
{
    [TestClass]
    public class CommandLineUtilsTests
    {
        [TestMethod]
        public void ParseGraphDbType_CosmosDb()
        {
            var result =  CommandLineUtils.ParseGraphDbType("AccountEndpoint=https://graph-database.documents.azure.com:443/;AccountKey=FakeKeyU8WB0cNFR0QvWT0jBouMnIqYuavySbwmYK3Ur2xvNBuVhAv3HHnrxhYBNf3dO2Kugbw==;ApiKind=Gremlin;Database=db001;Collection=col003");
            Assert.AreEqual(result, GraphDbType.CosmosDb);
        }

        [TestMethod]
        public void ParseGraphDbType_SqlServer()
        {
            var result = CommandLineUtils.ParseGraphDbType("Server=tcp:server.database.windows.net,1433;Initial Catalog=graph-db;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            Assert.AreEqual(result, GraphDbType.SqlServer);
        }

        [TestMethod]
        public void ParseGraphDbType_Unknown()
        {
            var result = CommandLineUtils.ParseGraphDbType("some random string");
            Assert.AreEqual(result, GraphDbType.Unknown);
        }

        [TestMethod]
        public void ParseCosmosDbConnectionString()
        {
            string accountEndpoint = "https://graph-database.documents.azure.com:443/";
            string accountKey = "FakeKeyU8WB0cNFR0QvWT0jBouMnIqYuavySbwmYK3Ur2xvNBuVhAv3HHnrxhYBNf3dO2Kugbw==";
            string apiKind = "Gremlin";
            string database = "db001";
            string collection = "col003";

            var result = CommandLineUtils.ParseCosmosDbConnectionString("AccountEndpoint=https://graph-database.documents.azure.com:443/;AccountKey=FakeKeyU8WB0cNFR0QvWT0jBouMnIqYuavySbwmYK3Ur2xvNBuVhAv3HHnrxhYBNf3dO2Kugbw==;ApiKind=Gremlin;Database=db001;Collection=col003");

            Assert.AreEqual(result.accountEndpoint, accountEndpoint);
            Assert.AreEqual(result.accountKey, accountKey);
            Assert.AreEqual(result.apiKind, apiKind);
            Assert.AreEqual(result.database, database);
            Assert.AreEqual(result.collection, collection);
        }
    }
}
