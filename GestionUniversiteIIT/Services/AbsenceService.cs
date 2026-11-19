using System.Data;
using GestionUniversiteIIT.Data;
using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Services;

public sealed class AbsenceService
{
    private static DataTable? _feuillePresenceSecours;
    private static DataTable? _historiqueSecours;

    public static void ReinitialiserDonneesSecours()
    {
        _feuillePresenceSecours = null;
        _historiqueSecours = null;
    }

    public DataTable Historique(DateTime debut, DateTime fin, string etudiantFiltre = "")
    {
        try
        {
            string filtre = etudiantFiltre.Trim();
            bool estNiveau = filtre is "L1" or "L2" or "L3";

            var table = DbHelper.Query(
                """
                SELECT
                    a.AbsenceId,
                    FORMAT(a.DateCours, 'dd/MM/yyyy') AS [Date],
                    e.Nom + N' ' + e.Prenom AS [Étudiant],
                    e.Niveau,
                    CASE
                        WHEN c.Code = N'GL101' THEN N'Prog. C#'
                        WHEN c.Code = N'GL102' THEN N'BD (SQL)'
                        WHEN c.Code = N'MT101' THEN N'Marketing'
                        ELSE c.Libelle
                    END AS Cours,
                    CASE WHEN a.Present = 1 THEN N'Présent' ELSE N'Absent' END AS Statut
                FROM dbo.Absences a
                JOIN dbo.Etudiants e ON e.EtudiantId = a.EtudiantId
                JOIN dbo.Cours c ON c.CoursId = a.CoursId
                WHERE a.Present = 0
                  AND a.DateCours >= @Debut
                  AND a.DateCours < DATEADD(day, 1, @Fin)
                  AND (
                        @Recherche = N'' OR @Recherche = N'Tous'
                        OR (@EstNiveau = 1 AND e.Niveau = @Recherche)
                        OR (@EstNiveau = 0 AND ((e.Nom + N' ' + e.Prenom) LIKE @Filtre OR e.Matricule LIKE @Filtre))
                  )
                ORDER BY a.DateCours DESC, e.Nom
                """,
                new Dictionary<string, object?>
                {
                    ["@Debut"] = debut.Date,
                    ["@Fin"] = fin.Date,
                    ["@Recherche"] = filtre,
                    ["@EstNiveau"] = estNiveau ? 1 : 0,
                    ["@Filtre"] = $"%{filtre}%"
                });

            return table;
        }
        catch
        {
            // Fallback maquette uniquement si SQL Server est indisponible
        }

        var secours = ObtenirHistoriqueSecours();
        var resultat = secours.Clone();
        foreach (DataRow row in secours.Rows)
        {
            string etu = Convert.ToString(row["Étudiant"]) ?? "";
            if (!string.IsNullOrWhiteSpace(etudiantFiltre) &&
                !string.Equals(etudiantFiltre.Trim(), "Tous", StringComparison.OrdinalIgnoreCase) &&
                !etu.Contains(etudiantFiltre.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (DateTime.TryParse(Convert.ToString(row["Date"]), out var d) &&
                (d.Date < debut.Date || d.Date > fin.Date))
            {
                continue;
            }

            resultat.ImportRow(row);
        }

        return resultat;
    }

    public DataTable EtudiantsDuCours(int coursId, DateTime date, string? filiere = null, string? niveau = null)
    {
        try
        {
            string? filNorm = string.IsNullOrWhiteSpace(filiere) || string.Equals(filiere, "Toutes", StringComparison.OrdinalIgnoreCase)
                ? null
                : filiere.Trim();
            string? nivNorm = string.IsNullOrWhiteSpace(niveau) || string.Equals(niveau, "Tous", StringComparison.OrdinalIgnoreCase)
                ? null
                : niveau.Trim();

            var table = DbHelper.Query(
                """
                SELECT
                    e.EtudiantId,
                    e.Matricule,
                    e.Nom + N' ' + e.Prenom AS [Nom et Prénoms],
                    CAST(CASE WHEN a.Present IS NULL THEN 1 ELSE a.Present END AS bit) AS Present
                FROM dbo.Etudiants e
                JOIN dbo.Filieres f ON f.FiliereId = e.FiliereId
                LEFT JOIN dbo.Absences a ON a.CoursId = @CoursId AND a.EtudiantId = e.EtudiantId AND a.DateCours = @Date
                WHERE e.Actif = 1
                  AND (@Filiere IS NULL OR f.Nom = @Filiere)
                  AND (@Niveau IS NULL OR e.Niveau = @Niveau)
                ORDER BY e.Matricule
                """,
                new Dictionary<string, object?>
                {
                    ["@CoursId"] = coursId,
                    ["@Date"] = date.Date,
                    ["@Filiere"] = filNorm ?? (object)DBNull.Value,
                    ["@Niveau"] = nivNorm ?? (object)DBNull.Value
                });

            return table;
        }
        catch
        {
            // Fallback maquette uniquement si SQL Server est indisponible
            return ObtenirFeuilleSecours().Copy();
        }
    }

    public void EnregistrerPresence(int coursId, DateTime date, DataTable etudiants, string? libelleCours = null)
    {
        try
        {
            using var connection = Connexion.Create();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new SqlCommand(
                    "UPDATE dbo.Absences SET Present = @Present WHERE CoursId = @CoursId AND EtudiantId = @EtudiantId AND DateCours = @Date; IF @@ROWCOUNT = 0 INSERT dbo.Absences(CoursId, EtudiantId, DateCours, Present) VALUES(@CoursId, @EtudiantId, @Date, @Present);",
                    connection, transaction);
                command.CommandTimeout = 5;
                command.Parameters.Add("@Present", SqlDbType.Bit);
                command.Parameters.Add("@CoursId", SqlDbType.Int).Value = coursId;
                command.Parameters.Add("@EtudiantId", SqlDbType.Int);
                command.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                foreach (DataRow row in etudiants.Rows)
                {
                    command.Parameters["@Present"].Value = Convert.ToBoolean(row["Present"]);
                    command.Parameters["@EtudiantId"].Value = Convert.ToInt32(row["EtudiantId"]);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch
        {
            _feuillePresenceSecours = etudiants.Copy();
            var hist = ObtenirHistoriqueSecours();
            string dateStr = date.ToString("dd/MM/yyyy");
            string coursCourt = coursId switch
            {
                1 => "Prog. C#",
                2 => "BD (SQL)",
                3 => "Marketing",
                4 => "Algorithmique",
                _ => string.IsNullOrWhiteSpace(libelleCours) ? "Prog. C#" : libelleCours
            };

            foreach (DataRow row in etudiants.Rows)
            {
                string nomEtu = Convert.ToString(row["Nom et Prénoms"]) ?? "";
                bool present = Convert.ToBoolean(row["Present"]);
                var existants = hist.AsEnumerable()
                    .Where(r => string.Equals(Convert.ToString(r["Date"]), dateStr, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(Convert.ToString(r["Étudiant"]), nomEtu, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(Convert.ToString(r["Cours"]), coursCourt, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (present)
                {
                    foreach (var ex in existants)
                    {
                        hist.Rows.Remove(ex);
                    }
                }
                else if (existants.Count == 0)
                {
                    int nextId = hist.Rows.Count == 0 ? 1 : hist.AsEnumerable().Max(r => Convert.ToInt32(r["AbsenceId"])) + 1;
                    var newRow = hist.NewRow();
                    newRow["AbsenceId"] = nextId;
                    newRow["Date"] = dateStr;
                    newRow["Étudiant"] = nomEtu;
                    newRow["Cours"] = coursCourt;
                    newRow["Statut"] = "Absent";
                    hist.Rows.InsertAt(newRow, 0);
                }
            }
        }
    }

    private static DataTable ObtenirFeuilleSecours()
    {
        if (_feuillePresenceSecours is not null)
        {
            return _feuillePresenceSecours;
        }

        var table = new DataTable();
        table.Columns.Add("EtudiantId", typeof(int));
        table.Columns.Add("Matricule", typeof(string));
        table.Columns.Add("Nom et Prénoms", typeof(string));
        table.Columns.Add("Present", typeof(bool));

        table.Rows.Add(1, "IIT0001", "KONE Ibrahim", true);
        table.Rows.Add(4, "IIT0004", "ZOUA Christ", false);
        table.Rows.Add(5, "IIT0005", "OUATTARA Michel", true);
        table.Rows.Add(6, "IIT0006", "KOFFI Ange", false);

        _feuillePresenceSecours = table;
        return table;
    }

    private static DataTable ObtenirHistoriqueSecours()
    {
        if (_historiqueSecours is not null)
        {
            return _historiqueSecours;
        }

        var table = new DataTable();
        table.Columns.Add("AbsenceId", typeof(int));
        table.Columns.Add("Date", typeof(string));
        table.Columns.Add("Étudiant", typeof(string));
        table.Columns.Add("Cours", typeof(string));
        table.Columns.Add("Statut", typeof(string));

        table.Rows.Add(1, "05/11/2026", "ZOUA Christ", "Prog. C#", "Absent");
        table.Rows.Add(2, "03/11/2026", "KONE Ibrahim", "BD (SQL)", "Absent");
        table.Rows.Add(3, "28/10/2026", "TAYE Emmanuel", "Algorithmique", "Absent");
        table.Rows.Add(4, "21/10/2026", "ADJA Koffi", "Marketing", "Absent");

        _historiqueSecours = table;
        return table;
    }
}
