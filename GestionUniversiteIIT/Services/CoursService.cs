using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class CoursService
{
    public DataTable Lister(string recherche = "")
    {
        return DbHelper.Query(
            "SELECT c.CoursId,c.Code,c.Libelle,f.Nom AS Filiere,c.Credits FROM dbo.Cours c JOIN dbo.Filieres f ON f.FiliereId=c.FiliereId WHERE @Recherche=N'' OR c.Code LIKE @Filtre OR c.Libelle LIKE @Filtre ORDER BY c.Code",
            new Dictionary<string, object?> { ["@Recherche"] = recherche.Trim(), ["@Filtre"] = $"%{recherche.Trim()}%" });
    }
}
