using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmAjouterEtudiant : FrmEditionCatalogue
{
    public FrmAjouterEtudiant() : base(new CatalogueService(), new CatalogueService().GetDefinition("Etudiants")) { }
}
