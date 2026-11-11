using System.Configuration;
using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Data;

public static class Connexion
{
    public static string ConnectionString =>
        ConfigurationManager.ConnectionStrings["Universite"]?.ConnectionString
        ?? throw new InvalidOperationException("La chaîne de connexion 'Universite' est absente de App.config.");

    public static SqlConnection Create()
    {
        return new SqlConnection(ConnectionString);
    }
}
