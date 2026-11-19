using System.Data;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmHistoriqueAbsences : Form
{
    private readonly AbsenceService _service = new();
    private readonly Action? _ouvrirSaisie;
    private readonly Panel _footerHost = new();
    private DataTable? _donnees;
    private int _pageCourante = 1;
    private int _dernierePageAffichee = -1;
    private int _taillePageActuelle = 8;
    private DateTime _debut = new(2026, 10, 1);
    private DateTime _fin = DateTime.Today;

    public FrmHistoriqueAbsences() : this(null) { }

    public FrmHistoriqueAbsences(Action? ouvrirSaisie)
    {
        _ouvrirSaisie = ouvrirSaisie;
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            ChargerListeEtudiants();
            Charger();
        }
    }

    private void ConstruireInterface()
    {
        BackColor = Color.FromArgb(246, 248, 252);
        _periode.Text = $"{_debut:dd/MM/yyyy} - {_fin:dd/MM/yyyy}";

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 16),
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.FromArgb(246, 248, 252)
        };
        UiStyle.AppliquerFondDesign(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var entete = UiStyle.EntetePage(IconeMenu.Absences, "Historique des Absences");
        if (_ouvrirSaisie is not null)
        {
            entete.DoubleClick += (_, _) => _ouvrirSaisie();
        }

        root.Controls.Add(entete, 0, 0);

        var filterBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        var leftGroup = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Location = new Point(2, 2),
            BackColor = Color.Transparent
        };
        leftGroup.Controls.Add(new Label
        {
            Text = "Niveau :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        });
        _etudiant.SelectedIndexChanged += (_, _) =>
        {
            _pageCourante = 1;
            Charger();
        };
        leftGroup.Controls.Add(UiStyle.EncadrerChamp(_etudiant, 160, 38));
        filterBar.Controls.Add(leftGroup);

        var rightGroup = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.Transparent
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

        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.AutoGenerateColumns = true;

        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        _grille.SizeChanged += (_, _) =>
        {
            int nouvelleTaille = UiStyle.CalculerTaillePage(_grille, 6);
            if (nouvelleTaille != _taillePageActuelle)
            {
                _taillePageActuelle = nouvelleTaille;
                MettreAJourPaginationEtPage();
            }
        };
        root.Controls.Add(card, 0, 2);

        _footerHost.Dock = DockStyle.Fill;
        _footerHost.BackColor = Color.Transparent;
        root.Controls.Add(_footerHost, 0, 3);

        Controls.Add(root);
    }


    private void ChargerListeEtudiants()
    {
        _etudiant.Items.Clear();
        _etudiant.Items.AddRange(["Tous", "L1", "L2", "L3"]);
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
        _dernierePageAffichee = -1;
        MettreAJourPaginationEtPage();
    }

    private void MettreAJourPaginationEtPage()
    {
        if (_donnees is null) return;
        _taillePageActuelle = UiStyle.CalculerTaillePage(_grille, 6);
        int nbPages = Math.Max(1, (int)Math.Ceiling(_donnees.DefaultView.Count / (double)_taillePageActuelle));
        if (_pageCourante > nbPages) _pageCourante = nbPages;

        if (nbPages != _dernierePageAffichee)
        {
            _dernierePageAffichee = nbPages;
            _footerHost.Controls.Clear();
            var footer = UiStyle.CreerPiedPagination(_lblVide, nbPages, page =>
            {
                _pageCourante = page;
                AfficherPage();
            });
            var btnImprimer = UiStyle.BoutonBleuExport("Imprimer", 132, 34);
            btnImprimer.Location = new Point(220, 6);
            btnImprimer.Click += (_, _) =>
            {
                if (_donnees is not null)
                {
                    string niv = Convert.ToString(_etudiant.SelectedItem) ?? "Tous";
                    UiStyle.ImprimerTable(
                        this,
                        "Historique des Absences",
                        $"Niveau : {niv}  |  Période : {_periode.Text}",
                        _donnees,
                        "Historique_Absences_IIT.csv");
                }
            };
            footer.Controls.Add(btnImprimer);
            _footerHost.Controls.Add(footer);
        }

        _lblVide.Text = $"Total : {_donnees.DefaultView.Count} absence(s)";
        AfficherPage();
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, _taillePageActuelle);

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
