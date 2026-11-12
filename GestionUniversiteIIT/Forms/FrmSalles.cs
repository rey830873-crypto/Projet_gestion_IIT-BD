using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmSalles : FrmCatalogue
{
    public FrmSalles() : base(new CatalogueService().GetDefinition("Salles")) { InitializeComponent(); }
}
