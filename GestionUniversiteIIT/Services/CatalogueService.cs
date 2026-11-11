using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public enum TypeChamp
{
    Texte,
    Nombre,
    Date,
    Heure,
    OuiNon,
    Liste
}

public sealed record ChampCatalogue(
    string Nom,
    string Libelle,
    TypeChamp Type = TypeChamp.Texte,
    bool Obligatoire = true,
    string? RequeteListe = null);

public sealed record DefinitionCatalogue(
    string Cle,
    string Titre,
    string PlaceholderRecherche,
    string LibelleBoutonNouveau,
    string LibelleTotal,
    int TotalMaquette,
    int DernierePageMaquette,
    string RequeteListe,
    string RequeteEdition,
    string RequeteAjout,
    string RequeteModification,
    string RequeteSuppression,
    string ColonneId,
    IReadOnlyList<ChampCatalogue> Champs);

public sealed class CatalogueService
{
    private static readonly Dictionary<string, DataTable> CacheSecours = new(StringComparer.Ordinal);
    private static DataTable? _filieresSecours;

    public static void ReinitialiserDonneesSecours()
    {
        CacheSecours.Clear();
        _filieresSecours = null;
    }

    public static DataTable ObtenirFilieresSecours()
    {
        if (_filieresSecours is not null)
        {
            return _filieresSecours;
        }

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Libelle", typeof(string));
        table.Rows.Add(1, "Génie Logiciel");
        table.Rows.Add(2, "Marketing");
        table.Rows.Add(3, "Informatique");
        table.Rows.Add(4, "Réseaux");
        _filieresSecours = table;
        return table;
    }

    public DataTable ListerFilieres()
    {
        try
        {
            var table = DbHelper.Query("SELECT FiliereId AS Id, Nom AS Libelle FROM dbo.Filieres ORDER BY FiliereId");
            if (table.Rows.Count > 0)
            {
                return table;
            }
        }
        catch
        {
            // Mode secours
        }

        return ObtenirFilieresSecours().Copy();
    }

    public void AjouterFiliere(string nom)
    {
        string clean = nom.Trim();
        if (string.IsNullOrWhiteSpace(clean))
        {
            throw new ArgumentException("Le nom de la filière est obligatoire.");
        }

        try
        {
            DbHelper.Execute(
                "IF NOT EXISTS (SELECT 1 FROM dbo.Filieres WHERE Nom = @Nom) INSERT dbo.Filieres(Nom) VALUES(@Nom)",
                new Dictionary<string, object?> { ["@Nom"] = clean });
        }
        catch
        {
            // Fallback mémoire
        }

        var table = ObtenirFilieresSecours();
        bool existe = table.AsEnumerable()
            .Any(r => string.Equals(Convert.ToString(r["Libelle"]), clean, StringComparison.OrdinalIgnoreCase));
        if (!existe)
        {
            int nextId = table.Rows.Count == 0 ? 1 : table.AsEnumerable().Max(r => Convert.ToInt32(r["Id"])) + 1;
            table.Rows.Add(nextId, clean);
        }
    }

    public void SupprimerFiliere(int filiereId)
    {
        try
        {
            DbHelper.Execute("DELETE dbo.Filieres WHERE FiliereId = @Id", new Dictionary<string, object?> { ["@Id"] = filiereId });
        }
        catch
        {
            // Fallback mémoire
        }

        var table = ObtenirFilieresSecours();
        foreach (var row in table.Select($"Id = {filiereId}"))
        {
            table.Rows.Remove(row);
        }
    }

    public static string ResoudreNomFiliere(object? filId)
    {
        if (filId is not null and not DBNull && int.TryParse(Convert.ToString(filId), out int id))
        {
            var match = ObtenirFilieresSecours().Select($"Id = {id}").FirstOrDefault();
            if (match is not null)
            {
                return Convert.ToString(match["Libelle"]) ?? "Génie Logiciel";
            }
        }

        return Convert.ToString(filId) switch
        {
            "2" => "Marketing",
            "3" => "Informatique",
            "4" => "Réseaux",
            _ => "Génie Logiciel"
        };
    }

    public static string ResoudreNomSalle(object? salleId) => Convert.ToString(salleId) switch
    {
        "2" => "A2",
        "3" => "B1",
        "4" => "Lab Info",
        _ => "A1"
    };

    public static (string Code, string LibelleCourt) ResoudreCoursSecours(object? coursId) => Convert.ToString(coursId) switch
    {
        "2" => ("GL102", "BD (SQL)"),
        "3" => ("MT101", "Marketing de base"),
        "4" => ("INF101", "Algo."),
        "5" => ("RES101", "Réseaux"),
        "6" => ("GL103", "Poo C#"),
        _ => ("GL101", "Prog. C#")
    };

    public static int ResoudreEnseignantDuCours(int coursId) => coursId switch
    {
        1 or 2 or 4 or 6 => 1,
        3 => 2,
        5 => 3,
        _ => 1
    };

