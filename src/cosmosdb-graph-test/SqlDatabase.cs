using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace cosmosdb_graph_test
{
    public class SqlDatabase : IDatabase
    {
        private string _connectionString;
        private SqlConnection _sqlConnection;
        private SqlBulkCopy _sqlBulkCopy;

        private int _batchSize;

        private DataTable _nodes = new DataTable();
        private DataTable _edges = new DataTable();

        public SqlDatabase(string connectionString, int batchSize)
        {
            _connectionString = connectionString;
            _batchSize = batchSize;
        }

        public async Task InitializeAsync()
        {
            _sqlConnection = new SqlConnection(_connectionString);
            await _sqlConnection.OpenAsync();
            _sqlBulkCopy = new SqlBulkCopy(_sqlConnection);
        }

        public async Task InsertVertexAsync(string id, string label, Dictionary<string, object> properties, 
            string partitionKeyValue = "")
        {
            await FlushIfNeededAsync();
        }

        public async Task InsertEdgeAsync(string edgeLabel, string sourceId, string destinationId, string sourceLabel, 
            string destinationLabel, string sourcePartitionKey, string destinationPartitionKey, string edgeIdSuffix = "")
        {
            var row = _edges.NewRow();
            row["fromId"] = sourceId;
            row["toId"] = destinationId;
            _edges.Rows.Add(row);

            await FlushIfNeededAsync();
        }

        private async Task FlushIfNeededAsync()
        {
            if (_nodes.Rows.Count + _edges.Rows.Count >= _batchSize)
            {
                await FlushAsync();
            }
        }

        public async Task FlushAsync()
        {
            _sqlBulkCopy.DestinationTableName = "Asset";
            await _sqlBulkCopy.WriteToServerAsync(_nodes);
            _nodes.Clear();

            //Replace with SP
            //await _sqlBulkCopy.WriteToServerAsync(_edges);
            _edges.Clear();
        }
    }
}
