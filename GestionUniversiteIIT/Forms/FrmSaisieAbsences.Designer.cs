namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmSaisieAbsences : Form
{
    private readonly ComboBox _cours = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _filiere = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _niveau = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _dateTexte = new() { Text = "06/11/2026" };
    private readonly DataGridView _grille = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Saisie des Absences";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
