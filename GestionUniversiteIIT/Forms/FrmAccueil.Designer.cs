namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmAccueil : Form
{
    private readonly Panel _contenu = new();
    private readonly Panel _pnlSignalSql = new();

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Gestion Université IIT";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1060, 680);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;
        ResumeLayout(false);
    }
}
