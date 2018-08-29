using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    internal interface IExecutor
    {
        IBulkExecutor Initialize(IDocumentClient database, DocumentCollection documentCollection);

        Task BulkImportAsync(IEnumerable<object> documents);
    }
}
