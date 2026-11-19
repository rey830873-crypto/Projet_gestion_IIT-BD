namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmHistoriqueAbsences : Form
{
    private readonly ComboBox _etudiant = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _periode = new() { ReadOnly = false };
    private readonly DataGridView _grille = new();
    private readonly Label _lblVide = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Historique des Absences";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
