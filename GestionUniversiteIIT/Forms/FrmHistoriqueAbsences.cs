using System.Data;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmHistoriqueAbsences : Form
{
    private readonly AbsenceService _service = new();
    private readonly Action? _ouvrirSaisie;
    private readonly ComboBox _etudiant = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _periode = new() { Text = "01/10/2026 - 06/11/2026", ReadOnly = false };
    private readonly DataGridView _grille = new();
    private readonly Label _lblVide = new();
    private DataTable? _donnees;
    private int _pageCourante = 1;
    private DateTime _debut = new(2026, 10, 1);
    private DateTime _fin = new(2026, 11, 6);

    public FrmHistoriqueAbsences(Action? ouvrirSaisie = null)
    {
        _ouvrirSaisie = ouvrirSaisie;
        ConstruireInterface();
        ChargerListeEtudiants();
        Charger();
    }

    private void ConstruireInterface()
    {
        Text = "Historique des Absences";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 16),
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        // 1. En-tête "Historique des Absences"
        var entete = UiStyle.EntetePage(IconeMenu.Absences, "Historique des Absences");
        if (_ouvrirSaisie is not null)
        {
            entete.DoubleClick += (_, _) => _ouvrirSaisie();
        }

        root.Controls.Add(entete, 0, 0);

        // 2. Barre de filtres : "Étudiant : [Tous]" à gauche + "Période : [01/10/2026 - 06/11/2026 📅]" à droite (11_historique_absences.png)
        var filterBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var leftGroup = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Location = new Point(2, 2),
            BackColor = Color.White
        };
        leftGroup.Controls.Add(new Label
        {
            Text = "Étudiant :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        });
        _etudiant.SelectedIndexChanged += (_, _) => Charger();
        leftGroup.Controls.Add(UiStyle.EncadrerChamp(_etudiant, 240, 38));
        filterBar.Controls.Add(leftGroup);

        var rightGroup = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.White
        };
        rightGroup.Controls.Add(new Label
        {
            Text = "Période :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        });

        var boxPeriode = new PanneauArrondi
        {
            Size = new Size(276, 38),
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = UiStyle.BordureGrise,
            Padding = new Padding(12, 8, 36, 6),
            Margin = new Padding(0, 3, 0, 3)
        };
        _periode.BorderStyle = BorderStyle.None;
        _periode.Dock = DockStyle.Fill;
        _periode.Font = new Font("Segoe UI", 10f);
        _periode.BackColor = Color.White;
        _periode.ForeColor = UiStyle.Texte;
        _periode.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                AnalyserPeriode();
                Charger();
            }
        };
        _periode.Leave += (_, _) =>
        {
            AnalyserPeriode();
            Charger();
        };
        boxPeriode.Controls.Add(_periode);
        boxPeriode.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int cx = boxPeriode.Width - 20;
            int cy = boxPeriode.Height / 2;
            UiStyle.DessinerIconeMenu(e.Graphics, new Rectangle(cx - 10, cy - 10, 20, 20), IconeMenu.Planification, UiStyle.Texte);
        };

        rightGroup.Controls.Add(boxPeriode);
        rightGroup.Location = new Point(filterBar.Width - 365, 2);
        filterBar.Resize += (_, _) => rightGroup.Location = new Point(Math.Max(340, filterBar.Width - rightGroup.Width - 4), 2);
        filterBar.Controls.Add(rightGroup);

        root.Controls.Add(filterBar, 0, 1);

        // 3. Grille : Date | Étudiant | Cours | Statut [badge rouge "Absent"]
        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.AutoGenerateColumns = true;

        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        root.Controls.Add(card, 0, 2);

        // 4. Pied de page avec bouton Imprimer/Exporter à gauche et pagination "< 1 2 >" à droite (11_historique_absences.png)
        var footer = UiStyle.CreerPiedPagination(_lblVide, 2, page =>
        {
            _pageCourante = page;
            AfficherPage();
        });
        var btnImprimer = UiStyle.BoutonBleuExport("Imprimer", 132, 34);
        btnImprimer.Location = new Point(4, 6);
        btnImprimer.Click += (_, _) =>
        {
            if (_donnees is not null)
            {
                string etu = Convert.ToString(_etudiant.SelectedItem) ?? "Tous";
                UiStyle.ImprimerTable(
                    this,
                    "Historique des Absences",
                    $"Étudiant : {etu}  |  Période : {_periode.Text}",
                    _donnees,
                    "Historique_Absences_IIT.csv");
            }
        };
        footer.Controls.Add(btnImprimer);
        root.Controls.Add(footer, 0, 3);

        Controls.Add(root);
    }

    private void ChargerListeEtudiants()
    {
        _etudiant.Items.Clear();
        _etudiant.Items.Add("Tous");

        try
        {
            var table = DbHelper.Query("SELECT Nom + N' ' + Prenom AS NomComplet FROM dbo.Etudiants WHERE Actif = 1 ORDER BY Nom");
            foreach (DataRow row in table.Rows)
            {
                string nom = Convert.ToString(row["NomComplet"]) ?? "";
                if (!string.IsNullOrWhiteSpace(nom) && !_etudiant.Items.Contains(nom))
                {
                    _etudiant.Items.Add(nom);
                }
            }
        }
        catch
        {
            // Mode maquette
        }

        if (_etudiant.Items.Count == 1)
        {
            _etudiant.Items.AddRange(["ZOUA Christ", "KONE Ibrahim", "TAYE Emmanuel", "ADJA Koffi"]);
        }

        _etudiant.SelectedIndex = 0;
    }

    private void AnalyserPeriode()
    {
        var parts = _periode.Text.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            DateTime.TryParse(parts[0], out var d1) &&
            DateTime.TryParse(parts[1], out var d2) &&
            d1 <= d2)
        {
            _debut = d1;
            _fin = d2;
        }
    }

    private void Charger()
    {
        string filtreEtu = Convert.ToString(_etudiant.SelectedItem) ?? "Tous";
        _donnees = _service.Historique(_debut, _fin, filtreEtu);
        AfficherPage();
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, 5);

        if (_grille.Columns.Contains("AbsenceId"))
        {
            _grille.Columns["AbsenceId"]!.Visible = false;
        }

        if (_grille.Columns.Contains("Statut"))
        {
            _grille.Columns["Statut"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grille.Columns["Statut"]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }
}
