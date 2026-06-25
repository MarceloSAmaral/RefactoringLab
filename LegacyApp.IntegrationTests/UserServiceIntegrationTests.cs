using System.Configuration;
using Testcontainers.MsSql;

namespace LegacyApp.IntegrationTests
{
    public class UserServiceIntegrationTests : IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;
        private string _connectionString = string.Empty;

        public UserServiceIntegrationTests()
        {
            _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync().ConfigureAwait(false);
            _connectionString = await ConfiguredDatabaseResource(_msSqlContainer.GetConnectionString()).ConfigureAwait(false);

            // Update the test process configuration file so ConfigurationManager sees the new value
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = (ConnectionStringsSection)config.GetSection("connectionStrings");

            // Remove existing entry if present and add the new one
            section.ConnectionStrings.Remove("appDatabase");
            section.ConnectionStrings.Add(new ConnectionStringSettings("appDatabase", _connectionString));

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

        }

        public async Task DisposeAsync()
        {
            await _msSqlContainer.StopAsync();
            await _msSqlContainer.DisposeAsync();
        }

        private static async Task<string> ConfiguredDatabaseResource(string originalConnectionString)
        {
            string databaseName = "TestDatabase";
            await PrepareDatabase(originalConnectionString, databaseName).ConfigureAwait(false);
            var newConnectionString = GetNewConnectionString(originalConnectionString, databaseName);
            return newConnectionString;
        }

        private static async Task PrepareDatabase(string connectionString, string databaseName)
        {
            await CreateDatabase(connectionString, databaseName).ConfigureAwait(false);
            var newConnectionString = GetNewConnectionString(connectionString, databaseName);
            await CreateDBObjects(newConnectionString).ConfigureAwait(false);
        }

        private static async Task CreateDatabase(string connectionString, string databaseName)
        {
            using var connection = new System.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{databaseName}]";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public static async Task CreateDBObjects(string connectionString)
        {
            List<string> sqlStatements = new();
            sqlStatements.Add(@"create procedure [dbo].[uspAddUser]
(
    @Firstname varchar(50)
    ,@Surname varchar(50)
    ,@DateOfBirth datetime
    ,@EmailAddress varchar(50)
    ,@HasCreditLimit bit
    ,@CreditLimit int
    ,@ClientId int 
)
as
return 0;");
            sqlStatements.Add(@"create procedure [dbo].[uspGetClientById]
(
    @clientId int
)
as
select @clientId as ClientId, 'VeryImportantClient' as [Name], 0 as ClientStatus;");
            using var connection = new System.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            using var command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            foreach (var sqlStatement in sqlStatements)
            {
                command.CommandText = sqlStatement;
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static string GetNewConnectionString(string originalConnectionString, string databaseName)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder connectionStringBuilder = new(originalConnectionString);
            connectionStringBuilder.InitialCatalog = databaseName;
            return connectionStringBuilder.ConnectionString;
        }

        [Fact]
        public void AddUser_WhenExecutingHappyPath_ShouldReturnTrue()
        {
            var sut = new UserService();
            var addResult = sut.AddUser("John", "Doe", "John.doe@gmail.com", new DateTime(1993, 1, 1), 4);
            Assert.True(addResult);
        }
    }
}