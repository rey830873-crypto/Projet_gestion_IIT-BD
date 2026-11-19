using System.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmInscriptions : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;
    private readonly Action? _ouvrirNouvelleInscription;
    private DataTable? _donnees;
    private int _pageCourante = 1;
    private int _dernierePageAffichee = -1;
    private int _taillePageActuelle = 8;

    public FrmInscriptions() : this(null) { }

    public FrmInscriptions(Action? ouvrirNouvelleInscription)
    {
        _ouvrirNouvelleInscription = ouvrirNouvelleInscription;
        _definition = _service.GetDefinition("Inscriptions");
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            Recharger();
        }
    }

    private void ConstruireInterface()
    {
        BackColor = Color.FromArgb(248, 250, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 16),
            RowCount = 5,
            ColumnCount = 1,
            BackColor = Color.FromArgb(246, 248, 252)
        };
        UiStyle.AppliquerFondDesign(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var entete = UiStyle.EntetePage(IconeMenu.Inscriptions, "Suivi des Inscriptions");
        if (_ouvrirNouvelleInscription is not null)
        {
            entete.DoubleClick += (_, _) => _ouvrirNouvelleInscription();
        }
        root.Controls.Add(entete, 0, 0);

        _pnlResume.Dock = DockStyle.Fill;
        _pnlResume.FlowDirection = FlowDirection.LeftToRight;
        _pnlResume.WrapContents = false;
        _pnlResume.BackColor = Color.Transparent;
        _pnlResume.Padding = new Padding(0, 2, 0, 6);
        root.Controls.Add(_pnlResume, 0, 1);

        var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var leftTools = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent
        };
        var searchBox = UiStyle.ChampRecherche("Rechercher une inscription...", 340, out _recherche);
        _recherche.TextChanged += (_, _) =>
        {
            _pageCourante = 1;
            Filtrer();
        };
        leftTools.Controls.Add(searchBox);

        leftTools.Controls.Add(new Label
        {
            Text = "Année :",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(8, 12, 6, 0)
        });
        _annee.Items.AddRange(["2026 - 2027", "2025 - 2026", "2024 - 2025", "Toutes"]);
        _annee.SelectedIndex = 0;
        _annee.SelectedIndexChanged += (_, _) =>
        {
            _pageCourante = 1;
            Filtrer();
        };
        leftTools.Controls.Add(UiStyle.EncadrerChamp(_annee, 165, 38));
        toolbar.Controls.Add(leftTools);

        var rightTools = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent
        };
        if (_ouvrirNouvelleInscription is not null)
        {
            var btnNouvelle = UiStyle.Bouton("+ Nouvelle inscription", true);
            btnNouvelle.Width = 185;
            btnNouvelle.Click += (_, _) => _ouvrirNouvelleInscription();
            rightTools.Controls.Add(btnNouvelle);
        }

        var btnImprimer = UiStyle.BoutonBleuExport("Imprimer / CSV", 155, 40);
        btnImprimer.Click += (_, _) =>
        {
            if (_donnees is not null)
            {
                string annee = Convert.ToString(_annee.SelectedItem) ?? "2026 - 2027";
                UiStyle.ImprimerTable(
                    this,
                    "Liste des Inscriptions",
                    $"Année académique : {annee}",
                    _donnees.DefaultView.ToTable(),
                    "Inscriptions_IIT.csv");
            }
        };
        rightTools.Controls.Add(btnImprimer);
        toolbar.Controls.Add(rightTools);
        root.Controls.Add(toolbar, 0, 2);

        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.AutoGenerateColumns = true;

        UiStyle.ActiverActionsGrille(
            _grille,
            onModifierLigne: rowIndex =>
            {
                var id = IdLigne(rowIndex);
                if (id.HasValue)
                {
                    using var editor = new FrmEditionCatalogue(_service, _definition, id.Value);
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        Recharger();
                    }
                }
            },
            onSupprimerLigne: rowIndex =>
            {
                var id = IdLigne(rowIndex);
                if (id.HasValue && MessageBox.Show(
                        "Confirmer la suppression de cette inscription ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _service.Supprimer(_definition, id.Value);
                    Recharger();
                }
            });

        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        _grille.SizeChanged += (_, _) =>
        {
            int nouvelleTaille = UiStyle.CalculerTaillePage(_grille);
            if (nouvelleTaille != _taillePageActuelle)
            {
                _taillePageActuelle = nouvelleTaille;
                Filtrer();
            }
        };
        root.Controls.Add(card, 0, 3);

        _footerHost.Dock = DockStyle.Fill;
        _footerHost.BackColor = Color.Transparent;
        root.Controls.Add(_footerHost, 0, 4);

        Controls.Add(root);
    }


    private void Recharger()
    {
        _donnees = _service.Lister(_definition);
        ConstruireBandeauResume();
        Filtrer();
    }

    private void ConstruireBandeauResume()
    {
        _pnlResume.Controls.Clear();
        if (_donnees is null) return;

        var rows = _donnees.AsEnumerable().ToList();
        int nbFil = _donnees.Columns.Contains("Filière")
            ? rows.Select(r => Convert.ToString(r["Filière"]) ?? "").Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            : 0;
        int l1 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L1", StringComparison.OrdinalIgnoreCase)) : 0;
        int l2 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L2", StringComparison.OrdinalIgnoreCase)) : 0;
        int l3 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L3", StringComparison.OrdinalIgnoreCase)) : 0;

        _pnlResume.Controls.Add(CreerBadge("Total inscriptions", rows.Count.ToString(), Color.FromArgb(186, 13, 20)));
        _pnlResume.Controls.Add(CreerBadge("Filières représentées", nbFil.ToString(), Color.FromArgb(37, 99, 235)));
        _pnlResume.Controls.Add(CreerBadge("Répartition", $"L1: {l1}  •  L2: {l2}  •  L3: {l3}", Color.FromArgb(16, 185, 129)));
    }

    private static Panel CreerBadge(string libelle, string valeur, Color accent)
    {
        var card = new PanneauArrondi
        {
            Size = new Size(Math.Max(175, 110 + valeur.Length * 7), 48),
            Rayon = 10,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(226, 232, 240),
            Margin = new Padding(0, 0, 12, 0)
        };
        card.Paint += (_, e) =>
        {
            using var bar = new SolidBrush(accent);
            e.Graphics.FillRectangle(bar, 8, 10, 4, card.Height - 20);
        };
        card.Controls.Add(new Label { Text = libelle.ToUpperInvariant(), AutoSize = true, Location = new Point(18, 6), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) });
        card.Controls.Add(new Label { Text = valeur, AutoSize = true, Location = new Point(18, 22), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = UiStyle.Texte });
        return card;
    }

    private void Filtrer()
    {
        if (_donnees is null)
        {
            return;
        }

        var filtres = new List<string>();
        var term = _recherche.Text.Trim();
        if (term.Length > 0)
        {
            var escaped = term.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            filtres.Add($"(CONVERT([Étudiant], 'System.String') LIKE '%{escaped}%' OR CONVERT([Filière], 'System.String') LIKE '%{escaped}%' OR CONVERT([Niveau], 'System.String') LIKE '%{escaped}%')");
        }

        var anneeSel = Convert.ToString(_annee.SelectedItem);
        if (!string.IsNullOrWhiteSpace(anneeSel) && anneeSel != "Toutes" && _donnees.Columns.Contains("AnneeAcademique"))
        {
            var escAnnee = anneeSel.Replace("'", "''");
            filtres.Add($"CONVERT([AnneeAcademique], 'System.String') = '{escAnnee}'");
        }

        _donnees.DefaultView.RowFilter = string.Join(" AND ", filtres);

        _taillePageActuelle = UiStyle.CalculerTaillePage(_grille);
        int nbPages = Math.Max(1, (int)Math.Ceiling(_donnees.DefaultView.Count / (double)_taillePageActuelle));
        if (_pageCourante > nbPages) _pageCourante = nbPages;
        if (nbPages != _dernierePageAffichee)
        {
            _dernierePageAffichee = nbPages;
            _footerHost.Controls.Clear();
            _footerHost.Controls.Add(UiStyle.CreerPiedPagination(_lblTotal, nbPages, page =>
            {
                _pageCourante = page;
                AfficherPage();
            }));
        }

        AfficherPage();

        int count = _donnees.DefaultView.Count;
        string nbFormat = count.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        _lblTotal.Text = $"Total : {nbFormat} inscriptions";
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, _taillePageActuelle);

        if (_grille.Columns.Contains("InscriptionId"))
        {
            _grille.Columns["InscriptionId"]!.Visible = false;
        }

        if (_grille.Columns.Contains("AnneeAcademique"))
        {
            _grille.Columns["AnneeAcademique"]!.Visible = false;
        }

        if (_grille.Columns.Contains("N°"))
        {
            _grille.Columns["N°"]!.FillWeight = 12;
            _grille.Columns["N°"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grille.Columns["N°"]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        if (_grille.Columns.Contains("Niveau"))
        {
            _grille.Columns["Niveau"]!.FillWeight = 15;
            _grille.Columns["Niveau"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grille.Columns["Niveau"]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        if (_grille.Columns.Contains("Date inscription"))
        {
            _grille.Columns["Date inscription"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grille.Columns["Date inscription"]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    private int? IdLigne(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _grille.Rows.Count || !_grille.Columns.Contains("InscriptionId"))
        {
            return null;
        }

        var val = _grille.Rows[rowIndex].Cells["InscriptionId"].Value;
        return val is null || val is DBNull ? null : Convert.ToInt32(val);
    }
}
