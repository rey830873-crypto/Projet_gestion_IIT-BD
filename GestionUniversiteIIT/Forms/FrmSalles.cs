using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmSalles : FrmCatalogue
{
    public FrmSalles() : base(new CatalogueService().GetDefinition("Salles")) { }
}
