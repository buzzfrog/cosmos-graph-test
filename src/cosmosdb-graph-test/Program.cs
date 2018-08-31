using CommandLine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    class Program
    {
        private static string _unparsedConnectionString;
        private static string _rootNodeId;
        private static int _batchSize;
        private static int _numberOfNodesOnEachLevel;
        private static int _numberOfTraversals;
        private static int _warmupPeriod;

        private static string _accountEndpoint;
        private static string _accountKey;
        private static string _apiKind;
        private static string _database;
        private static string _collection;

        private static void Main(string[] args)
        {

            var resultFromParsing = Parser.Default.ParseArguments<CommandLineOptions>(args);
            if (resultFromParsing.Tag != ParserResultType.Parsed)
                return;

            var result = (Parsed<CommandLineOptions>)resultFromParsing;

            _unparsedConnectionString = result.Value.ConnectionString;
            _rootNodeId = result.Value.RootNode.Trim();
            _batchSize = result.Value.BatchSize;
            _numberOfNodesOnEachLevel = result.Value.NumberOfNodesOnEachLevel;
            _numberOfTraversals = result.Value.NumberOfTraversalsToAdd;
            _warmupPeriod = result.Value.WarmupPeriod;

            Console.WriteLine($"Warmup Period: {_warmupPeriod} ms");
            Task.Delay(_warmupPeriod).GetAwaiter().GetResult();

            (_accountEndpoint, _accountKey, _apiKind, _database, _collection) = CommandLineUtils.ParseConnectionString(_unparsedConnectionString);

            if (CommandLineUtils.DoWeHaveAllParameters(_apiKind, _accountEndpoint, _accountKey, _database, _collection))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var dataCreator = new DataCreator(new CosmosDBDatabase(), new BulkCosmosDBExecutor(), new SpecialRandom());
                dataCreator.InitializeAsync(_accountEndpoint, _accountKey, _database, _collection).GetAwaiter().GetResult();

                var totalElementsInserted = dataCreator.StartAsync(_rootNodeId, _batchSize, _numberOfNodesOnEachLevel, _numberOfTraversals).GetAwaiter().GetResult();

                stopwatch.Stop();

                Console.WriteLine($"Added {totalElementsInserted} graph elements in {stopwatch.ElapsedMilliseconds} ms");
            }
            else
            {
                Console.WriteLine("Check the parameters");
            }

        }
    }
}