    public static int ResoudreEnseignantParCodeCours(string code) => code.ToUpperInvariant() switch
    {
        "GL101" or "GL102" or "INF101" or "GL103" => 1,
        "MT101" => 2,
        "RES101" => 3,
        _ => 1
    };

    private static readonly IReadOnlyDictionary<string, DefinitionCatalogue> Definitions =
        new Dictionary<string, DefinitionCatalogue>(StringComparer.Ordinal)
        {
            ["Etudiants"] = new(
                "Etudiants",
                "Gestion des Étudiants",
                "Rechercher un étudiant...",
                "+  Nouveau",
                "étudiants",
                1248,
                25,
                "SELECT e.EtudiantId, e.Matricule, e.Nom + N' ' + e.Prenom AS [Nom et Prénoms], f.Nom AS [Filière], e.Niveau, CASE WHEN e.Actif = 1 THEN N'Actif' ELSE N'Inactif' END AS Statut FROM dbo.Etudiants e JOIN dbo.Filieres f ON f.FiliereId = e.FiliereId ORDER BY e.Matricule",
                "SELECT Matricule, Nom, Prenom, FiliereId, Niveau, DateNaissance, Sexe, Actif FROM dbo.Etudiants WHERE EtudiantId = @Id",
                "INSERT dbo.Etudiants(Matricule, Nom, Prenom, FiliereId, Niveau, DateNaissance, Sexe, Actif) VALUES(@Matricule, @Nom, @Prenom, @FiliereId, @Niveau, @DateNaissance, @Sexe, @Actif)",
                "UPDATE dbo.Etudiants SET Matricule = @Matricule, Nom = @Nom, Prenom = @Prenom, FiliereId = @FiliereId, Niveau = @Niveau, DateNaissance = @DateNaissance, Sexe = @Sexe, Actif = @Actif WHERE EtudiantId = @Id",
                "DELETE dbo.Etudiants WHERE EtudiantId = @Id",
                "EtudiantId",
                new[]
                {
                    new ChampCatalogue("Matricule", "Matricule"),
                    new ChampCatalogue("Nom", "Nom"),
                    new ChampCatalogue("Prenom", "Prénoms"),
                    new ChampCatalogue("FiliereId", "Filière", TypeChamp.Liste, true, "SELECT FiliereId AS Id, Nom AS Libelle FROM dbo.Filieres ORDER BY Nom"),
                    new ChampCatalogue("Niveau", "Niveau", TypeChamp.Liste, true, "SELECT Valeur AS Id, Valeur AS Libelle FROM (VALUES(N'L1'),(N'L2'),(N'L3'),(N'M1'),(N'M2')) v(Valeur)"),
                    new ChampCatalogue("DateNaissance", "Date de naissance", TypeChamp.Date, false),
                    new ChampCatalogue("Sexe", "Sexe", TypeChamp.Liste, false, "SELECT Valeur AS Id, Valeur AS Libelle FROM (VALUES(N'M'),(N'F')) v(Valeur)"),
                    new ChampCatalogue("Actif", "Actif", TypeChamp.OuiNon)
                }),
            ["Enseignants"] = new(
                "Enseignants",
                "Gestion des enseignants",
                "Rechercher un enseignant...",
                "+  Nouveau",
                "enseignants",
                68,
                7,
                "SELECT EnseignantId, Matricule, Nom + N' ' + Prenom AS [Nom et Prénoms], Specialite AS [Spécialité], ISNULL(Telephone, N'') AS Contact FROM dbo.Enseignants ORDER BY Matricule",
                "SELECT Matricule, Nom, Prenom, Specialite, Email, Telephone, Actif FROM dbo.Enseignants WHERE EnseignantId = @Id",
                "INSERT dbo.Enseignants(Matricule, Nom, Prenom, Specialite, Email, Telephone, Actif) VALUES(@Matricule, @Nom, @Prenom, @Specialite, @Email, @Telephone, @Actif)",
                "UPDATE dbo.Enseignants SET Matricule = @Matricule, Nom = @Nom, Prenom = @Prenom, Specialite = @Specialite, Email = @Email, Telephone = @Telephone, Actif = @Actif WHERE EnseignantId = @Id",
                "DELETE dbo.Enseignants WHERE EnseignantId = @Id",
                "EnseignantId",
                new[]
                {
                    new ChampCatalogue("Matricule", "Matricule"),
                    new ChampCatalogue("Nom", "Nom"),
                    new ChampCatalogue("Prenom", "Prénoms"),
                    new ChampCatalogue("Specialite", "Spécialité"),
                    new ChampCatalogue("Email", "Courriel", Obligatoire: false),
                    new ChampCatalogue("Telephone", "Contact", Obligatoire: false),
                    new ChampCatalogue("Actif", "Actif", TypeChamp.OuiNon)
                }),
            ["Cours"] = new(
                "Cours",
                "Gestion des cours",
                "Rechercher un cours...",
                "+  Nouveau",
                "cours",
                37,
                10,
                "SELECT c.CoursId, c.Code, c.Libelle AS [Libellé du cours], f.Nom AS [Filière], c.Credits AS [Crédit] FROM dbo.Cours c JOIN dbo.Filieres f ON f.FiliereId = c.FiliereId ORDER BY c.Code",
                "SELECT Code, Libelle, FiliereId, EnseignantId, Credits FROM dbo.Cours WHERE CoursId = @Id",
                "INSERT dbo.Cours(Code, Libelle, FiliereId, EnseignantId, Credits) VALUES(@Code, @Libelle, @FiliereId, @EnseignantId, @Credits)",
                "UPDATE dbo.Cours SET Code = @Code, Libelle = @Libelle, FiliereId = @FiliereId, EnseignantId = @EnseignantId, Credits = @Credits WHERE CoursId = @Id",
                "DELETE dbo.Cours WHERE CoursId = @Id",
                "CoursId",
                new[]
                {
                    new ChampCatalogue("Code", "Code"),
                    new ChampCatalogue("Libelle", "Libellé du cours"),
                    new ChampCatalogue("FiliereId", "Filière", TypeChamp.Liste, true, "SELECT FiliereId AS Id, Nom AS Libelle FROM dbo.Filieres ORDER BY Nom"),
                    new ChampCatalogue("EnseignantId", "Enseignant", TypeChamp.Liste, false, "SELECT EnseignantId AS Id, Nom + N' ' + Prenom AS Libelle FROM dbo.Enseignants WHERE Actif = 1 ORDER BY Nom"),
                    new ChampCatalogue("Credits", "Crédit", TypeChamp.Nombre)
                }),
            ["Salles"] = new(
                "Salles",
                "Gestion des salles",
                "Rechercher une salle...",
                "+  Nouvelle",
                "salles",
                4,
                1,
                "SELECT SalleId, Code AS Salle, Capacite AS [Capacité], [Type], CASE WHEN Active = 1 THEN N'Disponible' ELSE N'Occupée' END AS Statut FROM dbo.Salles ORDER BY SalleId",
                "SELECT Code, Capacite, [Type], Active FROM dbo.Salles WHERE SalleId = @Id",
                "INSERT dbo.Salles(Code, Capacite, [Type], Active) VALUES(@Code, @Capacite, @Type, @Active)",
                "UPDATE dbo.Salles SET Code = @Code, Capacite = @Capacite, [Type] = @Type, Active = @Active WHERE SalleId = @Id",
                "DELETE dbo.Salles WHERE SalleId = @Id",
                "SalleId",
                new[]
                {
                    new ChampCatalogue("Code", "Salle"),
                    new ChampCatalogue("Capacite", "Capacité", TypeChamp.Nombre),
                    new ChampCatalogue("Type", "Type", TypeChamp.Liste, true, "SELECT Valeur AS Id, Valeur AS Libelle FROM (VALUES(N'Cours'),(N'TP'),(N'Laboratoire'),(N'Amphithéâtre')) v(Valeur)"),
                    new ChampCatalogue("Active", "Disponible", TypeChamp.OuiNon)
                }),
            ["Inscriptions"] = new(
                "Inscriptions",
                "Inscriptions",
                "Rechercher une inscription...",
                "+  Nouveau",
                "inscriptions",
                1226,
                62,
                "SELECT i.InscriptionId, ROW_NUMBER() OVER (ORDER BY i.InscriptionId) AS [N°], e.Nom + N' ' + e.Prenom AS [Étudiant], f.Nom AS [Filière], e.Niveau, FORMAT(i.DateInscription, 'dd/MM/yyyy') AS [Date inscription], i.AnneeAcademique FROM dbo.Inscriptions i JOIN dbo.Etudiants e ON e.EtudiantId = i.EtudiantId JOIN dbo.Filieres f ON f.FiliereId = e.FiliereId ORDER BY i.InscriptionId",
                "SELECT EtudiantId, AnneeAcademique, DateInscription FROM dbo.Inscriptions WHERE InscriptionId = @Id",
                "INSERT dbo.Inscriptions(EtudiantId, AnneeAcademique, DateInscription) VALUES(@EtudiantId, @AnneeAcademique, @DateInscription)",
                "UPDATE dbo.Inscriptions SET EtudiantId = @EtudiantId, AnneeAcademique = @AnneeAcademique, DateInscription = @DateInscription WHERE InscriptionId = @Id",
                "DELETE dbo.Inscriptions WHERE InscriptionId = @Id",
                "InscriptionId",
                new[]
                {
                    new ChampCatalogue("EtudiantId", "Étudiant", TypeChamp.Liste, true, "SELECT EtudiantId AS Id, Matricule + N' — ' + Nom + N' ' + Prenom AS Libelle FROM dbo.Etudiants WHERE Actif = 1 ORDER BY Nom"),
                    new ChampCatalogue("AnneeAcademique", "Année académique", TypeChamp.Liste, true, "SELECT Valeur AS Id, Valeur AS Libelle FROM (VALUES(N'2026 - 2027'),(N'2025 - 2026'),(N'2024 - 2025')) v(Valeur)"),
                    new ChampCatalogue("DateInscription", "Date d'inscription", TypeChamp.Date)
                }),
            ["Planning"] = new(
                "Planning",
                "Planification des Cours",
                "Rechercher un créneau...",
                "+  Nouvelle planification",
                "créneaux",
                5,
                1,
                "SELECT p.PlanningId, c.Code, c.Libelle AS Cours, f.Nom AS [Filière], p.Niveau, p.JourSemaine, p.HeureDebut, p.HeureFin, s.Code AS Salle FROM dbo.Plannings p JOIN dbo.Cours c ON c.CoursId = p.CoursId JOIN dbo.Filieres f ON f.FiliereId = p.FiliereId JOIN dbo.Salles s ON s.SalleId = p.SalleId ORDER BY p.HeureDebut, p.JourSemaine",
                "SELECT CoursId, SalleId, JourSemaine, HeureDebut, HeureFin, FiliereId, Niveau FROM dbo.Plannings WHERE PlanningId = @Id",
                "INSERT dbo.Plannings(CoursId, SalleId, JourSemaine, HeureDebut, HeureFin, FiliereId, Niveau) VALUES(@CoursId, @SalleId, @JourSemaine, @HeureDebut, @HeureFin, @FiliereId, @Niveau)",
                "UPDATE dbo.Plannings SET CoursId = @CoursId, SalleId = @SalleId, JourSemaine = @JourSemaine, HeureDebut = @HeureDebut, HeureFin = @HeureFin, FiliereId = @FiliereId, Niveau = @Niveau WHERE PlanningId = @Id",
                "DELETE dbo.Plannings WHERE PlanningId = @Id",
                "PlanningId",
                new[]
                {
                    new ChampCatalogue("CoursId", "Cours", TypeChamp.Liste, true, "SELECT CoursId AS Id, Code + N' — ' + Libelle AS Libelle FROM dbo.Cours ORDER BY Code"),
                    new ChampCatalogue("SalleId", "Salle", TypeChamp.Liste, true, "SELECT SalleId AS Id, Code AS Libelle FROM dbo.Salles ORDER BY Code"),
                    new ChampCatalogue("JourSemaine", "Jour", TypeChamp.Liste, true, "SELECT Jour AS Id, Jour AS Libelle FROM (VALUES(N'Lundi'),(N'Mardi'),(N'Mercredi'),(N'Jeudi'),(N'Vendredi')) v(Jour)"),
                    new ChampCatalogue("HeureDebut", "Heure de début", TypeChamp.Heure),
                    new ChampCatalogue("HeureFin", "Heure de fin", TypeChamp.Heure),
                    new ChampCatalogue("FiliereId", "Filière", TypeChamp.Liste, true, "SELECT FiliereId AS Id, Nom AS Libelle FROM dbo.Filieres ORDER BY Nom"),
                    new ChampCatalogue("Niveau", "Niveau", TypeChamp.Liste, true, "SELECT Valeur AS Id, Valeur AS Libelle FROM (VALUES(N'L1'),(N'L2'),(N'L3')) v(Valeur)")
                }),
            ["Absences"] = new(
                "Absences",
                "Historique des Absences",
                "Rechercher une absence...",
                "+  Nouveau",
                "absences",
                4,
                2,
                "SELECT a.AbsenceId, FORMAT(a.DateCours, 'dd/MM/yyyy') AS [Date], e.Nom + N' ' + e.Prenom AS [Étudiant], c.Libelle AS Cours, CASE WHEN a.Present = 1 THEN N'Présent' ELSE N'Absent' END AS Statut FROM dbo.Absences a JOIN dbo.Etudiants e ON e.EtudiantId = a.EtudiantId JOIN dbo.Cours c ON c.CoursId = a.CoursId ORDER BY a.DateCours DESC, e.Nom",
                "SELECT CoursId, EtudiantId, DateCours, Present, Justification FROM dbo.Absences WHERE AbsenceId = @Id",
                "INSERT dbo.Absences(CoursId, EtudiantId, DateCours, Present, Justification) VALUES(@CoursId, @EtudiantId, @DateCours, @Present, @Justification)",
                "UPDATE dbo.Absences SET CoursId = @CoursId, EtudiantId = @EtudiantId, DateCours = @DateCours, Present = @Present, Justification = @Justification WHERE AbsenceId = @Id",
                "DELETE dbo.Absences WHERE AbsenceId = @Id",
                "AbsenceId",
                new[]
                {
                    new ChampCatalogue("CoursId", "Cours", TypeChamp.Liste, true, "SELECT CoursId AS Id, Code + N' — ' + Libelle AS Libelle FROM dbo.Cours ORDER BY Code"),
                    new ChampCatalogue("EtudiantId", "Étudiant", TypeChamp.Liste, true, "SELECT EtudiantId AS Id, Matricule + N' — ' + Nom + N' ' + Prenom AS Libelle FROM dbo.Etudiants WHERE Actif = 1 ORDER BY Nom"),
                    new ChampCatalogue("DateCours", "Date du cours", TypeChamp.Date),
                    new ChampCatalogue("Present", "Présent", TypeChamp.OuiNon),
                    new ChampCatalogue("Justification", "Justification", Obligatoire: false)
                })
        };

