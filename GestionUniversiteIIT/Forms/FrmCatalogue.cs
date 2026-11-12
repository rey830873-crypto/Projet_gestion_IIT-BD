using System.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public partial class FrmCatalogue : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;
    private readonly Action? _onNouveau;
    private DataTable? _donnees;
    private int _pageCourante = 1;

    public FrmCatalogue() : this(new CatalogueService().GetDefinition("Etudiants")) { }

    public FrmCatalogue(DefinitionCatalogue definition, Action? onNouveau = null)
    {
        _definition = definition;
        _onNouveau = onNouveau;
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            Recharger();
        }
    }

    private void ConstruireInterface()
    {
        Text = _definition.Titre;

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

        root.Controls.Add(UiStyle.EntetePage(DeterminerIcone(), _definition.Titre), 0, 0);

        var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var searchBox = UiStyle.ChampRecherche(_definition.PlaceholderRecherche, 460, out _recherche);
        searchBox.Location = new Point(2, 4);
        _recherche.TextChanged += (_, _) => Filtrer();

        var btnNouveau = UiStyle.Bouton(_definition.LibelleBoutonNouveau, true);
        btnNouveau.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNouveau.Location = new Point(toolbar.Width - btnNouveau.Width - 2, 4);
        toolbar.Resize += (_, _) => btnNouveau.Location = new Point(Math.Max(480, toolbar.Width - btnNouveau.Width - 2), 4);
        btnNouveau.Click += (_, _) =>
        {
            if (_onNouveau is not null)
            {
                _onNouveau();
            }
            else
            {
                OuvrirEdition(null);
            }
        };

        toolbar.Controls.Add(searchBox);
        toolbar.Controls.Add(btnNouveau);
        root.Controls.Add(toolbar, 0, 1);

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
                    OuvrirEdition(id.Value);
                }
            },
            onSupprimerLigne: rowIndex =>
            {
                var id = IdLigne(rowIndex);
                if (id.HasValue)
                {
                    Supprimer(id.Value);
                }
            });

        _grille.DoubleClick += (_, _) =>
        {
            if (_grille.CurrentRow is not null)
            {
                var id = IdLigne(_grille.CurrentRow.Index);
                if (id.HasValue)
                {
                    OuvrirEdition(id.Value);
                }
            }
        };

        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        root.Controls.Add(card, 0, 2);

        root.Controls.Add(UiStyle.CreerPiedPagination(_lblTotal, _definition.DernierePageMaquette, page =>
        {
            _pageCourante = page;
            AfficherPage();
        }), 0, 3);

        Controls.Add(root);
    }

    private IconeMenu DeterminerIcone() => _definition.Cle switch
    {
        "Etudiants" => IconeMenu.Etudiants,
        "Inscriptions" => IconeMenu.Inscriptions,
        "Enseignants" => IconeMenu.Enseignants,
        "Cours" => IconeMenu.Cours,
        "Planning" => IconeMenu.Planification,
        "Salles" => IconeMenu.Salles,
        _ => IconeMenu.Etudiants
    };


    private void Recharger()
    {
        try
        {
            _donnees = _service.Lister(_definition);
            Filtrer();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Impossible de charger les données : {exception.Message}", "Erreur de base de données",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Filtrer()
    {
        if (_donnees is null)
        {
            return;
        }

        var term = _recherche.Text.Trim();
        if (term.Length == 0)
        {
            _donnees.DefaultView.RowFilter = "";
        }
        else
        {
            var escaped = term.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            var clauses = _donnees.Columns.Cast<DataColumn>()
                .Where(column => column.ColumnName != _definition.ColonneId)
                .Select(column => $"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{escaped}%'")
                .ToArray();
            _donnees.DefaultView.RowFilter = clauses.Length == 0 ? "" : string.Join(" OR ", clauses);
        }

        AfficherPage();
        MettreAJourTotal();
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, 8);
        if (_grille.Columns.Contains(_definition.ColonneId))
        {
            _grille.Columns[_definition.ColonneId]!.Visible = false;
        }

        if (_grille.Columns.Contains("AnneeAcademique"))
        {
            _grille.Columns["AnneeAcademique"]!.Visible = false;
        }

        foreach (DataGridViewColumn col in _grille.Columns)
        {
            if (col.Name is "Niveau" or "Crédit" or "Capacité" or "Statut" or "N°")
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }

    private void MettreAJourTotal()
    {
        int count = _donnees?.DefaultView.Count ?? 0;
        int affiche = string.IsNullOrWhiteSpace(_recherche.Text) && count <= 10
            ? _definition.TotalMaquette
            : count;
        string nbFormat = affiche.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        _lblTotal.Text = $"Total : {nbFormat} {_definition.LibelleTotal}";
    }

    private int? IdLigne(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _grille.Rows.Count || !_grille.Columns.Contains(_definition.ColonneId))
        {
            return null;
        }

        var val = _grille.Rows[rowIndex].Cells[_definition.ColonneId].Value;
        return val is null || val is DBNull ? null : Convert.ToInt32(val);
    }

    private void OuvrirEdition(int? id)
    {
        try
        {
            using var editor = new FrmEditionCatalogue(_service, _definition, id);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                Recharger();
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Impossible d'ouvrir le formulaire : {exception.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Supprimer(int id)
    {
        if (MessageBox.Show(
                "Confirmer la suppression de l'enregistrement sélectionné ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _service.Supprimer(_definition, id);
            Recharger();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Suppression impossible. Vérifiez que cet enregistrement n'est pas utilisé ailleurs.\n\n{exception.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
