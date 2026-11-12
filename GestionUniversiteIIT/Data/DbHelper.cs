using System.Data;
using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Data;

public static class DbHelper
{
    private const int CommandTimeoutSeconds = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(8);
    private static readonly object RetryLock = new();
    private static DateTime _retryAfterUtc = DateTime.MinValue;

    public static DataTable Query(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        return WithConnectionRetry(() =>
        {
            using var connection = Connexion.Create();
            using var command = CreateCommand(sql, connection, parameters);
            using var adapter = new SqlDataAdapter(command);
            var result = new DataTable();
            adapter.Fill(result);
            return result;
        });
    }

    public static int Execute(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        return WithConnectionRetry(() =>
        {
            using var connection = Connexion.Create();
            using var command = CreateCommand(sql, connection, parameters);
            connection.Open();
            return command.ExecuteNonQuery();
        });
    }

    public static object? Scalar(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        return WithConnectionRetry(() =>
        {
            using var connection = Connexion.Create();
            using var command = CreateCommand(sql, connection, parameters);
            connection.Open();
            var value = command.ExecuteScalar();
            return value is DBNull ? null : value;
        });
    }

    private static SqlCommand CreateCommand(string sql, SqlConnection connection,
        IReadOnlyDictionary<string, object?>? parameters)
    {
        var command = new SqlCommand(sql, connection) { CommandTimeout = CommandTimeoutSeconds };
        AddParameters(command, parameters);
        return command;
    }

    private static T WithConnectionRetry<T>(Func<T> operation)
    {
        lock (RetryLock)
        {
            if (DateTime.UtcNow < _retryAfterUtc)
            {
                throw new InvalidOperationException("SQL Server ne répond pas. Une nouvelle tentative sera faite automatiquement dans quelques secondes.");
            }
        }

        try
        {
            T result = operation();
            lock (RetryLock)
            {
                _retryAfterUtc = DateTime.MinValue;
            }
            Connexion.NotifierStatut(true, $"Connecté ({new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Connexion.ConnectionString).InitialCatalog})");
            return result;
        }
        catch (SqlException exception) when (IsConnectionFailure(exception))
        {
            lock (RetryLock)
            {
                _retryAfterUtc = DateTime.UtcNow.Add(RetryDelay);
            }
            Connexion.NotifierStatut(false, $"Déconnecté : {exception.Message}");
            throw;
        }
    }

    public static void ReinitialiserDelaiReessai()
    {
        lock (RetryLock)
        {
            _retryAfterUtc = DateTime.MinValue;
        }
    }

    private static bool IsConnectionFailure(SqlException exception)
    {
        int[] connectionErrors = [-2, 2, 17, 26, 40, 53, 64, 233, 258, 4060, 10053, 10054, 10060, 11001];
        return exception.Errors.Cast<SqlError>().Any(error => connectionErrors.Contains(error.Number));
    }

    public static void AddParameters(SqlCommand command, IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
        }
    }
}
