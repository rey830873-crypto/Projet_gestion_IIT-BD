using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmStatistiques : Form
{
    private readonly StatistiqueService _service = new();

    public FrmStatistiques()
    {
        Text = "Statistiques";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;
        Charger();
    }

    private void Charger()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 22),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 1. En-tête "Statistiques" avec icône rouge
        root.Controls.Add(UiStyle.EntetePage(IconeMenu.Rapports, "Statistiques"), 0, 0);

        var stats = _service.Charger();

        // 2. Quatre cartes KPI colorées dans l'ordre exact de 12_statistiques_generales.png :
        //    1 248 Étudiants (bleu) | 1 226 Inscriptions (rouge) | 37 Cours (vert) | 68 Enseignants (orange)
        var kpiRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 8)
        };
        for (int i = 0; i < 4; i++)
        {
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Étudiants",
                stats.Etudiants,
                Color.FromArgb(30, 124, 218),
                Color.FromArgb(18, 88, 178),
                IconeMenu.Etudiants,
                valeurAuDessus: true),
            0, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Inscriptions",
                stats.Inscriptions,
                Color.FromArgb(214, 34, 40),
                Color.FromArgb(166, 16, 22),
                IconeMenu.Inscriptions,
                valeurAuDessus: true),
            1, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Cours",
                stats.Cours,
                Color.FromArgb(72, 162, 56),
                Color.FromArgb(44, 126, 32),
                IconeMenu.Cours,
                valeurAuDessus: true),
            2, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Enseignants",
                stats.Enseignants,
                Color.FromArgb(242, 132, 20),
                Color.FromArgb(214, 98, 8),
                IconeMenu.Enseignants,
                valeurAuDessus: true),
            3, 0);

        root.Controls.Add(kpiRow, 0, 1);

        // 3. Deux graphiques côte à côte :
        //    Gauche : "Taux de présence (ce mois)" (Camembert 92% / 8%)
        //    Droite : "Inscriptions par niveau" (Histogramme L1=412, L2=438, L3=376)
        var chartsGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 6, 0, 0)
        };
        chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        var pieCard = new PanneauGraphiqueCamembert(stats.TauxPresence, stats.TauxAbsence)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 4, 10, 4)
        };

        var barCard = new PanneauGraphiqueBarres(stats.InscriptionsL1, stats.InscriptionsL2, stats.InscriptionsL3)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 4, 6, 4)
        };

        chartsGrid.Controls.Add(pieCard, 0, 0);
        chartsGrid.Controls.Add(barCard, 1, 0);
        root.Controls.Add(chartsGrid, 0, 2);

        Controls.Add(root);
    }
}
