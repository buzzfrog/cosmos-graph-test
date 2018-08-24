using System;
using cosmosdb_graph_test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace cosmosdb_graph_test_tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void CreatePartitionKey_With_OneLevel()
        {
            var parameter = "1";
            var answer = "1";

            var result = Utils.CreatePartitionKey(parameter);

            Assert.AreEqual(result, answer);
        }

        [TestMethod]
        public void CreatePartitionKey_With_ThreeLevels_ShouldReturnTwoLevels()
        {
            var parameter = "1-2-3";
            var answer = "1-2";

            var result = Utils.CreatePartitionKey(parameter);

            Assert.AreEqual(result, answer);
        }
    }
}
