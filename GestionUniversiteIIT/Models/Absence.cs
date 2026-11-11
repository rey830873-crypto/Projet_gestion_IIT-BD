namespace GestionUniversiteIIT.Models;

public sealed record Absence(
    int AbsenceId,
    int CoursId,
    int EtudiantId,
    DateTime DateCours,
    bool Present,
    string? Justification);
