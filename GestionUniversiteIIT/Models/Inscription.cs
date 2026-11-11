namespace GestionUniversiteIIT.Models;

public sealed record Inscription(
    int InscriptionId,
    int EtudiantId,
    string AnneeAcademique,
    DateTime DateInscription);
