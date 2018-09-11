using CommandLine;
using System;
using System.Diagnostics;

namespace graph_db_test
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var resultFromParsing = Parser.Default.ParseArguments<CommandLineOptions>(args);
            if (resultFromParsing.Tag != ParserResultType.Parsed)
                return;

            var result = (Parsed<CommandLineOptions>)resultFromParsing;

            var unparsedConnectionString = result.Value.ConnectionString;
            var rootNodeId = result.Value.RootNode.Trim();
            var batchSize = result.Value.BatchSize;
            var numberOfNodesOnEachLevel = result.Value.NumberOfNodesOnEachLevel;
            var numberOfTraversals = result.Value.NumberOfTraversalsToAdd;

            var database = CreateDatabase(unparsedConnectionString, batchSize);
            var dataCreator = new DataCreator(database);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            dataCreator.InitializeAsync().GetAwaiter().GetResult();
            var totalGraphElements = dataCreator.StartAsync(rootNodeId, numberOfNodesOnEachLevel, numberOfTraversals)
                .GetAwaiter().GetResult();

            stopwatch.Stop();

            Console.WriteLine($"Inserted {totalGraphElements} graph elements in {stopwatch.ElapsedMilliseconds} ms");
        }

        private static IDatabase CreateDatabase(string unparsedConnectionString, int batchSize)
        {
            var graphDbType = CommandLineUtils.ParseGraphDbType(unparsedConnectionString);

            if (graphDbType == GraphDbType.CosmosDb)
            {
                var cosmosDbConnectionString = CommandLineUtils.ParseCosmosDbConnectionString(unparsedConnectionString);
                if (!CommandLineUtils.AreCosmosDbParametersValid(cosmosDbConnectionString))
                {
                    throw new Exception("Please check Cosmos DB parameters");
                }

                return new CosmosDbDatabase(cosmosDbConnectionString, batchSize);
            }
            else if (graphDbType == GraphDbType.SqlServer)
            {
                return new SqlDatabase(unparsedConnectionString, batchSize);
            }
            else
            {
                throw new Exception("Unknown Graph Database. Please check Connection String.");
            }
        }
    }
}