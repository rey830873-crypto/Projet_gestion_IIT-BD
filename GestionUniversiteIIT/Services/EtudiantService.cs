using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class EtudiantService
{
    public DataTable Lister(string recherche = "")
    {
        return DbHelper.Query(
            "SELECT e.EtudiantId, e.Matricule, e.Nom, e.Prenom, f.Nom AS Filiere, e.Niveau, CASE WHEN e.Actif = 1 THEN N'Actif' ELSE N'Inactif' END AS Statut FROM dbo.Etudiants e JOIN dbo.Filieres f ON f.FiliereId = e.FiliereId WHERE @Recherche = N'' OR e.Matricule LIKE @Filtre OR e.Nom LIKE @Filtre OR e.Prenom LIKE @Filtre ORDER BY e.Matricule",
            new Dictionary<string, object?> { ["@Recherche"] = recherche.Trim(), ["@Filtre"] = $"%{recherche.Trim()}%" });
    }
}
