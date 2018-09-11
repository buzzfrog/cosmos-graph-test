using cosmosdb_graph_test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cosmosdb_graph_test_tests
{
    [TestClass]
    public class DataCreatorTests
    {
        private Mock<IDatabase> _db;
        private DataCreator _dataCreator;

        [TestInitialize]
        public void TestInit()
        {
            _db = new Mock<IDatabase>();
            _dataCreator = new DataCreator(_db.Object);
        }

        [TestMethod]
        public void Start_And_Insert_XXX_documents()
        {
            var rootNodeId = "1";
            var numberOfNodesOnEachLevel = 4;
            var numberOfTraversals = 100;

            _dataCreator.InitializeAsync().GetAwaiter().GetResult();

            var result = _dataCreator.StartAsync(rootNodeId, numberOfNodesOnEachLevel, numberOfTraversals).GetAwaiter().GetResult();

            Assert.AreEqual(result, 3729);
        }

        [TestMethod]
        public void Verify_That_Six_Levels_Are_Created()
        {
            var rootNodeId = "1";
            var numberOfNodesOnEachLevel = 4;
            var numberOfTraversals = 100;

            var vertices = new List<(Dictionary<string, object> mandatoryProperties, 
                Dictionary<string, object> optionalProperties)>();

            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(x => x.InsertVertexAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), 
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string, Dictionary<string, object>, Dictionary<string, object>, string>
                ((id, label, mandatoryProperties, optionalProperties, partitionKeyValue) => 
                {
                    vertices.Add((mandatoryProperties, optionalProperties));
                });

            _dataCreator = new DataCreator(dbMock.Object);
            _dataCreator.InitializeAsync().GetAwaiter().GetResult();

            var result = _dataCreator.StartAsync(rootNodeId, numberOfNodesOnEachLevel, numberOfTraversals)
                .GetAwaiter().GetResult();

            var vertexOfLevel6 = vertices.First(x => (int)x.mandatoryProperties["level"] == 6);
            Assert.IsTrue(vertexOfLevel6.optionalProperties.Any(x => x.Key == "manufacturer"));
        }

        [TestMethod]
        public void GenerateRandomId_ReturnUpto6Level()
        {
            var result = _dataCreator.GenerateRandomId("1", 6, 10);

            Assert.IsTrue(result.StartsWith("1"));
            Assert.IsTrue(result.Split('-').Length <= 6);
        }

        [TestMethod]
        public void CreatePartitionKey_With_OneLevel()
        {
            var parameter = "1";
            var answer = "1";

            var result = DataCreator.CreatePartitionKey(parameter);

            Assert.AreEqual(result, answer);
        }

        [TestMethod]
        public void CreatePartitionKey_With_ThreeLevels_ShouldReturnOneLevels()
        {
            var parameter = "1-2-3";
            var answer = "1";

            var result = DataCreator.CreatePartitionKey(parameter);

            Assert.AreEqual(result, answer);
        }
    }
}