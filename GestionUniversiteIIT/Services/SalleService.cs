using System.Data;
using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT.Services;

public sealed class SalleService
{
    public DataTable Lister() => DbHelper.Query("SELECT SalleId,Code,Capacite,[Type],Active FROM dbo.Salles ORDER BY Code");
}
