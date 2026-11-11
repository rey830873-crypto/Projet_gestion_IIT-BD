using System.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmInscriptions : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;
    private readonly Action? _ouvrirNouvelleInscription;
    private readonly DataGridView _grille = new();
    private readonly ComboBox _annee = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblTotal = new();
    private TextBox _recherche = null!;
    private DataTable? _donnees;
    private int _pageCourante = 1;

    public FrmInscriptions(Action? ouvrirNouvelleInscription = null)
    {
        _ouvrirNouvelleInscription = ouvrirNouvelleInscription;
        _definition = _service.GetDefinition("Inscriptions");
        ConstruireInterface();
        Recharger();
    }

    private void ConstruireInterface()
    {
        Text = "Inscriptions";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
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

        // 1. En-tête "Inscriptions" avec badge rouge
        var entete = UiStyle.EntetePage(IconeMenu.Inscriptions, "Inscriptions");
        if (_ouvrirNouvelleInscription is not null)
        {
            entete.DoubleClick += (_, _) => _ouvrirNouvelleInscription();
        }

        root.Controls.Add(entete, 0, 0);

        // 2. Barre d'outils : "Rechercher une inscription..." à gauche + "Année : [2026 - 2027]" à droite (05_liste_inscriptions.png)
        var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var searchBox = UiStyle.ChampRecherche("Rechercher une inscription...", 430, out _recherche);
        searchBox.Location = new Point(2, 4);
        _recherche.TextChanged += (_, _) => Filtrer();
        toolbar.Controls.Add(searchBox);

        var rightFilter = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        var lblAnnee = new Label
        {
            Text = "Année :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        };

        _annee.Items.AddRange(["2026 - 2027", "2025 - 2026", "2024 - 2025", "Toutes"]);
        _annee.SelectedIndex = 0;
        _annee.SelectedIndexChanged += (_, _) => Filtrer();
        var boxAnnee = UiStyle.EncadrerChamp(_annee, 188, 38);

        rightFilter.Controls.Add(lblAnnee);
        rightFilter.Controls.Add(boxAnnee);
        rightFilter.Location = new Point(toolbar.Width - 260, 2);
        toolbar.Resize += (_, _) => rightFilter.Location = new Point(Math.Max(450, toolbar.Width - rightFilter.Width - 4), 2);
        toolbar.Controls.Add(rightFilter);

        root.Controls.Add(toolbar, 0, 1);

        // 3. Grille des inscriptions avec colonnes N°, Étudiant, Filière, Niveau, Date inscription, Actions
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
        root.Controls.Add(card, 0, 2);

        // 4. Pied de page : "Total : 1 226 inscriptions" + Bouton Imprimer/Exporter + Pagination "< 1 2 3 ... 62 >"
        var footer = UiStyle.CreerPiedPagination(_lblTotal, 62, page =>
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
                string annee = Convert.ToString(_annee.SelectedItem) ?? "2026 - 2027";
                UiStyle.ImprimerTable(
                    this,
                    "Liste des Inscriptions",
                    $"Année académique : {annee}",
                    _donnees.DefaultView.ToTable(),
                    "Inscriptions_IIT.csv");
            }
        };
        footer.Controls.Add(btnImprimer);
        root.Controls.Add(footer, 0, 3);

        Controls.Add(root);
    }

    private void Recharger()
    {
        _donnees = _service.Lister(_definition);
        Filtrer();
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
        AfficherPage();

        int count = _donnees.DefaultView.Count;
        int affiche = string.IsNullOrWhiteSpace(term) && count <= 10 ? 1226 : count;
        string nbFormat = affiche.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        _lblTotal.Text = $"Total : {nbFormat} inscriptions";
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, 8);

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
