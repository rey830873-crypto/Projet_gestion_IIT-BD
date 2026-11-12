namespace GestionUniversiteIIT.Forms;

public partial class FrmEditionCatalogue : Form
{
    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Édition";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        MinimumSize = new Size(490, 260);
        ClientSize = new Size(540, 400);
        ResumeLayout(false);
    }
}
