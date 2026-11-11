namespace GestionUniversiteIIT.Models;

public sealed record Etudiant(
    int EtudiantId,
    string Matricule,
    string Nom,
    string Prenom,
    int FiliereId,
    string Niveau,
    DateTime? DateNaissance,
    string? Sexe,
    bool Actif);
