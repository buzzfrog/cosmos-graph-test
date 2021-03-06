using CommandLine;

namespace graph_db_test
{
    public class CommandLineOptions
    {
        [Option('c', "connection-string", Required = true, HelpText = "Connection String")]
        public string ConnectionString { get; set; }

        [Option('r', "root-name", Required = false, HelpText = "Name of root node", Default = "1")]
        public string RootNode { get; set; }

        [Option('b', "batch-size", Required = false, HelpText = "Batch size", Default = 1000)]
        public int BatchSize { get; set; }

        [Option('n', "nodes-on-each-level", Required = false, HelpText = "Number of nodes to create on each level", Default = 18)]
        public int NumberOfNodesOnEachLevel { get; set; }

        [Option('a', "additional-traversals", Required = false, HelpText = "Number of additional traversals to create", Default = 20000)]
        public int NumberOfTraversalsToAdd { get; set; }

        [Option('w', "warmup-period", Required = false, HelpText = "Warm up period in ms", Default = 0)]
        public int WarmupPeriod { get; set; }
    }
}