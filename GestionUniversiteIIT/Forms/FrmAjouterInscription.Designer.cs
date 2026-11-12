namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmAjouterInscription : Form
{
    private readonly TextBox _matricule = new() { Text = "IIT0015" };
    private readonly TextBox _nom = new();
    private readonly TextBox _prenoms = new();
    private readonly TextBox _dateNaissance = new() { PlaceholderText = "jj/mm/aaaa" };
    private readonly DateTimePicker _datePickerCache = new() { Format = DateTimePickerFormat.Short, Visible = false };
    private readonly ComboBox _sexe = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _filiere = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _niveau = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _annee = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly PanneauPhotoEtudiant _photoBox = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Nouvelle Inscription";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
