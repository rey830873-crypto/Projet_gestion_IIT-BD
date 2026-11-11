namespace GestionUniversiteIIT.Models;

public sealed record Cours(
    int CoursId,
    string Code,
    string Libelle,
    int FiliereId,
    int? EnseignantId,
    int Credits);
