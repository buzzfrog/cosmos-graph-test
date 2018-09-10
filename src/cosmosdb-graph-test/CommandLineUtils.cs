namespace cosmosdb_graph_test
{
    public class CommandLineUtils
    {
        public static string GetValueFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[1];
        }

        public static string GetKeyFromPart(string part)
        {
            return part.Split(new[] { '=' }, 2)[0];
        }

        public static bool AreCosmosDbParametersValid((string accountEndpoint,
                         string accountKey,
                         string apiKind,
                         string database,
                         string collection) cosmosDbConnectionString)
        {
            // ApiKind needs to be Gremlin
            if (cosmosDbConnectionString.apiKind.ToLower() != "gremlin")
                return false;

            if (cosmosDbConnectionString.accountEndpoint != string.Empty && cosmosDbConnectionString.accountKey != string.Empty &&
                cosmosDbConnectionString.database != string.Empty && cosmosDbConnectionString.collection != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static (string accountEndpoint, 
                         string accountKey, 
                         string apiKind, 
                         string database, 
                         string collection) ParseCosmosDbConnectionString(string unparsedConnectionString)
        { 

            string accountEndpoint = "";
            string accountKey = "";
            string apiKind = "";
            string database = "";
            string collection = "";
        
            foreach (var part in unparsedConnectionString.Trim().Split(';'))
            {
                switch (GetKeyFromPart(part.ToLower()))
                {
                    case "accountendpoint":
                        accountEndpoint = GetValueFromPart(part);
                        break;
                    case "accountkey":
                        accountKey = GetValueFromPart(part);
                        break;
                    case "apikind":
                        apiKind = GetValueFromPart(part);
                        break;
                    case "database":
                        database = GetValueFromPart(part);
                        break;
                    case "collection":
                        collection = GetValueFromPart(part);
                        break;
                    default:
                        break;
                }
            }

            return (accountEndpoint, accountKey, apiKind, database, collection);
        }
    }
}

