using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmCours : FrmCatalogue
{
    public FrmCours() : base(new CatalogueService().GetDefinition("Cours")) { }
}
