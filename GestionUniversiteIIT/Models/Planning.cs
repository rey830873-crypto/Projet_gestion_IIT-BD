namespace GestionUniversiteIIT.Models;

public sealed record Planning(
    int PlanningId,
    int CoursId,
    int SalleId,
    string JourSemaine,
    TimeSpan HeureDebut,
    TimeSpan HeureFin,
    string Niveau);
