using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
     class DataCreator
    {
        private IDocumentClient _documentClient;
        private IBulkExecutor _bulkExecutor;

        public DataCreator(IDocumentClient documentClient, IBulkExecutor bulkExecutor)
        {
            this._documentClient = documentClient;
            this._bulkExecutor = bulkExecutor;
        }

        public async Task<bool> InitializeBulkExecutorAsync(string Database, string Collection, string PartitionKey)
        {
            try
            {
                var documentCollection = _documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(Database), null);
             
                var dataCollection = documentCollection.Where(c => c.Id == Collection).AsEnumerable().FirstOrDefault();

                PartitionKey = dataCollection.PartitionKey.Paths.First().Replace("/", string.Empty);

                // Set retry options high during initialization (default values).
                _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
                _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

                await _bulkExecutor.InitializeAsync();

                // Set retries to 0 to pass complete control to bulk executor.
                _documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
                _documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
            }
            catch
            {
                return false;

            }

            return true;
        }
    }
}
