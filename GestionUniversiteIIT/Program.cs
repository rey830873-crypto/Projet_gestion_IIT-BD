using GestionUniversiteIIT.Data;

namespace GestionUniversiteIIT;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseSeeder.SynchroniserEtudiantsReels();
        Application.Run(new Forms.FrmConnexion());
    }
}
