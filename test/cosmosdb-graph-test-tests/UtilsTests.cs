using System;
using cosmosdb_graph_test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        public void CreatePartitionKey_With_ThreeLevels_ShouldReturnOneLevels()
        {
            var parameter = "1-2-3";
            var answer = "1";

            var result = Utils.CreatePartitionKey(parameter);

            Assert.AreEqual(result, answer);
        }

        [TestMethod]
        public void GenerateRandomId_Return3Level()
        {
            var random = new Mock<IRandom>();
            random.SetupSequence(x => x.Next(It.IsAny<int>())).Returns(3).Returns(1).Returns(2).Returns(3);
            
            var result = Utils.GenerateRandomId("1", 6, 10, random.Object);

            Assert.AreEqual("1-1-2-3", result);
        }
    }
}
