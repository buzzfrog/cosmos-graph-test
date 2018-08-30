using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    internal interface IDatabase
    {
        IDocumentClient Initialize(string accountEndpoint, string accountKey);
    }
}
