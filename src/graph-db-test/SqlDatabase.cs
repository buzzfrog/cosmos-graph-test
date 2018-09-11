using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace graph_db_test
{
    public class SqlDatabase : IDatabase
    {
        private string _connectionString;
        private SqlConnection _sqlConnection;

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

            FillSchema(_nodes, "AssetType");
            FillSchema(_edges, "ChildType");
        }

        private void FillSchema(DataTable table, string typeName)
        {
            var adapter = new SqlDataAdapter($"DECLARE @tableType dbo.{typeName} SELECT * FROM @tableType", _sqlConnection);
            adapter.FillSchema(table, SchemaType.Source);
        }

        public async Task InsertVertexAsync(string id, string label,
            Dictionary<string, object> mandatoryProperties,
            Dictionary<string, object> optionalProperties, string partitionKeyValue = "")
        {
            var row = _nodes.NewRow();
            row["Id"] = id;
            row["partitionKey"] = partitionKeyValue;

            foreach (var property in mandatoryProperties)
            {
                row[property.Key] = property.Value;
            }

            if (optionalProperties.Count > 0)
            {
                row["propertiesJson"] = JsonConvert.SerializeObject(optionalProperties);
            }

            _nodes.Rows.Add(row);

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
            using (var transaction = _sqlConnection.BeginTransaction())
            {
                var insertCommand = new SqlCommand("usp_InsertGraphElements", _sqlConnection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var tvpAssetType = insertCommand.Parameters.AddWithValue("@tvpAssetType", _nodes);
                tvpAssetType.SqlDbType = SqlDbType.Structured;

                var tvpChildType = insertCommand.Parameters.AddWithValue("@tvpChildType", _edges);
                tvpChildType.SqlDbType = SqlDbType.Structured;

                await insertCommand.ExecuteNonQueryAsync();

                _nodes.Clear();
                _edges.Clear();

                transaction.Commit();
            }            
        }
    }
}
