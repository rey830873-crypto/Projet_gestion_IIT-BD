using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmEnseignants : FrmCatalogue
{
    public FrmEnseignants() : base(new CatalogueService().GetDefinition("Enseignants")) { InitializeComponent(); }
}
