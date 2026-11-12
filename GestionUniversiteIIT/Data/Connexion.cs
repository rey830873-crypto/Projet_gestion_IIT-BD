using System.Configuration;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Data;

public static class Connexion
{
    private static string? _connectionStringOverride;

    public static bool? EstConnecte { get; private set; }
    public static string DernierMessageStatut { get; private set; } = "Vérification en cours...";

    public static event Action<bool, string>? StatusChanged;

    public static string ConnectionString
    {
        get => _connectionStringOverride
            ?? ConfigurationManager.ConnectionStrings["Universite"]?.ConnectionString
            ?? throw new InvalidOperationException("La chaîne de connexion 'Universite' est absente de App.config.");
        set => _connectionStringOverride = value;
    }

    public static SqlConnection Create() => Create(ConnectionString);

    public static SqlConnection Create(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (builder.ConnectTimeout < 10)
        {
            builder.ConnectTimeout = 10;
        }
        return new SqlConnection(builder.ConnectionString);
    }

    public static string ConstruireChaine(
        string serveur,
        string baseDeDonnees,
        bool authWindows,
        string? utilisateurSql,
        string? motDePasseSql,
        bool encrypt = true,
        bool trustServerCertificate = true,
        int timeoutSeconds = 15)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = serveur.Trim(),
            InitialCatalog = baseDeDonnees.Trim(),
            IntegratedSecurity = authWindows,
            Encrypt = encrypt,
            TrustServerCertificate = trustServerCertificate,
            ConnectTimeout = Math.Max(5, timeoutSeconds)
        };

        if (!authWindows)
        {
            builder.UserID = utilisateurSql?.Trim() ?? "";
            builder.Password = motDePasseSql ?? "";
        }

        return builder.ConnectionString;
    }

    public static void NotifierStatut(bool connecte, string message)
    {
        bool changed = EstConnecte != connecte || !string.Equals(DernierMessageStatut, message, StringComparison.Ordinal);
        EstConnecte = connecte;
        DernierMessageStatut = message;
        if (changed)
        {
            StatusChanged?.Invoke(connecte, message);
        }
    }

    public static (bool Succes, string Message, string VersionServeur, long LatenceMs) TesterConnexion(string? connectionString = null)
    {
        string target = string.IsNullOrWhiteSpace(connectionString) ? ConnectionString : connectionString;
        var sw = Stopwatch.StartNew();
        try
        {
            using var conn = Create(target);
            conn.Open();
            sw.Stop();
            string version = conn.ServerVersion;
            string db = conn.Database;
            string msg = $"Connecté à {db} ({sw.ElapsedMilliseconds} ms)";
            if (string.IsNullOrWhiteSpace(connectionString) || string.Equals(connectionString, ConnectionString, StringComparison.Ordinal))
            {
                NotifierStatut(true, msg);
            }
            return (true, msg, version, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            string msg = $"Déconnecté : {ex.Message}";
            if (string.IsNullOrWhiteSpace(connectionString) || string.Equals(connectionString, ConnectionString, StringComparison.Ordinal))
            {
                NotifierStatut(false, msg);
            }
            return (false, msg, string.Empty, sw.ElapsedMilliseconds);
        }
    }

    public static void SauvegarderEtAppliquer(
        string serveur,
        string baseDeDonnees,
        bool authWindows,
        string? utilisateurSql,
        string? motDePasseSql,
        bool encrypt,
        bool trustServerCertificate)
    {
        string newConnStr = ConstruireChaine(serveur, baseDeDonnees, authWindows, utilisateurSql, motDePasseSql, encrypt, trustServerCertificate);

        // Appliquer immédiatement en mémoire
        ConnectionString = newConnStr;

        // Sauvegarder dans le fichier de configuration de l'exécutable (.dll.config)
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var section = config.ConnectionStrings;
        if (section.ConnectionStrings["Universite"] is null)
        {
            section.ConnectionStrings.Add(new ConnectionStringSettings("Universite", newConnStr, "Microsoft.Data.SqlClient"));
        }
        else
        {
            section.ConnectionStrings["Universite"].ConnectionString = newConnStr;
            section.ConnectionStrings["Universite"].ProviderName = "Microsoft.Data.SqlClient";
        }
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("connectionStrings");

        // Mettre aussi à jour App.config source si présent (utile en développement)
        string projectAppConfig = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "App.config"));
        if (File.Exists(projectAppConfig))
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(projectAppConfig);
                var addNode = doc.Root?
                    .Element("connectionStrings")?
                    .Elements("add")
                    .FirstOrDefault(e => string.Equals((string?)e.Attribute("name"), "Universite", StringComparison.OrdinalIgnoreCase));
                if (addNode is not null)
                {
                    addNode.SetAttributeValue("connectionString", newConnStr);
                    doc.Save(projectAppConfig);
                }
            }
            catch
            {
                // Ignorer si App.config source est en lecture seule
            }
        }
    }
}
