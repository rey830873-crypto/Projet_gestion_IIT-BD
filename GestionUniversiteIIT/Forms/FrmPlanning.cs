using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmPlanning : Form
{
    private readonly CatalogueService _service = new();
    private readonly DefinitionCatalogue _definition;

    public FrmPlanning()
    {
        _definition = _service.GetDefinition("Planning");
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            RechargerGrille();
        }
    }

    private void ConstruireInterface()
    {
        BackColor = Color.FromArgb(246, 248, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 14, 24, 14),
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.FromArgb(246, 248, 252)
        };
        UiStyle.AppliquerFondDesign(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        // 1. En-tête de la page
        root.Controls.Add(UiStyle.EntetePage(IconeMenu.Planification, "Emploi du Temps — IIT"), 0, 0);

        // 2. Filtres : Semaine | Filière | Niveau
        var filterBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 0, 0)
        };

        filterBar.Controls.Add(CreerLabelFiltre("Semaine :"));
        _semaine.Items.AddRange([
            "S3 - SEMAINE 9 (16 au 21 nov. 2026)",
            "S3 - SEMAINE 10 (23 au 28 nov. 2026)",
            "S3 - SEMAINE 11 (30 nov. au 05 déc. 2026)"
        ]);
        _semaine.SelectedIndex = 0;
        _semaine.SelectedIndexChanged += (_, _) => _pnlEnteteOfficielle.Invalidate();
        filterBar.Controls.Add(UiStyle.EncadrerChamp(_semaine, 315, 36));

        filterBar.Controls.Add(CreerLabelFiltre("Filière :", 16));
        _filiere.Items.AddRange([
            "Toutes",
            "Génie Logiciel",
            "Informatique",
            "Marketing",
            "Réseaux",
            "Data Science & IA",
            "Cybersécurité"
        ]);
        _filiere.SelectedIndex = 1;
        _filiere.SelectedIndexChanged += (_, _) => RechargerGrille();
        filterBar.Controls.Add(UiStyle.EncadrerChamp(_filiere, 195, 36));

        filterBar.Controls.Add(CreerLabelFiltre("Niveau :", 16));
        _niveau.Items.AddRange(["Tous", "L1", "L2", "L3"]);
        _niveau.SelectedIndex = 0;
        _niveau.SelectedIndexChanged += (_, _) => RechargerGrille();
        filterBar.Controls.Add(UiStyle.EncadrerChamp(_niveau, 110, 36));

        root.Controls.Add(filterBar, 0, 1);

        // 3. Carte officielle style "INSTITUT IVOIRIEN DE TECHNOLOGIE — EMPLOI DU TEMPS"
        var card = new PanneauArrondi
        {
            Dock = DockStyle.Fill,
            Rayon = 12,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(203, 213, 225),
            Padding = new Padding(12, 10, 12, 10)
        };

        var sheetLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.White
        };
        sheetLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        sheetLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sheetLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        sheetLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        // 3a. En-tête officielle IIT dessinée façon fiche PDF officielle
        _pnlEnteteOfficielle.Dock = DockStyle.Fill;
        _pnlEnteteOfficielle.Margin = Padding.Empty;
        _pnlEnteteOfficielle.BackColor = Color.White;
        _pnlEnteteOfficielle.Paint += DessinerEnteteOfficielle;
        _pnlEnteteOfficielle.Resize += (_, _) => _pnlEnteteOfficielle.Invalidate();
        sheetLayout.Controls.Add(_pnlEnteteOfficielle, 0, 0);

        // 3b. Grille hebdomadaire (HEURE | LUNDI | MARDI | MERCREDI | JEUDI | VENDREDI)
        ConfigurerGrilleEmploiDuTemps();
        _grille.Margin = Padding.Empty;
        sheetLayout.Controls.Add(_grille, 0, 1);

        // 3c. Bandeau SAMEDI (style bleu pâle officiel IIT)
        var pnlSamedi = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Color.White };
        pnlSamedi.Paint += (_, e) =>
        {
            var g = e.Graphics;
            int w = pnlSamedi.Width - 2;
            int h = pnlSamedi.Height - 2;
            if (w <= 40 || h <= 10) return;

            int col1 = Math.Max(130, w / 5);
            int col2 = Math.Max(120, w / 6);
            int col3 = w - col1 - col2;

            using var pen = new Pen(Color.FromArgb(40, 48, 60), 1.4f);
            using var blueBg = new SolidBrush(Color.FromArgb(198, 220, 240));

            g.FillRectangle(Brushes.White, 1, 1, col1 + col2, h);
            g.FillRectangle(blueBg, 1 + col1 + col2, 1, col3, h);
            g.DrawRectangle(pen, 1, 1, w, h);
            g.DrawLine(pen, 1 + col1, 1, 1 + col1, h + 1);
            g.DrawLine(pen, 1 + col1 + col2, 1, 1 + col1 + col2, h + 1);

            TextRenderer.DrawText(g, "SAMEDI  •  CAMPUS IIT", new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(4, 2, col1 - 8, h - 4), UiStyle.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, "09H00 - 14H00", new Font("Segoe UI", 9.5f, FontStyle.Bold),
                new Rectangle(col1 + 4, 2, col2 - 8, h - 4), UiStyle.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, "JOURNÉE SPORTIVE  /  ACTIVITÉS PRATIQUES AU CAMPUS", new Font("Segoe UI", 10f, FontStyle.Bold),
                new Rectangle(col1 + col2 + 6, 2, col3 - 12, h - 4), Color.FromArgb(18, 36, 68), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        pnlSamedi.Resize += (_, _) => pnlSamedi.Invalidate();
        sheetLayout.Controls.Add(pnlSamedi, 0, 2);

        // 3d. Pied de page officiel IIT
        var lblNote = new Label
        {
            Dock = DockStyle.Fill,
            Text = "NB : les emplois du temps peuvent subir des modifications selon la disponibilité des enseignants  •  IIT, l'école qui forme par la pratique !",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold | FontStyle.Italic),
            ForeColor = Color.FromArgb(51, 65, 85)
        };
        sheetLayout.Controls.Add(lblNote, 0, 3);

        card.Controls.Add(sheetLayout);
        root.Controls.Add(card, 0, 2);

        // 4. Barre d'actions en bas : + Nouvelle planification | Imprimer (format officiel) | Exporter CSV
        var actionsBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = Color.Transparent
        };

        var btnNouvelle = UiStyle.BoutonMaquette(
            "+  Nouvelle planification",
            UiStyle.Rouge,
            Color.FromArgb(148, 9, 15),
            Color.White,
            215,
            40);
        btnNouvelle.Click += (_, _) => OuvrirNouvellePlanification();

        var btnImprimer = UiStyle.BoutonBleuExport("Imprimer (IIT)", 160, 40);
        btnImprimer.Margin = new Padding(12, 4, 4, 4);
        btnImprimer.Click += (_, _) => ImprimerEmploiDuTempsOfficiel();

        var btnExporter = UiStyle.BoutonArdoise("Exporter CSV", 140, 40);
        btnExporter.Margin = new Padding(8, 4, 4, 4);
        btnExporter.Click += (_, _) => ExporterEmploiDuTemps();

        actionsBar.Controls.Add(btnNouvelle);
        actionsBar.Controls.Add(btnImprimer);
        actionsBar.Controls.Add(btnExporter);
        root.Controls.Add(actionsBar, 0, 3);

        Controls.Add(root);
    }

    private static Label CreerLabelFiltre(string texte, int leftMargin = 0) => new()
    {
        Text = texte,
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = UiStyle.Texte,
        Margin = new Padding(leftMargin, 11, 6, 0)
    };

    private void DessinerEnteteOfficielle(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int w = _pnlEnteteOfficielle.Width - 2;
        if (w <= 120) return;

        using var pen = new Pen(Color.FromArgb(30, 41, 59), 1.4f);
        using var blueIit = new SolidBrush(Color.FromArgb(42, 54, 146));
        using var grayTint = new SolidBrush(Color.FromArgb(228, 234, 242));

        // 1. Bloc logo bleu IIT à gauche (x: 1..215, y: 1..52) + Titre INSTITUT IVOIRIEN DE TECHNOLOGIE
        int logoW = 215;
        g.FillRectangle(blueIit, 1, 1, logoW, 52);
        g.DrawRectangle(pen, 1, 1, w, 52);
        g.DrawLine(pen, logoW + 1, 1, logoW + 1, 53);
        g.DrawLine(pen, logoW + 1, 30, w + 1, 30);

        TextRenderer.DrawText(g, "IIT", new Font("Segoe UI", 18f, FontStyle.Bold),
            new Rectangle(10, 5, 56, 42), Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "School of Engineering\nand Technology", new Font("Segoe UI", 8f, FontStyle.Bold),
            new Rectangle(66, 6, logoW - 72, 42), Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(g, "INSTITUT IVOIRIEN DE TECHNOLOGIE", new Font("Segoe UI", 13.5f, FontStyle.Bold),
            new Rectangle(logoW + 8, 2, w - logoW - 14, 28), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "EMPLOI DU TEMPS", new Font("Segoe UI", 10f, FontStyle.Bold),
            new Rectangle(logoW + 8, 30, w - logoW - 14, 22), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // 2. Ligne Semaine & Campus/Salle (y: 53..86)
        int splitX = Math.Max(220, w / 3);
        g.FillRectangle(Brushes.White, 1, 53, w, 34);
        g.FillRectangle(grayTint, splitX + 1, 70, w - splitX, 17);
        g.DrawRectangle(pen, 1, 53, w, 34);
        g.DrawLine(pen, splitX + 1, 53, splitX + 1, 87);
        g.DrawLine(pen, 1, 70, w + 1, 70);

        string semText = Convert.ToString(_semaine.SelectedItem) ?? "S3 - SEMAINE 9 (16 au 21 nov. 2026)";
        string semCode = semText.Contains('(') ? semText[..semText.IndexOf('(')].Trim() : semText;
        string semDates = semText.Contains('(') ? semText[(semText.IndexOf('(') + 1)..].TrimEnd(')').ToUpperInvariant() : "DU 16 AU 21 NOVEMBRE 2026";

        TextRenderer.DrawText(g, semCode, new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(4, 53, splitX - 8, 17), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, $"DU {semDates}", new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(splitX + 4, 53, w - splitX - 8, 17), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(g, "CAMPUS & SALLE", new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(4, 70, splitX - 8, 17), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, "BASSAM — CAMPUS IIT", new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(splitX + 4, 70, w - splitX - 8, 17), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // 3. Bandeau rouge Filière / Niveau (y: 87..114)
        g.FillRectangle(Brushes.White, 1, 87, w, 27);
        g.DrawRectangle(pen, 1, 87, w, 27);

        string fil = Convert.ToString(_filiere.SelectedItem) ?? "Toutes";
        string niv = Convert.ToString(_niveau.SelectedItem) ?? "Tous";
        string titreRouge = niv == "Tous" && fil == "Toutes"
            ? "TOUTES FILIÈRES — TOUS NIVEAUX"
            : $"{(niv == "Tous" ? "LICENCE" : niv)} — {fil.ToUpperInvariant()}";

        TextRenderer.DrawText(g, titreRouge, new Font("Segoe UI", 11.5f, FontStyle.Bold),
            new Rectangle(8, 88, w - 16, 25), Color.FromArgb(204, 18, 28),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    private void ConfigurerGrilleEmploiDuTemps()
    {
        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _grille.GridColor = Color.FromArgb(203, 213, 225);
        _grille.RowTemplate.Height = 140;
        _grille.ColumnHeadersHeight = 40;
        _grille.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grille.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grille.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _grille.DefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
        _grille.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grille.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        _grille.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
        _grille.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        _grille.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _grille.ScrollBars = ScrollBars.None;
        _grille.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.ColumnIndex == 0)
            {
                e.CellStyle.BackColor = Color.FromArgb(248, 250, 252);
                e.CellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            }
            else
            {
                string txt = Convert.ToString(e.Value) ?? "";
                bool occupe = !string.IsNullOrWhiteSpace(txt);
                e.CellStyle.BackColor = occupe ? Color.FromArgb(244, 248, 255) : Color.White;
                e.CellStyle.ForeColor = occupe ? Color.FromArgb(24, 31, 46) : Color.FromArgb(148, 163, 184);
            }
        };
        _grille.DataBindingComplete += (_, _) => AjusterHauteurLignesGrille();
        _grille.SizeChanged += (_, _) => AjusterHauteurLignesGrille();
        _grille.Resize += (_, _) => AjusterHauteurLignesGrille();
        _grille.DoubleClick += (_, _) => OuvrirNouvellePlanification();
    }

    private void AjusterHauteurLignesGrille()
    {
        if (_grille.Rows.Count == 0) return;
        int dispo = _grille.ClientSize.Height - _grille.ColumnHeadersHeight - 2;
        if (dispo <= 80) return;
        int rowH = Math.Max(140, dispo / _grille.Rows.Count);
        _grille.RowTemplate.Height = rowH;
        foreach (DataGridViewRow row in _grille.Rows)
        {
            row.Height = rowH;
        }
    }

    private void RechargerGrille()
    {
        _pnlEnteteOfficielle.Invalidate();

        var table = new DataTable();
        string[] colonnes = ["HEURE", "LUNDI", "MARDI", "MERCREDI", "JEUDI", "VENDREDI"];
        foreach (var col in colonnes)
        {
            table.Columns.Add(col, typeof(string));
        }

        var creneaux = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["09H00 - 12H00"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["LUNDI"] = "",
                ["MARDI"] = "",
                ["MERCREDI"] = "",
                ["JEUDI"] = "",
                ["VENDREDI"] = ""
            },
            ["13H30 - 16H30"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["LUNDI"] = "",
                ["MARDI"] = "",
                ["MERCREDI"] = "",
                ["JEUDI"] = "",
                ["VENDREDI"] = ""
            }
        };
        var brut = _service.Lister(_definition);
        string filiereChoisie = Convert.ToString(_filiere.SelectedItem) ?? "Toutes";
        string niveauChoisi = Convert.ToString(_niveau.SelectedItem) ?? "Tous";

        foreach (DataRow row in brut.Rows)
        {
            string fil = Convert.ToString(row["Filière"]) ?? "";
            string niv = Convert.ToString(row["Niveau"]) ?? "";

            if (!string.Equals(filiereChoisie, "Toutes", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(fil) &&
                !string.Equals(fil, filiereChoisie, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(niveauChoisi, "Tous", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(niv) &&
                !string.Equals(niv, niveauChoisi, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string jour = (Convert.ToString(row["JourSemaine"]) ?? "").ToUpperInvariant();
            string cours = Convert.ToString(row["Cours"]) ?? "";
            string salle = Convert.ToString(row["Salle"]) ?? "";
            string ens = brut.Columns.Contains("Enseignant") ? Convert.ToString(row["Enseignant"]) ?? "" : "";

            TimeSpan debut = row["HeureDebut"] is TimeSpan tsD ? tsD : new TimeSpan(9, 0, 0);
            string tranche = debut.Hours < 13 ? "09H00 - 12H00" : "13H30 - 16H30";

            var jours = creneaux[tranche];
            if (jours.ContainsKey(jour))
            {
                string texteCellule = string.IsNullOrWhiteSpace(ens)
                    ? $"{cours}\nSalle {salle} • {niv}"
                    : $"{cours}\n{ens}\nSalle {salle} • {niv}";
                if (string.IsNullOrEmpty(jours[jour]))
                {
                    jours[jour] = texteCellule;
                }
            }
        }

        foreach (var kvp in creneaux)
        {
            table.Rows.Add(
                kvp.Key,
                kvp.Value["LUNDI"],
                kvp.Value["MARDI"],
                kvp.Value["MERCREDI"],
                kvp.Value["JEUDI"],
                kvp.Value["VENDREDI"]);
        }

        _grille.DataSource = table;
        foreach (DataGridViewColumn col in _grille.Columns)
        {
            col.SortMode = DataGridViewColumnSortMode.NotSortable;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.FillWeight = col.Name == "HEURE" ? 65 : 100;
        }

        AjusterHauteurLignesGrille();
    }

    private void OuvrirNouvellePlanification()
    {
        using var editor = new FrmEditionCatalogue(_service, _definition);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            RechargerGrille();
        }
    }

    private void ImprimerEmploiDuTempsOfficiel()
    {
        if (_grille.DataSource is not DataTable table) return;

        using var doc = new PrintDocument();
        doc.DefaultPageSettings.Landscape = true;
        doc.DocumentName = "Emploi du Temps IIT";
        doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            var b = e.MarginBounds;
            using var pen = new Pen(Color.Black, 1.5f);
            using var titleFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            using var cellFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            int y = b.Top;
            g.DrawRectangle(pen, b.Left, y, b.Width, 56);
            g.DrawString("INSTITUT IVOIRIEN DE TECHNOLOGIE — EMPLOI DU TEMPS", titleFont, Brushes.Black,
                new RectangleF(b.Left, y + 6, b.Width, 26), new StringFormat { Alignment = StringAlignment.Center });
            string fil = Convert.ToString(_filiere.SelectedItem) ?? "Toutes";
            string niv = Convert.ToString(_niveau.SelectedItem) ?? "Tous";
            string sem = Convert.ToString(_semaine.SelectedItem) ?? "";
            g.DrawString($"{sem}  |  {niv} — {fil.ToUpperInvariant()}", subFont, Brushes.DarkRed,
                new RectangleF(b.Left, y + 32, b.Width, 22), new StringFormat { Alignment = StringAlignment.Center });

            y += 66;
            int colW = b.Width / table.Columns.Count;
            int headerH = 28;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var r = new Rectangle(b.Left + c * colW, y, colW, headerH);
                g.DrawRectangle(pen, r);
                g.DrawString(table.Columns[c].ColumnName, subFont, Brushes.Black, r,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            y += headerH;
            int rowH = Math.Min(110, Math.Max(68, (b.Height - 160) / Math.Max(1, table.Rows.Count)));
            foreach (DataRow row in table.Rows)
            {
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var r = new Rectangle(b.Left + c * colW, y, colW, rowH);
                    g.DrawRectangle(pen, r);
                    g.DrawString(Convert.ToString(row[c]) ?? "", cellFont, Brushes.Black, r,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
                y += rowH;
            }

            g.DrawString("NB : les emplois du temps peuvent subir des modifications selon la disponibilité des enseignants — IIT, l'école qui forme par la pratique !",
                new Font("Segoe UI", 8.5f, FontStyle.Italic), Brushes.DimGray,
                new RectangleF(b.Left, y + 14, b.Width, 24), new StringFormat { Alignment = StringAlignment.Center });
            e.HasMorePages = false;
        };

        using var preview = new PrintPreviewDialog
        {
            Document = doc,
            Text = "Aperçu Emploi du Temps — IIT",
            Width = 1020,
            Height = 720,
            StartPosition = FormStartPosition.CenterParent
        };
        preview.ShowDialog(this);
    }

    private void ExporterEmploiDuTemps()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Exporter la planification des cours",
            Filter = "Fichier CSV (*.csv)|*.csv|Fichier texte (*.txt)|*.txt",
            FileName = "Emploi_Du_Temps_IIT.csv"
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
