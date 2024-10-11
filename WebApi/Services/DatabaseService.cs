using MySqlConnector;

namespace WebApi.Services;

public sealed class DatabaseService : IDisposable
{
	private readonly ILogger<DatabaseService> _logger;
	private readonly MySqlConnection _connection;

	public DatabaseService(ILogger<DatabaseService> logger)
	{
		_logger = logger;

		_connection = new MySqlConnection("Server=localhost;User ID=root;Password=root;Database=work");
		_connection.Open();

		var schemaCommand = _connection.CreateCommand();
		schemaCommand.CommandText = "CREATE TABLE IF NOT EXISTS RequestedWork(Id int NOT NULL AUTO_INCREMENT, Status TEXT)";
		schemaCommand.ExecuteNonQuery();
	}

	void IDisposable.Dispose()
	{
		_connection.Dispose();
	}

	public async Task<int> InsertRequestedWorkAsync()
	{
		using var command = _connection.CreateCommand();
		command.CommandText = "INSERT INTO RequestedWork (Status) VALUES ('Requested')";
		await command.ExecuteNonQueryAsync();

		using var command2 = _connection.CreateCommand();
		command2.CommandText = "SELECT LAST_INSERT_ID()";
		return Convert.ToInt32(await command2.ExecuteScalarAsync());
	}
}
