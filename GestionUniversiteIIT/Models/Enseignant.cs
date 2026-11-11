namespace GestionUniversiteIIT.Models;

public sealed record Enseignant(
    int EnseignantId,
    string Matricule,
    string Nom,
    string Prenom,
    string Specialite,
    string? Email,
    string? Telephone,
    bool Actif);
