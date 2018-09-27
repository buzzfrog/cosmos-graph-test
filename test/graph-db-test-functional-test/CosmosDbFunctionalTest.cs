using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace graph_db_test_functional_test
{
    [TestClass]
    public class CosmosDbFunctionalTest
    {
        
        [TestMethod]
        public void TestMethod1()
        {

            var cosmosDbName = Environment.GetEnvironmentVariable("COSMOS_DB_NAME");
            var cosmosDbKey = Environment.GetEnvironmentVariable("COSMOS_DB_KEY");

            Console.WriteLine($"N: {cosmosDbName} K: {cosmosDbKey}");

        }
    }
}
