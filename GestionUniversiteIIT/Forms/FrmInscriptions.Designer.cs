namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmInscriptions : Form
{
    private readonly DataGridView _grille = new();
    private readonly ComboBox _annee = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblTotal = new();
    private readonly FlowLayoutPanel _pnlResume = new();
    private readonly Panel _footerHost = new();
    private TextBox _recherche = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Inscriptions";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
