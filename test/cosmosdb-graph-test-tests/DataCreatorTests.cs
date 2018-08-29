using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cosmosdb_graph_test;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
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

            _dataCreator = new DataCreator(_db.Object, _executor.Object);
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
    }
}
