namespace GestionUniversiteIIT.Forms;

public partial class FrmCatalogue : Form
{
    private readonly DataGridView _grille = new();
    private readonly Label _lblTotal = new();
    private readonly ComboBox _filtreSecondaire = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly FlowLayoutPanel _pnlResume = new();
    private readonly Panel _footerHost = new();
    private TextBox _recherche = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // FrmCatalogue
        // 
        BackColor = Color.White;
        ClientSize = new Size(1477, 539);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9.5F);
        FormBorderStyle = FormBorderStyle.None;
        Name = "FrmCatalogue";
        Text = "Catalogue";
        ResumeLayout(false);
    }
}
