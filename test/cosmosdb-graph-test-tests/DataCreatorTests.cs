using System;
using System.Collections.Generic;
using System.Linq;
using cosmosdb_graph_test;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace cosmosdb_graph_test_tests
{
    [TestClass]
    public class DataCreatorTests
    {
        [TestMethod]
        public void ConstructorCosmosDB()
        {
            var dataCreator = new DataCreator(null, null);
        }

        [TestMethod]
        public void InitializeBulkExecutor()
        {
            var documentClient = new Mock<IDocumentClient>();
            var bulkExecutor = new Mock<IBulkExecutor>();
            string database = "testdb001";
            string collection = "testcollection001";

            EnumerableQuery<DocumentCollection> queryAnswer = new EnumerableQuery<DocumentCollection>(new List<DocumentCollection>()
            {
                new DocumentCollection() { Id = collection,
                    PartitionKey = new PartitionKeyDefinition() { Paths  = new System.Collections.ObjectModel.Collection<string>() { "/_partitionKey" } } }
            });
            documentClient.Setup(f => f.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(database), null)).Returns(queryAnswer);

            ConnectionPolicy connectionPolicy = new ConnectionPolicy() { RetryOptions = new RetryOptions() };
            documentClient.Setup(x => x.ConnectionPolicy).Returns(connectionPolicy);


            var dataCreator = new DataCreator(documentClient.Object, bulkExecutor.Object);
            var result =  dataCreator.InitializeBulkExecutorAsync(database, collection, null).GetAwaiter().GetResult();

            Assert.AreEqual(connectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests, 0);
            Assert.AreEqual(connectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds, 0);
            Assert.IsTrue(result);
        }
    }
}
