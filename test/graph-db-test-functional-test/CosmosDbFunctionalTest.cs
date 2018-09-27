using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace graph_db_test_functional_test
{
    [TestClass]
    public class CosmosDbFunctionalTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var cosmosDbName = TestContext.Properties["cosmosdbname"];
            var cosmosDbKey = TestContext.Properties["key"];

            Console.WriteLine($"N: {cosmosDbName} K: {cosmosDbKey}");

        }
    }
}
