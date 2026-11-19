using System.Data;
using System.Drawing.Drawing2D;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public partial class FrmCatalogue : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;
    private readonly Action? _onNouveau;
    private DataTable? _donnees;
    private int _pageCourante = 1;
    private int _dernierePageAffichee = -1;
    private int _taillePageActuelle = 8;

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

    private Color CouleurAccentModule() => _definition.Cle switch
    {
        "Etudiants" => Color.FromArgb(37, 99, 235),
        "Enseignants" => Color.FromArgb(16, 185, 129),
        "Cours" => Color.FromArgb(186, 13, 20),
        "Salles" => Color.FromArgb(245, 158, 11),
        _ => UiStyle.Rouge
    };

    private string LibelleFiltreSecondaire() => _definition.Cle switch
    {
        "Etudiants" => "Niveau :",
        "Enseignants" => "Spécialité :",
        "Cours" => "Filière :",
        "Salles" => "Type :",
        _ => "Filtre :"
    };

    private void ConstruireInterface()
    {
        Text = _definition.Titre;
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

        // 1. En-tête du module
        root.Controls.Add(UiStyle.EntetePage(DeterminerIcone(), _definition.Titre), 0, 0);

        // 2. Bandeau de synthèse contextuel propre à chaque module
        _pnlResume.Dock = DockStyle.Fill;
        _pnlResume.FlowDirection = FlowDirection.LeftToRight;
        _pnlResume.WrapContents = false;
        _pnlResume.BackColor = Color.Transparent;
        _pnlResume.Padding = new Padding(0, 2, 0, 6);
        root.Controls.Add(_pnlResume, 0, 1);

        // 3. Barre d'outils : Recherche + Filtre secondaire + Exporter + Nouveau
        var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var leftTools = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var searchBox = UiStyle.ChampRecherche(_definition.PlaceholderRecherche, 320, out _recherche);
        _recherche.TextChanged += (_, _) =>
        {
            _pageCourante = 1;
            Filtrer();
        };
        leftTools.Controls.Add(searchBox);

        leftTools.Controls.Add(new Label
        {
            Text = LibelleFiltreSecondaire(),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(8, 12, 6, 0)
        });

        _filtreSecondaire.Items.Add("Tous");
        _filtreSecondaire.SelectedIndex = 0;
        _filtreSecondaire.SelectedIndexChanged += (_, _) =>
        {
            _pageCourante = 1;
            Filtrer();
        };
        leftTools.Controls.Add(UiStyle.EncadrerChamp(_filtreSecondaire, 165, 38));
        toolbar.Controls.Add(leftTools);

        var rightTools = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent
        };

        var btnNouveau = UiStyle.Bouton(_definition.LibelleBoutonNouveau, true);
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

        var btnExport = UiStyle.BoutonBleuExport("Exporter", 132, 40);
        btnExport.Click += (_, _) =>
        {
            if (_donnees is not null)
            {
                UiStyle.ExporterTableCsv(
                    this,
                    _donnees.DefaultView.ToTable(),
                    $"{_definition.Cle}_IIT.csv",
                    $"Exporter — {_definition.Titre}");
            }
        };

        rightTools.Controls.Add(btnNouveau);
        rightTools.Controls.Add(btnExport);
        toolbar.Controls.Add(rightTools);
        root.Controls.Add(toolbar, 0, 2);

        // 4. Grille dans une carte blanche moderne
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

        // 5. Pied de pagination dynamique
        _footerHost.Dock = DockStyle.Fill;
        _footerHost.BackColor = Color.Transparent;
        root.Controls.Add(_footerHost, 0, 4);

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
            AlimenterFiltreSecondaire();
            ConstruireBandeauResume();
            Filtrer();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Impossible de charger les données : {exception.Message}", "Erreur de base de données",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string? ColonneFiltreSecondaire() => _definition.Cle switch
    {
        "Etudiants" => "Niveau",
        "Enseignants" => "Spécialité",
        "Cours" => "Filière",
        "Salles" => "Type",
        _ => null
    };

    private void AlimenterFiltreSecondaire()
    {
        string? col = ColonneFiltreSecondaire();
        string precedent = Convert.ToString(_filtreSecondaire.SelectedItem) ?? "Tous";
        _filtreSecondaire.Items.Clear();
        _filtreSecondaire.Items.Add("Tous");

        if (_donnees is not null && col is not null && _donnees.Columns.Contains(col))
        {
            var valeurs = _donnees.AsEnumerable()
                .Select(r => Convert.ToString(r[col])?.Trim() ?? "")
                .Where(v => v.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v);

            foreach (var v in valeurs)
            {
                _filtreSecondaire.Items.Add(v);
            }
        }

        int idx = _filtreSecondaire.Items.IndexOf(precedent);
        _filtreSecondaire.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private void ConstruireBandeauResume()
    {
        _pnlResume.Controls.Clear();
        if (_donnees is null) return;

        Color accent = CouleurAccentModule();
        var rows = _donnees.AsEnumerable().ToList();

        switch (_definition.Cle)
        {
            case "Etudiants":
                int actifsEtu = _donnees.Columns.Contains("Statut")
                    ? rows.Count(r => string.Equals(Convert.ToString(r["Statut"]), "Actif", StringComparison.OrdinalIgnoreCase))
                    : rows.Count;
                int l1 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L1", StringComparison.OrdinalIgnoreCase)) : 0;
                int l2 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L2", StringComparison.OrdinalIgnoreCase)) : 0;
                int l3 = _donnees.Columns.Contains("Niveau") ? rows.Count(r => string.Equals(Convert.ToString(r["Niveau"]), "L3", StringComparison.OrdinalIgnoreCase)) : 0;
                _pnlResume.Controls.Add(CreerBadgeResume("Total étudiants", rows.Count.ToString(), accent));
                _pnlResume.Controls.Add(CreerBadgeResume("Actifs", actifsEtu.ToString(), Color.FromArgb(16, 185, 129)));
                _pnlResume.Controls.Add(CreerBadgeResume("Niveaux", $"L1: {l1}  •  L2: {l2}  •  L3: {l3}", Color.FromArgb(71, 85, 105)));
                break;

            case "Enseignants":
                int specs = _donnees.Columns.Contains("Spécialité")
                    ? rows.Select(r => Convert.ToString(r["Spécialité"]) ?? "").Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    : 0;
                int avecContact = _donnees.Columns.Contains("Contact")
                    ? rows.Count(r => !string.IsNullOrWhiteSpace(Convert.ToString(r["Contact"])))
                    : 0;
                _pnlResume.Controls.Add(CreerBadgeResume("Total enseignants", rows.Count.ToString(), accent));
                _pnlResume.Controls.Add(CreerBadgeResume("Spécialités", specs.ToString(), Color.FromArgb(139, 92, 246)));
                _pnlResume.Controls.Add(CreerBadgeResume("Joignables", avecContact.ToString(), Color.FromArgb(37, 99, 235)));
                break;

            case "Cours":
                int totalCredits = _donnees.Columns.Contains("Crédit")
                    ? rows.Sum(r => int.TryParse(Convert.ToString(r["Crédit"]), out int c) ? c : 0)
                    : 0;
                int nbFil = _donnees.Columns.Contains("Filière")
                    ? rows.Select(r => Convert.ToString(r["Filière"]) ?? "").Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    : 0;
                _pnlResume.Controls.Add(CreerBadgeResume("Total cours", rows.Count.ToString(), accent));
                _pnlResume.Controls.Add(CreerBadgeResume("Crédits cumulés", totalCredits.ToString(), Color.FromArgb(37, 99, 235)));
                _pnlResume.Controls.Add(CreerBadgeResume("Filières couvertes", nbFil.ToString(), Color.FromArgb(16, 185, 129)));
                break;

            case "Salles":
                int activesSalles = _donnees.Columns.Contains("Statut")
                    ? rows.Count(r => string.Equals(Convert.ToString(r["Statut"]), "Disponible", StringComparison.OrdinalIgnoreCase))
                    : 0;
                int capTotale = _donnees.Columns.Contains("Capacité")
                    ? rows.Sum(r => int.TryParse(Convert.ToString(r["Capacité"]), out int c) ? c : 0)
                    : 0;
                _pnlResume.Controls.Add(CreerBadgeResume("Total salles", rows.Count.ToString(), accent));
                _pnlResume.Controls.Add(CreerBadgeResume("Disponibles", activesSalles.ToString(), Color.FromArgb(16, 185, 129)));
                _pnlResume.Controls.Add(CreerBadgeResume("Capacité totale", $"{capTotale} places", Color.FromArgb(37, 99, 235)));
                break;
        }
    }

    private static Panel CreerBadgeResume(string libelle, string valeur, Color accent)
    {
        var card = new PanneauArrondi
        {
            Size = new Size(Math.Max(165, 110 + valeur.Length * 7), 48),
            Rayon = 10,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(226, 232, 240),
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(14, 6, 14, 6)
        };
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var bar = new SolidBrush(accent);
            e.Graphics.FillRectangle(bar, 8, 10, 4, card.Height - 20);
        };

        card.Controls.Add(new Label
        {
            Text = valeur,
            AutoSize = true,
            Location = new Point(18, 22),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte
        });
        card.Controls.Add(new Label
        {
            Text = libelle.ToUpperInvariant(),
            AutoSize = true,
            Location = new Point(18, 6),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139)
        });
        return card;
    }

    private void Filtrer()
    {
        if (_donnees is null)
        {
            return;
        }

        var parts = new List<string>();
        var term = _recherche.Text.Trim();
        if (term.Length > 0)
        {
            var escaped = term.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            var clauses = _donnees.Columns.Cast<DataColumn>()
                .Where(column => column.ColumnName != _definition.ColonneId)
                .Select(column => $"CONVERT([{column.ColumnName}], 'System.String') LIKE '%{escaped}%'")
                .ToArray();
            if (clauses.Length > 0)
            {
                parts.Add("(" + string.Join(" OR ", clauses) + ")");
            }
        }

        string? colSec = ColonneFiltreSecondaire();
        string valSec = Convert.ToString(_filtreSecondaire.SelectedItem) ?? "Tous";
        if (colSec is not null && _donnees.Columns.Contains(colSec) && !string.Equals(valSec, "Tous", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add($"[{colSec}] = '{valSec.Replace("'", "''")}'");
        }

        _donnees.DefaultView.RowFilter = string.Join(" AND ", parts);

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
        MettreAJourTotal();
    }

    private void AfficherPage()
    {
        if (_donnees is null)
        {
            return;
        }

        _grille.DataSource = UiStyle.PaginerVue(_donnees.DefaultView, _pageCourante, _taillePageActuelle);
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
            if (col.Name is "Nom et Prénoms" or "Libellé du cours")
            {
                col.FillWeight = 175;
            }
            else if (col.Name is "Matricule" or "Code" or "Salle")
            {
                col.FillWeight = 68;
            }
            else if (col.Name is "Niveau" or "Crédit" or "Capacité" or "Statut" or "N°")
            {
                col.FillWeight = 58;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }

    private void MettreAJourTotal()
    {
        int count = _donnees?.DefaultView.Count ?? 0;
        string nbFormat = count.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
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
