using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmEtudiants : FrmCatalogue
{
    public FrmEtudiants() : this(null) { }

    public FrmEtudiants(Action? onNouveau) : base(new CatalogueService().GetDefinition("Etudiants"), onNouveau) { InitializeComponent(); }
}
