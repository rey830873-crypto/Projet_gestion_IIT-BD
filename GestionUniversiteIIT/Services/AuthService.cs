using System.Security.Cryptography;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Models;

namespace GestionUniversiteIIT.Services;

public sealed class AuthService
{
    private const int Iterations = 210_000;
    private static readonly Dictionary<string, (byte[] Salt, byte[] Hash, string Role)> UtilisateursSecours =
        new(StringComparer.OrdinalIgnoreCase);

    public bool HasUsers()
    {
        try
        {
            return Convert.ToInt32(DbHelper.Scalar("SELECT COUNT(*) FROM dbo.Utilisateurs") ?? 0) > 0;
        }
        catch
        {
            return UtilisateursSecours.Count > 0;
        }
    }

    public Utilisateur? Authenticate(string username, string password)
    {
        string cleanUser = username.Trim();
        try
        {
            var table = DbHelper.Query(
                "SELECT UtilisateurId,NomUtilisateur,Role,SelMotDePasse,HachageMotDePasse FROM dbo.Utilisateurs WHERE NomUtilisateur=@Nom AND Actif=1",
                new Dictionary<string, object?> { ["@Nom"] = cleanUser });
            if (table.Rows.Count == 0)
            {
                return null;
            }

            var row = table.Rows[0];
            var salt = (byte[])row["SelMotDePasse"];
            var storedHash = (byte[])row["HachageMotDePasse"];
            var suppliedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(storedHash, suppliedHash)
                ? new Utilisateur(Convert.ToInt32(row["UtilisateurId"]), Convert.ToString(row["NomUtilisateur"])!, Convert.ToString(row["Role"])!)
                : null;
        }
        catch
        {
            if (UtilisateursSecours.TryGetValue(cleanUser, out var entry))
            {
                var suppliedHash = Rfc2898DeriveBytes.Pbkdf2(password, entry.Salt, Iterations, HashAlgorithmName.SHA256, entry.Hash.Length);
                return CryptographicOperations.FixedTimeEquals(entry.Hash, suppliedHash)
                    ? new Utilisateur(1, cleanUser, entry.Role)
                    : null;
            }

            return null;
        }
    }

    public void CreateInitialAdministrator(string username, string password)
    {
        if (HasUsers())
        {
            throw new InvalidOperationException("Un administrateur existe déjà.");
        }

        ChangerOuCreerMotDePasse(username, password);
    }

    public void ChangerOuCreerMotDePasse(string username, string newPassword, string role = "Administrateur")
    {
        string cleanUser = username.Trim();
        if (string.IsNullOrWhiteSpace(cleanUser) || newPassword.Length < 10)
        {
            throw new ArgumentException("Choisissez un identifiant valide et un mot de passe d'au moins 10 caractères.");
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(newPassword, salt, Iterations, HashAlgorithmName.SHA256, 32);

        UtilisateursSecours[cleanUser] = (salt, hash, role);

        try
        {
            DbHelper.Execute(
                """
                IF EXISTS (SELECT 1 FROM dbo.Utilisateurs WHERE NomUtilisateur = @Nom)
                    UPDATE dbo.Utilisateurs
                    SET SelMotDePasse = @Sel, HachageMotDePasse = @Hachage, Actif = 1
                    WHERE NomUtilisateur = @Nom;
                ELSE
                    INSERT dbo.Utilisateurs(NomUtilisateur, SelMotDePasse, HachageMotDePasse, Role, Actif)
                    VALUES(@Nom, @Sel, @Hachage, @Role, 1);
                """,
                new Dictionary<string, object?>
                {
                    ["@Nom"] = cleanUser,
                    ["@Sel"] = salt,
                    ["@Hachage"] = hash,
                    ["@Role"] = role
                });
        }
        catch
        {
            // Conservé dans UtilisateursSecours en mode hors-ligne
        }
    }
}
