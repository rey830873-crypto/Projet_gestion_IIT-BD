using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmStatistiques : Form
{
    private readonly StatistiqueService _service = new();

    public FrmStatistiques()
    {
        InitializeComponent();
        ConstruireInterface();
    }

    private void ConstruireInterface()
    {
        BackColor = Color.FromArgb(248, 250, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 20),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.FromArgb(246, 248, 252)
        };
        UiStyle.AppliquerFondDesign(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 212));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 1. En-tête + résumé
        var headerBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var entete = UiStyle.EntetePage(IconeMenu.Rapports, "Analytique & Rapports");
        entete.Dock = DockStyle.Left;
        entete.Width = 420;
        headerBar.Controls.Add(entete);

        var stats = System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime
            ? new Statistiques(1248, 1226, 37, 68, 92, 8, 412, 438, 376)
            : _service.Charger();

        var lblSousTitre = new Label
        {
            Text = $"Année 2026 - 2027  •  {stats.SallesActives} salles actives  •  {stats.CreneauxPlanning} créneaux planifiés",
            Dock = DockStyle.Right,
            AutoSize = false,
            Width = 460,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139)
        };
        headerBar.Controls.Add(lblSousTitre);
        root.Controls.Add(headerBar, 0, 0);

        // 2. Ligne Bento supérieure : Jauge sombre à gauche (36%) + Grille 2x2 de KPI à droite (64%)
        var topBento = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 2, 0, 8)
        };
        topBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
        topBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));

        var jauge = new PanneauJaugeAssiduite(stats.TauxPresence, stats.Presents, stats.Absents)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 4, 10, 4)
        };
        topBento.Controls.Add(jauge, 0, 0);

        var kpiGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty
        };
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        kpiGrid.Controls.Add(
            UiStyle.CarteIndicateurModerne("Étudiants actifs", stats.Etudiants,
                Color.FromArgb(37, 99, 235), Color.FromArgb(29, 78, 216), IconeMenu.Etudiants, true), 0, 0);
        kpiGrid.Controls.Add(
            UiStyle.CarteIndicateurModerne("Inscriptions 2026-2027", stats.Inscriptions,
                Color.FromArgb(186, 13, 20), Color.FromArgb(140, 10, 16), IconeMenu.Inscriptions, true), 1, 0);
        kpiGrid.Controls.Add(
            UiStyle.CarteIndicateurModerne("Cours au programme", stats.Cours,
                Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105), IconeMenu.Cours, true), 0, 1);
        kpiGrid.Controls.Add(
            UiStyle.CarteIndicateurModerne("Enseignants", stats.Enseignants,
                Color.FromArgb(245, 158, 11), Color.FromArgb(217, 119, 6), IconeMenu.Enseignants, true), 1, 1);

        topBento.Controls.Add(kpiGrid, 1, 0);
        root.Controls.Add(topBento, 0, 1);

        // 3. Ligne Bento inférieure : Courbe de tendance (42%) | Niveaux & Lollipop Hebdo (30%) | Filières (28%)
        var bottomBento = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 6, 0, 0)
        };
        bottomBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
        bottomBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        bottomBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

        var courbeCard = new PanneauCourbeTendance(stats.TendancePresences)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 4, 8, 4)
        };
        var niveauxPlanCard = new PanneauNiveauxEtPlanning(stats.InscriptionsL1, stats.InscriptionsL2, stats.InscriptionsL3, stats.SeancesParJour)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 4, 8, 4)
        };
        var filieresCard = new PanneauRepartitionFilieres(stats.RepartitionFilieres)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 4, 4, 4)
        };

        bottomBento.Controls.Add(courbeCard, 0, 0);
        bottomBento.Controls.Add(niveauxPlanCard, 1, 0);
        bottomBento.Controls.Add(filieresCard, 2, 0);
        root.Controls.Add(bottomBento, 0, 2);

        Controls.Add(root);
    }
}
