using System.Data;
using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Data;

public static class DbHelper
{
    public static DataTable Query(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        using var connection = Connexion.Create();
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        using var adapter = new SqlDataAdapter(command);
        var result = new DataTable();
        adapter.Fill(result);
        return result;
    }

    public static int Execute(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        using var connection = Connexion.Create();
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        using var connection = Connexion.Create();
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        connection.Open();
        var value = command.ExecuteScalar();
        return value is DBNull ? null : value;
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
