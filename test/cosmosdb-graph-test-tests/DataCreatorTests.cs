using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cosmosdb_graph_test;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph.Element;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace cosmosdb_graph_test_tests
{
    [TestClass]
    public class DataCreatorTests
    {
        Mock<IDatabase> _db;
        Mock<IExecutor> _executor;
        Mock<IDocumentClient> _documentClient;
        Mock<IBulkExecutor> _bulkExecutor;
        Mock<IRandom> _random;

        string _database = "testdb001";
        string _collection = "testcollection001";
        string _partitionKey = "/_partitionKey";

        DataCreator _dataCreator;

        [TestInitialize]
        public void TestInit()
        {
            _db = new Mock<IDatabase>();
            _executor = new Mock<IExecutor>();
            _documentClient = new Mock<IDocumentClient>();
            _bulkExecutor = new Mock<IBulkExecutor>();
            _random = new Mock<IRandom>();

            _random.Setup(x => x.Next(It.IsAny<int>())).Returns((int i) => new Random().Next(i));

            EnumerableQuery<DocumentCollection> queryAnswer = new EnumerableQuery<DocumentCollection>(new List<DocumentCollection>()
            {
                new DocumentCollection() { Id = _collection,
                    PartitionKey = new PartitionKeyDefinition() { Paths  = new System.Collections.ObjectModel.Collection<string>() { _partitionKey } } }
            });
            _documentClient.Setup(x => x.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(_database), null)).Returns(queryAnswer);
            ConnectionPolicy connectionPolicy = new ConnectionPolicy() { RetryOptions = new RetryOptions() };
            _documentClient.Setup(x => x.ConnectionPolicy).Returns(connectionPolicy);
            _db.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>())).Returns(_documentClient.Object);

            _executor.Setup(x => x.Initialize(It.IsAny<IDocumentClient>(), It.IsAny<DocumentCollection>())).Returns(_bulkExecutor.Object);

            _dataCreator = new DataCreator(_db.Object, _executor.Object, _random.Object);
        }

        [TestMethod]
        public void InitializeBulkExecutor_With_CorrectParameters()
        {

            _dataCreator.InitializeAsync(null, null, _database, _collection).GetAwaiter().GetResult();

            _executor.Verify(x => x.Initialize(It.IsAny<IDocumentClient>(), It.IsAny<DocumentCollection>()), Times.Once);
        }

        [TestMethod]
        public void Start_And_Insert_XXX_documents()
        {
            var rootNodeId = "1";
            var batchSize = 100;
            var numberOfNodesOnEachLevel = 4;
            var numberOfTraversals = 100;

            _dataCreator.InitializeAsync(null, null, _database, _collection).GetAwaiter().GetResult();

            var result = _dataCreator.StartAsync(rootNodeId, batchSize, numberOfNodesOnEachLevel, numberOfTraversals).GetAwaiter().GetResult();

            Assert.AreEqual(result, 3729);
        }

        [TestMethod]
        public void Verify_That_Six_Levels_Are_Created()
        {
            var rootNodeId = "1";
            var batchSize = 100;
            var numberOfNodesOnEachLevel = 4;
            var numberOfTraversals = 100;

            // override default construction
            var fakeExecutor = new FakeExecutor();
            _dataCreator = new DataCreator(_db.Object, fakeExecutor, _random.Object);

            _dataCreator.InitializeAsync(null, null, _database, _collection).GetAwaiter().GetResult();

            var result = _dataCreator.StartAsync(rootNodeId, batchSize, numberOfNodesOnEachLevel, numberOfTraversals).GetAwaiter().GetResult();

            var vertex_of_level_6 = (GremlinVertex) (from v in fakeExecutor.Documents
                                     where v is GremlinVertex && ((GremlinVertex)v).GetVertexProperties("level").Any(k => (int)k.Value == 6)
                                     select v).First();

            Assert.IsTrue(vertex_of_level_6.GetVertexProperties().Any(p => p.Key == "manufacturer"));

            Assert.AreEqual(fakeExecutor.Documents.Count, 3729);
        }

    }


    class FakeExecutor : IExecutor
    {
        public List<object> Documents =  new List<object>();

        public Task BulkImportAsync(IEnumerable<object> documents)
        {
            foreach (var d in documents)
            {
                Documents.Add(d);
            }
            return Task.CompletedTask;
        }

        public IBulkExecutor Initialize(IDocumentClient database, DocumentCollection documentCollection)
        {
            return null;
        }
    }
}
