using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace sql_db_functional_test
{
    [TestClass]
    public class SqlDatabaseFunctionalTest
    {
        [TestMethod]
        public async Task DoDatabaseCheckAsync()
        {
            var connectionString = "Server=tcp:localhost,1433;Initial Catalog=master;Persist Security Info=False;User ID=sa;Password=<YourStrong!Passw0rd>;MultipleActiveResultSets=False;Connection Timeout=30;";
            var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();

            var nodesCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.Asset WHERE partitionKey = 1", sqlConnection);
            var nodes = (int)await nodesCommand.ExecuteScalarAsync();
            Assert.AreEqual(3916, nodes);

            var edgesCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.Child", sqlConnection);
            var edges = (int)await edgesCommand.ExecuteScalarAsync();
            Assert.AreEqual(8905, edges);
        }
    }
}