    public DefinitionCatalogue GetDefinition(string cle)
    {
        return Definitions.TryGetValue(cle, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(cle), cle, "Catalogue inconnu.");
    }

    public DataTable Lister(DefinitionCatalogue definition)
    {
        try
        {
            var table = DbHelper.Query(definition.RequeteListe);
            if (table.Rows.Count > 0)
            {
                return table;
            }
        }
        catch
        {
            // Mode maquette / hors-ligne si SQL Server n'est pas joignable
        }

        return ObtenirDonneesSecours(definition.Cle).Copy();
    }

    public DataRow? Lire(DefinitionCatalogue definition, int id)
    {
        try
        {
            var table = DbHelper.Query(definition.RequeteEdition, new Dictionary<string, object?> { ["@Id"] = id });
            if (table.Rows.Count > 0)
            {
                return table.Rows[0];
            }
        }
        catch
        {
            // Fallback
        }

        return ConstruireLigneEditionSecours(definition, id);
    }

    public DataTable ChargerListe(string requete)
    {
        try
        {
            var table = DbHelper.Query(requete);
            if (table.Rows.Count > 0)
            {
                return table;
            }
        }
        catch
        {
            // Fallback
        }

        return ChargerListeSecours(requete);
    }

    public void Enregistrer(DefinitionCatalogue definition, int? id, IReadOnlyDictionary<string, object?> valeurs)
    {
        if (string.Equals(definition.Cle, "Planning", StringComparison.Ordinal))
        {
            int coursId = Convert.ToInt32(valeurs.GetValueOrDefault("@CoursId") ?? 1);
            int salleId = Convert.ToInt32(valeurs.GetValueOrDefault("@SalleId") ?? 1);
            string jourSemaine = Convert.ToString(valeurs.GetValueOrDefault("@JourSemaine")) ?? "Lundi";
            TimeSpan heureDebut = valeurs.GetValueOrDefault("@HeureDebut") is TimeSpan hd ? hd : new TimeSpan(8, 0, 0);
            TimeSpan heureFin = valeurs.GetValueOrDefault("@HeureFin") is TimeSpan hf ? hf : new TimeSpan(10, 0, 0);
            int filiereId = Convert.ToInt32(valeurs.GetValueOrDefault("@FiliereId") ?? 1);
            string niveau = Convert.ToString(valeurs.GetValueOrDefault("@Niveau")) ?? "L2";

            PlanningService.VerifierConflits(
                id,
                coursId,
                salleId,
                jourSemaine,
                heureDebut,
                heureFin,
                filiereId,
                niveau);
        }

        var parameters = valeurs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        if (id.HasValue)
        {
            parameters["@Id"] = id.Value;
        }

        try
        {
            DbHelper.Execute(id.HasValue ? definition.RequeteModification : definition.RequeteAjout, parameters);
        }
        catch
        {
            EnregistrerSecours(definition, id, valeurs);
        }
    }

