using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class PlanningService
{
    public DataTable Lister() => DbHelper.Query(
        "SELECT p.PlanningId,c.Code+N' — '+c.Libelle AS Cours,f.Nom AS Filiere,p.Niveau,p.JourSemaine,p.HeureDebut,p.HeureFin,s.Code AS Salle FROM dbo.Plannings p JOIN dbo.Cours c ON c.CoursId=p.CoursId JOIN dbo.Filieres f ON f.FiliereId=p.FiliereId JOIN dbo.Salles s ON s.SalleId=p.SalleId ORDER BY p.JourSemaine,p.HeureDebut");

    public static void VerifierConflits(
        int? planningId,
        int coursId,
        int salleId,
        string jourSemaine,
        TimeSpan heureDebut,
        TimeSpan heureFin,
        int filiereId,
        string niveau)
    {
        if (heureFin <= heureDebut)
        {
            throw new InvalidOperationException("L'heure de fin doit être strictement postérieure à l'heure de début.");
        }

        // 1. Vérification en base SQL Server si joignable
        try
        {
            var conflitsSql = DbHelper.Query(
                """
                SELECT
                    p.PlanningId,
                    p.SalleId,
                    s.Code AS SalleCode,
                    p.FiliereId,
                    f.Nom AS FiliereNom,
                    p.Niveau,
                    c.Code AS CoursCode,
                    c.Libelle AS CoursLibelle,
                    c.EnseignantId,
                    p.HeureDebut,
                    p.HeureFin
                FROM dbo.Plannings p
                JOIN dbo.Salles s ON s.SalleId = p.SalleId
                JOIN dbo.Filieres f ON f.FiliereId = p.FiliereId
                JOIN dbo.Cours c ON c.CoursId = p.CoursId
                WHERE p.JourSemaine = @Jour
                  AND (@Id IS NULL OR p.PlanningId <> @Id)
                  AND p.HeureDebut < @Fin
                  AND @Debut < p.HeureFin
                """,
                new Dictionary<string, object?>
                {
                    ["@Id"] = planningId,
                    ["@Jour"] = jourSemaine,
                    ["@Debut"] = heureDebut,
                    ["@Fin"] = heureFin
                });

            object? enseignantCibleObj = DbHelper.Scalar(
                "SELECT EnseignantId FROM dbo.Cours WHERE CoursId = @CoursId",
                new Dictionary<string, object?> { ["@CoursId"] = coursId });
            int? enseignantCible = enseignantCibleObj is null or DBNull ? null : Convert.ToInt32(enseignantCibleObj);

            foreach (DataRow row in conflitsSql.Rows)
            {
                string coursConflit = $"{row["CoursCode"]} ({row["CoursLibelle"]})";
                TimeSpan d = row["HeureDebut"] is TimeSpan tsD ? tsD : TimeSpan.Zero;
                TimeSpan f = row["HeureFin"] is TimeSpan tsF ? tsF : TimeSpan.Zero;
                string plage = $"{d.Hours:00}h{d.Minutes:00} - {f.Hours:00}h{f.Minutes:00}";

                if (Convert.ToInt32(row["SalleId"]) == salleId)
                {
                    throw new InvalidOperationException(
                        $"Conflit de salle : La salle {row["SalleCode"]} est déjà occupée le {jourSemaine} sur le créneau {plage} par le cours {coursConflit}.");
                }

                if (Convert.ToInt32(row["FiliereId"]) == filiereId &&
                    string.Equals(Convert.ToString(row["Niveau"]), niveau, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Conflit de promotion : La filière {row["FiliereNom"]} ({niveau}) suit déjà le cours {coursConflit} le {jourSemaine} sur le créneau {plage}.");
                }

                if (enseignantCible.HasValue &&
                    row["EnseignantId"] is not null and not DBNull &&
                    Convert.ToInt32(row["EnseignantId"]) == enseignantCible.Value)
                {
                    throw new InvalidOperationException(
                        $"Conflit d'enseignant : L'enseignant affecté à ce cours assure déjà {coursConflit} le {jourSemaine} sur le créneau {plage}.");
                }
            }

            return;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch
        {
            // Mode maquette / hors-ligne : vérification sur le jeu de données en mémoire
        }

        string salleCible = CatalogueService.ResoudreNomSalle(salleId);
        string filiereCible = CatalogueService.ResoudreNomFiliere(filiereId);
        int enseignantSecoursCible = CatalogueService.ResoudreEnseignantDuCours(coursId);

        var tableSecours = CatalogueService.ObtenirDonneesSecours("Planning");
        foreach (DataRow row in tableSecours.Rows)
        {
            int rowId = Convert.ToInt32(row["PlanningId"]);
            if (planningId.HasValue && rowId == planningId.Value)
            {
                continue;
            }

            string jour = Convert.ToString(row["JourSemaine"]) ?? "";
            if (!string.Equals(jour, jourSemaine, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            TimeSpan d = row["HeureDebut"] is TimeSpan tsD ? tsD : new TimeSpan(8, 0, 0);
            TimeSpan f = row["HeureFin"] is TimeSpan tsF ? tsF : new TimeSpan(10, 0, 0);
            if (!(d < heureFin && heureDebut < f))
            {
                continue;
            }

            string plage = $"{d.Hours:00}h{d.Minutes:00} - {f.Hours:00}h{f.Minutes:00}";
            string coursExistant = Convert.ToString(row["Cours"]) ?? "";
            string codeExistant = Convert.ToString(row["Code"]) ?? "";
            string salleExistante = Convert.ToString(row["Salle"]) ?? "";
            string filiereExistante = Convert.ToString(row["Filière"]) ?? "";
            string niveauExistant = Convert.ToString(row["Niveau"]) ?? "";

            if (string.Equals(salleExistante, salleCible, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Conflit de salle : La salle {salleCible} est déjà occupée le {jourSemaine} sur le créneau {plage} ({coursExistant}).");
            }

            if (string.Equals(filiereExistante, filiereCible, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(niveauExistant, niveau, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Conflit de promotion : La classe {filiereCible} {niveau} a déjà le cours « {coursExistant} » le {jourSemaine} ({plage}).");
            }

            int ensExistant = CatalogueService.ResoudreEnseignantParCodeCours(codeExistant);
            if (enseignantSecoursCible > 0 && ensExistant == enseignantSecoursCible)
            {
                throw new InvalidOperationException(
                    $"Conflit d'enseignant : L'enseignant responsable assure déjà le cours « {coursExistant} » le {jourSemaine} ({plage}).");
            }
        }
    }
}
