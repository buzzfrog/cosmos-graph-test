using System;
using cosmosdb_graph_test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace cosmosdb_graph_test_tests
{
    [TestClass]
    public class CommandLineUtilsTests
    {
        [TestMethod]
        public void ParseConnectionString()
        {
            string accountEndPoint = "https://graph-database.documents.azure.com:443/";
            string accountKey = "FakeKeyU8WB0cNFR0QvWT0jBouMnIqYuavySbwmYK3Ur2xvNBuVhAv3HHnrxhYBNf3dO2Kugbw==";
            string apiKind = "Gremlin";
            string database = "db001";
            string collection = "col003";

            (string AccountEndPoint, 
             string AccountKey, 
             string ApiKind, 
             string Database, 
             string Collection) result  =  CommandLineUtils.ParseConnectionString("AccountEndpoint=https://graph-database.documents.azure.com:443/;AccountKey=FakeKeyU8WB0cNFR0QvWT0jBouMnIqYuavySbwmYK3Ur2xvNBuVhAv3HHnrxhYBNf3dO2Kugbw==;ApiKind=Gremlin;Database=db001;Collection=col003");

            Assert.AreEqual(result.AccountEndPoint, accountEndPoint);
            Assert.AreEqual(result.AccountKey, accountKey);
            Assert.AreEqual(result.ApiKind, apiKind);
            Assert.AreEqual(result.Database, database);
            Assert.AreEqual(result.Collection, collection);
        }
    }
}