    public void Supprimer(DefinitionCatalogue definition, int id)
    {
        try
        {
            DbHelper.Execute(definition.RequeteSuppression, new Dictionary<string, object?> { ["@Id"] = id });
        }
        catch
        {
            var table = ObtenirDonneesSecours(definition.Cle);
            var rows = table.Select($"{definition.ColonneId} = {id}");
            foreach (var row in rows)
            {
                table.Rows.Remove(row);
            }
        }
    }

    public static DataTable ObtenirDonneesSecours(string cle)
    {
        if (CacheSecours.TryGetValue(cle, out var existant))
        {
            return existant;
        }

        var table = new DataTable();
        switch (cle)
        {
            case "Etudiants":
                table.Columns.Add("EtudiantId", typeof(int));
                table.Columns.Add("Matricule", typeof(string));
                table.Columns.Add("Nom et Prénoms", typeof(string));
                table.Columns.Add("Filière", typeof(string));
                table.Columns.Add("Niveau", typeof(string));
                table.Columns.Add("Statut", typeof(string));
                table.Rows.Add(1, "IIT0001", "KONE Ibrahim", "Génie Logiciel", "L2", "Actif");
                table.Rows.Add(2, "IIT0002", "ADJA Koffi", "Marketing", "L2", "Actif");
                table.Rows.Add(3, "IIT0003", "TAYE Emmanuel", "Informatique", "L1", "Actif");
                table.Rows.Add(4, "IIT0004", "ZOUA Christ", "Génie Logiciel", "L2", "Actif");
                break;

            case "Inscriptions":
                table.Columns.Add("InscriptionId", typeof(int));
                table.Columns.Add("N°", typeof(long));
                table.Columns.Add("Étudiant", typeof(string));
                table.Columns.Add("Filière", typeof(string));
                table.Columns.Add("Niveau", typeof(string));
                table.Columns.Add("Date inscription", typeof(string));
                table.Columns.Add("AnneeAcademique", typeof(string));
                table.Rows.Add(1, 1L, "KONE Ibrahim", "Génie Logiciel", "L2", "05/10/2026", "2026 - 2027");
                table.Rows.Add(2, 2L, "ADJA Koffi", "Marketing", "L2", "05/10/2026", "2026 - 2027");
                table.Rows.Add(3, 3L, "TAYE Emmanuel", "Informatique", "L1", "06/10/2026", "2026 - 2027");
                table.Rows.Add(4, 4L, "ZOUA Christ", "Génie Logiciel", "L2", "06/10/2026", "2026 - 2027");
                break;

            case "Enseignants":
                table.Columns.Add("EnseignantId", typeof(int));
                table.Columns.Add("Matricule", typeof(string));
                table.Columns.Add("Nom et Prénoms", typeof(string));
                table.Columns.Add("Spécialité", typeof(string));
                table.Columns.Add("Contact", typeof(string));
                table.Rows.Add(1, "ENS001", "DIA Mamadou", "Informatique", "07 01 02 03 04");
                table.Rows.Add(2, "ENS002", "KOUAME Alice", "Marketing", "07 11 22 33 44");
                table.Rows.Add(3, "ENS003", "YAO Jean", "Réseaux", "07 55 66 77 88");
                break;

            case "Cours":
                table.Columns.Add("CoursId", typeof(int));
                table.Columns.Add("Code", typeof(string));
                table.Columns.Add("Libellé du cours", typeof(string));
                table.Columns.Add("Filière", typeof(string));
                table.Columns.Add("Crédit", typeof(int));
                table.Rows.Add(1, "GL101", "Programmation C#", "Génie Logiciel", 5);
                table.Rows.Add(2, "GL102", "Base de données (SQL)", "Génie Logiciel", 4);
                table.Rows.Add(3, "MT101", "Marketing de base", "Marketing", 3);
                table.Rows.Add(4, "INF101", "Algorithmique", "Informatique", 5);
                break;

            case "Salles":
                table.Columns.Add("SalleId", typeof(int));
                table.Columns.Add("Salle", typeof(string));
                table.Columns.Add("Capacité", typeof(int));
                table.Columns.Add("Type", typeof(string));
                table.Columns.Add("Statut", typeof(string));
                table.Rows.Add(1, "A1", 50, "Cours", "Disponible");
                table.Rows.Add(2, "A2", 50, "Cours", "Disponible");
                table.Rows.Add(3, "B1", 30, "TP", "Disponible");
                table.Rows.Add(4, "Lab Info", 40, "Laboratoire", "Occupée");
                break;

            case "Planning":
                table.Columns.Add("PlanningId", typeof(int));
                table.Columns.Add("Code", typeof(string));
                table.Columns.Add("Cours", typeof(string));
                table.Columns.Add("Filière", typeof(string));
                table.Columns.Add("Niveau", typeof(string));
                table.Columns.Add("JourSemaine", typeof(string));
                table.Columns.Add("HeureDebut", typeof(TimeSpan));
                table.Columns.Add("HeureFin", typeof(TimeSpan));
                table.Columns.Add("Salle", typeof(string));
                table.Rows.Add(1, "GL101", "Prog. C#", "Génie Logiciel", "L2", "Lundi", new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), "A1");
                table.Rows.Add(2, "GL102", "BD (SQL)", "Génie Logiciel", "L2", "Mardi", new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), "A2");
                table.Rows.Add(3, "INF101", "Algo.", "Génie Logiciel", "L2", "Mardi", new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0), "A1");
                table.Rows.Add(4, "RES101", "Réseaux", "Génie Logiciel", "L2", "Jeudi", new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), "B1");
                table.Rows.Add(5, "GL103", "Poo C#", "Génie Logiciel", "L2", "Jeudi", new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0), "A1");
                break;
        }

        CacheSecours[cle] = table;
        return table;
    }

    private static DataTable ChargerListeSecours(string requete)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(object));
        table.Columns.Add("Libelle", typeof(string));

        if (requete.Contains("dbo.Filieres", StringComparison.OrdinalIgnoreCase))
        {
            foreach (DataRow row in ObtenirFilieresSecours().Rows)
            {
                table.Rows.Add(row["Id"], row["Libelle"]);
            }
        }
        else if (requete.Contains("dbo.Enseignants", StringComparison.OrdinalIgnoreCase))
        {
            foreach (DataRow row in ObtenirDonneesSecours("Enseignants").Rows)
            {
                table.Rows.Add(row["EnseignantId"], row["Nom et Prénoms"]);
            }
        }
        else if (requete.Contains("dbo.Etudiants", StringComparison.OrdinalIgnoreCase))
        {
            foreach (DataRow row in ObtenirDonneesSecours("Etudiants").Rows)
            {
                table.Rows.Add(row["EtudiantId"], $"{row["Matricule"]} — {row["Nom et Prénoms"]}");
            }
        }
        else if (requete.Contains("dbo.Cours", StringComparison.OrdinalIgnoreCase))
        {
            table.Rows.Add(1, "GL101 — Programmation C#");
            table.Rows.Add(2, "GL102 — Base de données (SQL)");
            table.Rows.Add(3, "MT101 — Marketing de base");
            table.Rows.Add(4, "INF101 — Algorithmique");
            table.Rows.Add(5, "RES101 — Réseaux");
            table.Rows.Add(6, "GL103 — Poo C#");
        }
        else if (requete.Contains("dbo.Salles", StringComparison.OrdinalIgnoreCase))
        {
            foreach (DataRow row in ObtenirDonneesSecours("Salles").Rows)
            {
                table.Rows.Add(row["SalleId"], row["Salle"]);
            }
        }
        else if (requete.Contains("Lundi", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var jour in new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" })
            {
                table.Rows.Add(jour, jour);
            }
        }
        else if (requete.Contains("2026 - 2027", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var annee in new[] { "2026 - 2027", "2025 - 2026", "2024 - 2025" })
            {
                table.Rows.Add(annee, annee);
            }
        }
        else if (requete.Contains("Laboratoire", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var type in new[] { "Cours", "TP", "Laboratoire", "Amphithéâtre" })
            {
                table.Rows.Add(type, type);
            }
        }
        else if (requete.Contains("'L1'", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var niv in new[] { "L1", "L2", "L3", "M1", "M2" })
            {
                table.Rows.Add(niv, niv);
            }
        }
        else
        {
            table.Rows.Add("M", "M");
            table.Rows.Add("F", "F");
        }

        return table;
    }

    private static DataRow? ConstruireLigneEditionSecours(DefinitionCatalogue definition, int id)
    {
        var source = ObtenirDonneesSecours(definition.Cle);
        var match = source.Select($"{definition.ColonneId} = {id}").FirstOrDefault();
        if (match is null)
        {
            return null;
        }

        var result = new DataTable();
        foreach (var champ in definition.Champs)
        {
            result.Columns.Add(champ.Nom, typeof(object));
        }

        var row = result.NewRow();
        switch (definition.Cle)
        {
            case "Etudiants":
                var partsEtu = (Convert.ToString(match["Nom et Prénoms"]) ?? "").Split(' ', 2);
                row["Matricule"] = match["Matricule"];
                row["Nom"] = partsEtu.Length > 0 ? partsEtu[0] : "";
                row["Prenom"] = partsEtu.Length > 1 ? partsEtu[1] : "";
                row["FiliereId"] = 1;
                row["Niveau"] = match["Niveau"];
                row["DateNaissance"] = new DateTime(2004, 3, 14);
                row["Sexe"] = "M";
                row["Actif"] = string.Equals(Convert.ToString(match["Statut"]), "Actif", StringComparison.OrdinalIgnoreCase);
                break;

            case "Enseignants":
                var partsEns = (Convert.ToString(match["Nom et Prénoms"]) ?? "").Split(' ', 2);
                row["Matricule"] = match["Matricule"];
                row["Nom"] = partsEns.Length > 0 ? partsEns[0] : "";
                row["Prenom"] = partsEns.Length > 1 ? partsEns[1] : "";
                row["Specialite"] = match["Spécialité"];
                row["Email"] = "";
                row["Telephone"] = match["Contact"];
                row["Actif"] = true;
                break;

            case "Cours":
                row["Code"] = match["Code"];
                row["Libelle"] = match["Libellé du cours"];
                row["FiliereId"] = 1;
                row["EnseignantId"] = 1;
                row["Credits"] = match["Crédit"];
                break;

            case "Salles":
                row["Code"] = match["Salle"];
                row["Capacite"] = match["Capacité"];
                row["Type"] = match["Type"];
                row["Active"] = string.Equals(Convert.ToString(match["Statut"]), "Disponible", StringComparison.OrdinalIgnoreCase);
                break;

            case "Inscriptions":
                row["EtudiantId"] = id;
                row["AnneeAcademique"] = match["AnneeAcademique"];
                row["DateInscription"] = DateTime.Today;
                break;

            case "Planning":
                string codeExist = Convert.ToString(match["Code"]) ?? "GL101";
                row["CoursId"] = codeExist.ToUpperInvariant() switch
                {
                    "GL102" => 2,
                    "MT101" => 3,
                    "INF101" => 4,
                    "RES101" => 5,
                    "GL103" => 6,
                    _ => 1
                };
                string salleExist = Convert.ToString(match["Salle"]) ?? "A1";
                row["SalleId"] = salleExist switch
                {
                    "A2" => 2,
                    "B1" => 3,
                    "Lab Info" => 4,
                    _ => 1
                };
                row["JourSemaine"] = match["JourSemaine"];
                row["HeureDebut"] = match["HeureDebut"];
                row["HeureFin"] = match["HeureFin"];
                row["FiliereId"] = 1;
                row["Niveau"] = match["Niveau"];
                break;
        }

        result.Rows.Add(row);
        return row;
    }

    private static void EnregistrerSecours(DefinitionCatalogue definition, int? id, IReadOnlyDictionary<string, object?> valeurs)
    {
        var table = ObtenirDonneesSecours(definition.Cle);
        int targetId = id ?? (table.Rows.Count == 0 ? 1 : table.AsEnumerable().Max(r => Convert.ToInt32(r[definition.ColonneId])) + 1);
        DataRow? row = id.HasValue ? table.Select($"{definition.ColonneId} = {id.Value}").FirstOrDefault() : null;
        bool isNew = row is null;
        row ??= table.NewRow();
        row[definition.ColonneId] = targetId;

        string LireTexte(string cle) => Convert.ToString(valeurs.GetValueOrDefault(cle)) ?? "";

        switch (definition.Cle)
        {
            case "Etudiants":
                row["Matricule"] = LireTexte("@Matricule");
                row["Nom et Prénoms"] = $"{LireTexte("@Nom")} {LireTexte("@Prenom")}".Trim();
                row["Filière"] = ResoudreNomFiliere(valeurs.GetValueOrDefault("@FiliereId"));
                row["Niveau"] = LireTexte("@Niveau");
                row["Statut"] = Convert.ToBoolean(valeurs.GetValueOrDefault("@Actif") ?? true) ? "Actif" : "Inactif";
                break;

            case "Enseignants":
                row["Matricule"] = LireTexte("@Matricule");
                row["Nom et Prénoms"] = $"{LireTexte("@Nom")} {LireTexte("@Prenom")}".Trim();
                row["Spécialité"] = LireTexte("@Specialite");
                row["Contact"] = LireTexte("@Telephone");
                break;

            case "Cours":
                row["Code"] = LireTexte("@Code");
                row["Libellé du cours"] = LireTexte("@Libelle");
                row["Filière"] = ResoudreNomFiliere(valeurs.GetValueOrDefault("@FiliereId"));
                row["Crédit"] = Convert.ToInt32(valeurs.GetValueOrDefault("@Credits") ?? 3);
                break;

            case "Salles":
                row["Salle"] = LireTexte("@Code");
                row["Capacité"] = Convert.ToInt32(valeurs.GetValueOrDefault("@Capacite") ?? 30);
                row["Type"] = LireTexte("@Type");
                row["Statut"] = Convert.ToBoolean(valeurs.GetValueOrDefault("@Active") ?? true) ? "Disponible" : "Occupée";
                break;

            case "Inscriptions":
                row["N°"] = (long)targetId;
                int etuId = Convert.ToInt32(valeurs.GetValueOrDefault("@EtudiantId") ?? 1);
                var etuMatch = ObtenirDonneesSecours("Etudiants").Select($"EtudiantId = {etuId}").FirstOrDefault();
                row["Étudiant"] = etuMatch is not null ? Convert.ToString(etuMatch["Nom et Prénoms"]) ?? "KONE Ibrahim" : "KONE Ibrahim";
                row["Filière"] = etuMatch is not null ? Convert.ToString(etuMatch["Filière"]) ?? "Génie Logiciel" : "Génie Logiciel";
                row["Niveau"] = etuMatch is not null ? Convert.ToString(etuMatch["Niveau"]) ?? "L2" : "L2";
                var dateIns = valeurs.GetValueOrDefault("@DateInscription") is DateTime dt ? dt : DateTime.Today;
                row["Date inscription"] = dateIns.ToString("dd/MM/yyyy");
                row["AnneeAcademique"] = LireTexte("@AnneeAcademique");
                break;

            case "Planning":
                var (codeCours, libelleCours) = ResoudreCoursSecours(valeurs.GetValueOrDefault("@CoursId"));
                row["Code"] = codeCours;
                row["Cours"] = libelleCours;
                row["Filière"] = ResoudreNomFiliere(valeurs.GetValueOrDefault("@FiliereId"));
                row["Niveau"] = LireTexte("@Niveau");
                row["JourSemaine"] = LireTexte("@JourSemaine");
                row["HeureDebut"] = valeurs.GetValueOrDefault("@HeureDebut") is TimeSpan hd ? hd : new TimeSpan(8, 0, 0);
                row["HeureFin"] = valeurs.GetValueOrDefault("@HeureFin") is TimeSpan hf ? hf : new TimeSpan(10, 0, 0);
                row["Salle"] = ResoudreNomSalle(valeurs.GetValueOrDefault("@SalleId"));
                break;
        }

        if (isNew)
        {
            table.Rows.Add(row);
        }
    }
}
