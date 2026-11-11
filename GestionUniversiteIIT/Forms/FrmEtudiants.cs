using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmEtudiants : FrmCatalogue
{
    public FrmEtudiants(Action? onNouveau = null) : base(new CatalogueService().GetDefinition("Etudiants"), onNouveau) { }
}
