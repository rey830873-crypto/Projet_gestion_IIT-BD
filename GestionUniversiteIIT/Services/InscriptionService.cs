using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class InscriptionService
{
    public DataTable Lister(string recherche = "")
    {
        return DbHelper.Query(
            "SELECT i.InscriptionId,e.Matricule,e.Nom+N' '+e.Prenom AS Etudiant,f.Nom AS Filiere,e.Niveau,i.AnneeAcademique,i.DateInscription FROM dbo.Inscriptions i JOIN dbo.Etudiants e ON e.EtudiantId=i.EtudiantId JOIN dbo.Filieres f ON f.FiliereId=e.FiliereId WHERE @Recherche=N'' OR e.Matricule LIKE @Filtre OR e.Nom LIKE @Filtre OR i.AnneeAcademique LIKE @Filtre ORDER BY i.DateInscription DESC",
            new Dictionary<string, object?> { ["@Recherche"] = recherche.Trim(), ["@Filtre"] = $"%{recherche.Trim()}%" });
    }
}
