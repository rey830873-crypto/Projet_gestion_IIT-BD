namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmConnexion : Form
{
    private readonly TextBox _nom = new();
    private readonly TextBox _motDePasse = new();
    private readonly Panel _pnlSignalSql = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Gestion Université IIT";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 560);
        ClientSize = new Size(980, 600);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10f);
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
