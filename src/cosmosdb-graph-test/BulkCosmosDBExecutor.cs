using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.Graph;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace cosmosdb_graph_test
{
    class BulkCosmosDBExecutor : IExecutor
    {
        GraphBulkExecutor _graphBulkExecutor; 
        public async Task BulkImportAsync(IEnumerable<object> documents)
        {
            var response = await _graphBulkExecutor.BulkImportAsync(documents);

            if (response.BadInputDocuments.Any())
                throw new Exception($"BulkExecutor found {response.BadInputDocuments.Count} bad input graph element(s)!");
        }

        public IBulkExecutor Initialize(IDocumentClient database, DocumentCollection documentCollection)
        {
            _graphBulkExecutor = new GraphBulkExecutor((DocumentClient) database, documentCollection);
            _graphBulkExecutor.InitializeAsync().GetAwaiter().GetResult();

            return _graphBulkExecutor;
        }
    }
}
