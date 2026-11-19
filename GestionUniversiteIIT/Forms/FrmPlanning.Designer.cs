namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmPlanning : Form
{
    private readonly ComboBox _semaine = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _filiere = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _niveau = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Panel _pnlEnteteOfficielle = new();
    private readonly DataGridView _grille = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Planification des Cours";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
