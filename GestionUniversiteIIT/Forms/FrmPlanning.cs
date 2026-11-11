using System.Data;
using System.Text;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmPlanning : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;
    private readonly ComboBox _semaine = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _filiere = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grille = new();

    public FrmPlanning()
    {
        _definition = _service.GetDefinition("Planning");
        ConstruireInterface();
        RechargerGrille();
    }

    private void ConstruireInterface()
    {
        Text = "Planification des Cours";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 18),
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        // 1. En-tête "Planification des Cours"
        root.Controls.Add(UiStyle.EntetePage(IconeMenu.Planification, "Planification des Cours"), 0, 0);

        // 2. Filtres : "Semaine : [09 nov. – 15 nov. 2026]" à gauche et "Filière : [Génie Logiciel]" à droite
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
            Text = "Semaine :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        });
        _semaine.Items.AddRange([
            "09 nov. – 15 nov. 2026",
            "16 nov. – 22 nov. 2026",
            "23 nov. – 29 nov. 2026"
        ]);
        _semaine.SelectedIndex = 0;
        leftGroup.Controls.Add(UiStyle.EncadrerChamp(_semaine, 240, 38));
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
            Text = "Filière :",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 10, 8, 0)
        });
        _filiere.Items.AddRange([
            "Génie Logiciel",
            "Marketing",
            "Informatique",
            "Réseaux",
            "Toutes"
        ]);
        _filiere.SelectedIndex = 0;
        _filiere.SelectedIndexChanged += (_, _) => RechargerGrille();
        rightGroup.Controls.Add(UiStyle.EncadrerChamp(_filiere, 220, 38));
        rightGroup.Location = new Point(filterBar.Width - 310, 2);
        filterBar.Resize += (_, _) => rightGroup.Location = new Point(Math.Max(360, filterBar.Width - rightGroup.Width - 4), 2);
        filterBar.Controls.Add(rightGroup);

        root.Controls.Add(filterBar, 0, 1);

        // 3. Grille matricielle hebdomadaire (Heure | Lundi | Mardi | Mercredi | Jeudi | Vendredi)
        ConfigurerGrilleEmploiDuTemps();
        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        root.Controls.Add(card, 0, 2);

        // 4. Boutons d'action en bas à gauche : "+ Nouvelle planification" (rouge) + "Exporter" (bleu foncé)
        var actionsBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.White
        };

        var btnNouvelle = UiStyle.BoutonMaquette(
            "+   Nouvelle planification",
            UiStyle.Rouge,
            Color.FromArgb(148, 9, 15),
            Color.White,
            228,
            42);
        btnNouvelle.Click += (_, _) => OuvrirNouvellePlanification();

        var btnExporter = UiStyle.BoutonBleuExport("Exporter", 156, 42);
        btnExporter.Margin = new Padding(14, 4, 4, 4);
        btnExporter.Click += (_, _) => ExporterEmploiDuTemps();

        actionsBar.Controls.Add(btnNouvelle);
        actionsBar.Controls.Add(btnExporter);
        root.Controls.Add(actionsBar, 0, 3);

        Controls.Add(root);
    }

    private void ConfigurerGrilleEmploiDuTemps()
    {
        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _grille.GridColor = Color.FromArgb(204, 212, 224);
        _grille.RowTemplate.Height = 76;
        _grille.ColumnHeadersHeight = 44;
        _grille.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grille.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grille.DefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _grille.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grille.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(216, 222, 233);
        _grille.DoubleClick += (_, _) => OuvrirNouvellePlanification();
    }

    private void RechargerGrille()
    {
        var table = new DataTable();
        string[] colonnes = ["Heure", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi"];
        foreach (var col in colonnes)
        {
            table.Columns.Add(col, typeof(string));
        }

        var creneaux = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["08h00 - 10h00"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Lundi"] = "—",
                ["Mardi"] = "—",
                ["Mercredi"] = "—",
                ["Jeudi"] = "—",
                ["Vendredi"] = "—"
            },
            ["10h00 - 12h00"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Lundi"] = "—",
                ["Mardi"] = "—",
                ["Mercredi"] = "—",
                ["Jeudi"] = "—",
                ["Vendredi"] = "—"
            }
        };

        var brut = _service.Lister(_definition);
        string filiereChoisie = Convert.ToString(_filiere.SelectedItem) ?? "Génie Logiciel";

        foreach (DataRow row in brut.Rows)
        {
            string fil = Convert.ToString(row["Filière"]) ?? "";
            if (!string.Equals(filiereChoisie, "Toutes", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(fil) &&
                !string.Equals(fil, filiereChoisie, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string jour = Convert.ToString(row["JourSemaine"]) ?? "";
            string cours = Convert.ToString(row["Cours"]) ?? "";
            string code = brut.Columns.Contains("Code") ? Convert.ToString(row["Code"]) ?? "" : "";
            string salle = Convert.ToString(row["Salle"]) ?? "";

            // Formatage court fidèle à 08_planification_cours.png
            string libelleCourt = code switch
            {
                "GL101" => "Prog. C#",
                "GL102" => "BD (SQL)",
                "INF101" => "Algo.",
                "RES101" => "Réseaux",
                "GL103" => "Poo C#",
                _ => cours
            };

            TimeSpan debut = row["HeureDebut"] is TimeSpan tsD ? tsD : new TimeSpan(8, 0, 0);
            TimeSpan fin = row["HeureFin"] is TimeSpan tsF ? tsF : new TimeSpan(10, 0, 0);
            string tranche = $"{debut.Hours:00}h{debut.Minutes:00} - {fin.Hours:00}h{fin.Minutes:00}";

            if (!creneaux.TryGetValue(tranche, out var jours))
            {
                jours = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Lundi"] = "—",
                    ["Mardi"] = "—",
                    ["Mercredi"] = "—",
                    ["Jeudi"] = "—",
                    ["Vendredi"] = "—"
                };
                creneaux[tranche] = jours;
            }

            if (jours.ContainsKey(jour))
            {
                jours[jour] = $"{libelleCourt}\n(Salle {salle})";
            }
        }

        foreach (var kvp in creneaux)
        {
            table.Rows.Add(
                kvp.Key,
                kvp.Value["Lundi"],
                kvp.Value["Mardi"],
                kvp.Value["Mercredi"],
                kvp.Value["Jeudi"],
                kvp.Value["Vendredi"]);
        }

        _grille.DataSource = table;
        foreach (DataGridViewColumn col in _grille.Columns)
        {
            col.SortMode = DataGridViewColumnSortMode.NotSortable;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    private void OuvrirNouvellePlanification()
    {
        using var editor = new FrmEditionCatalogue(_service, _definition);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            RechargerGrille();
        }
    }

    private void ExporterEmploiDuTemps()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Exporter la planification des cours",
            Filter = "Fichier CSV (*.csv)|*.csv|Fichier texte (*.txt)|*.txt",
            FileName = "Planification_Cours_IIT.csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK || _grille.DataSource is not DataTable table)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
        foreach (DataRow row in table.Rows)
        {
            var cells = row.ItemArray.Select(v => $"\"{(Convert.ToString(v) ?? "").Replace("\n", " ")}\"");
            sb.AppendLine(string.Join(";", cells));
        }

        File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show("La planification a été exportée avec succès.", "Exportation terminée",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
