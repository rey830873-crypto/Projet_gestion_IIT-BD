using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmAjouterEtudiant : FrmEditionCatalogue
{
    public FrmAjouterEtudiant() : base(new CatalogueService(), new CatalogueService().GetDefinition("Etudiants")) { InitializeComponent(); }
}
