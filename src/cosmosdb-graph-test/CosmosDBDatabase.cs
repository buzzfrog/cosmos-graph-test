using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace cosmosdb_graph_test
{
    class CosmosDBDatabase : IDatabase
    {
        public IDocumentClient Initialize(string accountEndpoint, string accountKey)
        {
             var connectionPolicy = new ConnectionPolicy
             {
                 ConnectionMode = ConnectionMode.Direct,
                 ConnectionProtocol = Protocol.Tcp
             };

            return new DocumentClient(new Uri(accountEndpoint), accountKey, connectionPolicy);

        }
    }
}
