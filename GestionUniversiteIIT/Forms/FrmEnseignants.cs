using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmEnseignants : FrmCatalogue
{
    public FrmEnseignants() : base(new CatalogueService().GetDefinition("Enseignants")) { }
}
