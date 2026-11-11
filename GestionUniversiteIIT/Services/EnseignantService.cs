using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class EnseignantService
{
    public DataTable Lister(string recherche = "")
    {
        return DbHelper.Query(
            "SELECT EnseignantId,Matricule,Nom,Prenom,Specialite,Telephone,Email FROM dbo.Enseignants WHERE @Recherche=N'' OR Matricule LIKE @Filtre OR Nom LIKE @Filtre OR Prenom LIKE @Filtre ORDER BY Nom,Prenom",
            new Dictionary<string, object?> { ["@Recherche"] = recherche.Trim(), ["@Filtre"] = $"%{recherche.Trim()}%" });
    }
}
