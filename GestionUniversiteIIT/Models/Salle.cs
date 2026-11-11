namespace GestionUniversiteIIT.Models;

public sealed record Salle(
    int SalleId,
    string Code,
    int Capacite,
    string Type,
    bool Active);
