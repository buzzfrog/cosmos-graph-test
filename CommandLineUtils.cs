using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    internal class CommandLineUtils
    {
        internal static string GetValueFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[1];
        }

        internal static string GetKeyFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[0];
        }

        internal static bool DoWeHaveAllParameters(string _apiKind, string _accountEndpoint, string _accountKey, string _database, string _collection)
        {
            // ApiKind needs to be Gremlin
            if (_apiKind.ToLower() != "gremlin")
                return false;

            if (_accountEndpoint != string.Empty && _accountKey != string.Empty &&
                _database != string.Empty && _collection != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
