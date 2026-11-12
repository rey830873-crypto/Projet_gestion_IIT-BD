namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmParametres : Form
{
    private readonly TextBox _txtServeur = new();
    private readonly TextBox _txtBase = new();
    private readonly ComboBox _cboAuth = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtUserSql = new();
    private readonly TextBox _txtPassSql = new() { UseSystemPasswordChar = true };
    private readonly CheckBox _chkEncrypt = new() { Text = "Chiffrement SSL (Encrypt)", AutoSize = true, Checked = true };
    private readonly CheckBox _chkTrustCert = new() { Text = "Certificat serveur approuvé (TrustServerCertificate)", AutoSize = true, Checked = true };
    private readonly Panel _pnlPastilleSignal = new() { Size = new Size(14, 14) };
    private readonly Label _lblStatutConnexion = new();
    private readonly DataGridView _gridEtatTables = new();

    private readonly DataGridView _gridFilieres = new();
    private readonly TextBox _txtNouvelleFiliere = new();
    private readonly TextBox _txtNouveauMdp = new();
    private readonly TextBox _txtConfirmerMdp = new();
    private readonly TextBox _txtNouvelUtilisateur = new();
    private readonly ComboBox _cboRoleUtilisateur = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtMdpUtilisateur = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Paramètres";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
