using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmCours : FrmCatalogue
{
    public FrmCours() : base(new CatalogueService().GetDefinition("Cours")) { InitializeComponent(); }
}
