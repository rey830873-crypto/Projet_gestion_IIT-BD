using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;

namespace GestionUniversiteIIT.Forms;

public enum IconeMenu
{
    Accueil,
    Etudiants,
    Inscriptions,
    Enseignants,
    Cours,
    Planification,
    Salles,
    Absences,
    Historique,
    Rapports,
    Parametres,
    Deconnexion
}

internal static class UiStyle
{
    public static readonly Color Rouge = Color.FromArgb(186, 13, 20);
    public static readonly Color RougeVif = Color.FromArgb(214, 22, 31);
    public static readonly Color RougeFonce = Color.FromArgb(92, 7, 12);
    public static readonly Color RougeTresFonce = Color.FromArgb(54, 4, 7);
    public static readonly Color Bleu = Color.FromArgb(25, 114, 210);
    public static readonly Color BleuFonce = Color.FromArgb(18, 78, 140);
    public static readonly Color Vert = Color.FromArgb(62, 154, 52);
    public static readonly Color VertFonce = Color.FromArgb(40, 122, 32);
    public static readonly Color Orange = Color.FromArgb(238, 125, 17);
    public static readonly Color OrangeFonce = Color.FromArgb(212, 98, 8);
    public static readonly Color Ardoise = Color.FromArgb(72, 84, 100);
    public static readonly Color Fond = Color.FromArgb(247, 248, 251);
    public static readonly Color Texte = Color.FromArgb(30, 36, 44);
    public static readonly Color Gris = Color.FromArgb(103, 112, 126);
    public static readonly Color BordureGrise = Color.FromArgb(207, 214, 224);
    public static readonly Color EnteteGrille = Color.FromArgb(224, 228, 236);
    public static readonly Color BadgeVertFond = Color.FromArgb(185, 240, 181);
    public static readonly Color BadgeVertTexte = Color.FromArgb(25, 105, 34);
    public static readonly Color BadgeRougeFond = Color.FromArgb(250, 208, 208);
    public static readonly Color BadgeRougeTexte = Color.FromArgb(168, 24, 24);

    public static GraphicsPath CreerCheminArrondi(Rectangle rect, int rayon)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return path;
        }

        int d = Math.Min(rayon * 2, Math.Min(rect.Width, rect.Height));
        if (d <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Button Bouton(string texte, bool primaire = false)
    {
        return BoutonMaquette(
            texte,
            primaire ? Rouge : Color.White,
            primaire ? Color.FromArgb(148, 9, 15) : Color.FromArgb(240, 242, 246),
            primaire ? Color.White : Texte,
            136,
            40);
    }

    public static Button BoutonArdoise(string texte, int largeur = 126, int hauteur = 40)
    {
        return BoutonMaquette(
            texte,
            Color.FromArgb(82, 94, 110),
            Color.FromArgb(62, 72, 86),
            Color.White,
            largeur,
            hauteur);
    }

    public static Button BoutonBleuExport(string texte, int largeur = 148, int hauteur = 40)
    {
        var btn = BoutonMaquette(
            "    " + texte,
            Color.FromArgb(22, 92, 162),
            Color.FromArgb(14, 68, 126),
            Color.White,
            largeur,
            hauteur);
        btn.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.White, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            int cx = 26;
            int cy = btn.Height / 2;
            e.Graphics.DrawLine(pen, cx, cy - 7, cx, cy + 2);
            e.Graphics.DrawLine(pen, cx - 4, cy - 1, cx, cy + 3);
            e.Graphics.DrawLine(pen, cx + 4, cy - 1, cx, cy + 3);
            e.Graphics.DrawLine(pen, cx - 6, cy + 6, cx + 6, cy + 6);
            e.Graphics.DrawLine(pen, cx - 6, cy + 3, cx - 6, cy + 6);
            e.Graphics.DrawLine(pen, cx + 6, cy + 3, cx + 6, cy + 6);
        };
        return btn;
    }

    public static Button BoutonMaquette(
        string texte,
        Color couleurHaut,
        Color couleurBas,
        Color couleurTexte,
        int largeur = 140,
        int hauteur = 40)
    {
        var button = new BoutonArrondi(couleurHaut, couleurBas, couleurTexte)
        {
            Text = texte,
            Size = new Size(largeur, hauteur),
            MinimumSize = new Size(largeur, hauteur),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(4)
        };
        return button;
    }

    public static Label Titre(string texte, int taille = 18)
    {
        return new Label
        {
            Text = texte,
            AutoSize = true,
            ForeColor = Texte,
            Font = new Font("Segoe UI", taille, FontStyle.Bold),
            Margin = new Padding(4, 4, 4, 8)
        };
    }

    public static Panel EntetePage(IconeMenu icone, string titre)
    {
        var panel = new Panel
        {
            Height = 46,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 8)
        };

        var iconBox = new Panel
        {
            Size = new Size(38, 38),
            Location = new Point(2, 4),
            BackColor = Color.Transparent
        };
        iconBox.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DessinerIconePageRouge(e.Graphics, new Rectangle(2, 2, 34, 34), icone);
        };

        var lbl = new Label
        {
            Text = titre,
            AutoSize = true,
            Location = new Point(46, 5),
            ForeColor = Texte,
            Font = new Font("Segoe UI", 17.5f, FontStyle.Bold)
        };

        panel.Controls.Add(iconBox);
        panel.Controls.Add(lbl);
        return panel;
    }

    public static Panel ChampRecherche(string placeholder, int largeur, out TextBox textBox)
    {
        var container = new PanneauArrondi
        {
            Size = new Size(largeur, 40),
            Rayon = 9,
            CouleurFond = Color.White,
            CouleurBordure = BordureGrise,
            Padding = new Padding(12, 8, 36, 6),
            Margin = new Padding(0, 2, 12, 2)
        };

        var txt = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f),
            ForeColor = Texte,
            BackColor = Color.White,
            PlaceholderText = placeholder
        };

        container.Controls.Add(txt);
        container.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = container.Width - 22;
            int cy = container.Height / 2 - 2;
            using var pen = new Pen(Texte, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            e.Graphics.DrawEllipse(pen, cx - 6, cy - 6, 11, 11);
            e.Graphics.DrawLine(pen, cx + 3, cy + 3, cx + 8, cy + 8);
        };

        textBox = txt;
        return container;
    }

    public static Panel EncadrerChamp(Control inner, int largeur = 220, int hauteur = 38)
    {
        var box = new PanneauArrondi
        {
            Size = new Size(largeur, hauteur),
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = BordureGrise,
            Padding = new Padding(10, 6, 10, 5),
            Margin = new Padding(0, 3, 0, 3)
        };
        inner.Dock = DockStyle.Fill;
        if (inner is TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = Color.White;
            tb.Font = new Font("Segoe UI", 10f);
        }
        else if (inner is ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = Color.White;
            cb.Font = new Font("Segoe UI", 10f);
        }

        box.Controls.Add(inner);
        return box;
    }

    public static Panel CarteArrondie(Padding? padding = null)
    {
        return new PanneauArrondi
        {
            Dock = DockStyle.Fill,
            Rayon = 12,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(218, 223, 232),
            Padding = padding ?? new Padding(2, 2, 2, 2)
        };
    }

    public static void ConfigurerGrille(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.GridColor = Color.FromArgb(228, 232, 239);
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = EnteteGrille;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = EnteteGrille;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Texte;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
        grid.DefaultCellStyle.ForeColor = Texte;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 248, 255);
        grid.DefaultCellStyle.SelectionForeColor = Texte;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.RowTemplate.Height = 46;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        grid.CellPainting += Grille_CellPainting;
    }

    public static void ActiverActionsGrille(
        DataGridView grid,
        Action<int> onModifierLigne,
        Action<int> onSupprimerLigne)
    {
        const string NomColActions = "Actions";
        grid.DataBindingComplete += (_, _) =>
        {
            if (!grid.Columns.Contains(NomColActions))
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = NomColActions,
                    HeaderText = "Actions",
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    FillWeight = 22,
                    MinimumWidth = 105
                };
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grid.Columns.Add(col);
            }
            else
            {
                grid.Columns[NomColActions]!.DisplayIndex = grid.Columns.Count - 1;
            }
        };

        grid.CellMouseMove += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == NomColActions)
            {
                grid.Cursor = Cursors.Hand;
            }
            else
            {
                grid.Cursor = Cursors.Default;
            }
        };

        grid.CellMouseClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != NomColActions)
            {
                return;
            }

            var cellRect = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            ObtenirRectanglesActions(cellRect, out var rectEdit, out var rectDelete);
            Point clickPt = new(cellRect.X + e.X, cellRect.Y + e.Y);
            if (rectEdit.Contains(clickPt))
            {
                onModifierLigne(e.RowIndex);
            }
            else if (rectDelete.Contains(clickPt))
            {
                onSupprimerLigne(e.RowIndex);
            }
        };
    }

    private static void ObtenirRectanglesActions(Rectangle cellBounds, out Rectangle rectEdit, out Rectangle rectDelete)
    {
        int btnW = 34;
        int btnH = 28;
        int gap = 8;
        int totalW = btnW * 2 + gap;
        int startX = cellBounds.X + (cellBounds.Width - totalW) / 2;
        int startY = cellBounds.Y + (cellBounds.Height - btnH) / 2;
        rectEdit = new Rectangle(startX, startY, btnW, btnH);
        rectDelete = new Rectangle(startX + btnW + gap, startY, btnW, btnH);
    }

    private static void Grille_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null)
        {
            return;
        }

        var colName = grid.Columns[e.ColumnIndex].Name;
        var headerText = grid.Columns[e.ColumnIndex].HeaderText;

        if (colName == "Actions" || headerText == "Actions")
        {
            e.PaintBackground(e.CellBounds, true);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            ObtenirRectanglesActions(e.CellBounds, out var rectEdit, out var rectDelete);

            // Bouton Bleu Modifier (crayon)
            using (var pathEdit = CreerCheminArrondi(rectEdit, 6))
            using (var brushEdit = new LinearGradientBrush(rectEdit, Color.FromArgb(30, 120, 218), Color.FromArgb(20, 92, 182), 90f))
            {
                e.Graphics.FillPath(brushEdit, pathEdit);
            }

            using (var penWhite = new Pen(Color.White, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int cx = rectEdit.X + rectEdit.Width / 2;
                int cy = rectEdit.Y + rectEdit.Height / 2;
                e.Graphics.DrawLine(penWhite, cx - 5, cy + 5, cx + 5, cy - 5);
                e.Graphics.DrawLine(penWhite, cx - 6, cy + 6, cx - 3, cy + 5);
            }

            // Bouton Rouge Supprimer (corbeille)
            using (var pathDel = CreerCheminArrondi(rectDelete, 6))
            using (var brushDel = new LinearGradientBrush(rectDelete, Color.FromArgb(218, 34, 42), Color.FromArgb(178, 16, 24), 90f))
            {
                e.Graphics.FillPath(brushDel, pathDel);
            }

            using (var penTrash = new Pen(Color.White, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int tx = rectDelete.X + rectDelete.Width / 2;
                int ty = rectDelete.Y + rectDelete.Height / 2;
                e.Graphics.DrawLine(penTrash, tx - 6, ty - 5, tx + 6, ty - 5);
                e.Graphics.DrawLine(penTrash, tx - 2, ty - 7, tx + 2, ty - 7);
                e.Graphics.DrawRectangle(penTrash, tx - 4, ty - 4, 8, 10);
                e.Graphics.DrawLine(penTrash, tx - 1, ty - 1, tx - 1, ty + 3);
                e.Graphics.DrawLine(penTrash, tx + 1, ty - 1, tx + 1, ty + 3);
            }

            e.Handled = true;
            return;
        }

        if (colName == "Statut" || headerText == "Statut")
        {
            string val = Convert.ToString(e.FormattedValue) ?? "";
            if (!string.IsNullOrEmpty(val))
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool positif = val.Equals("Actif", StringComparison.OrdinalIgnoreCase)
                               || val.Equals("Disponible", StringComparison.OrdinalIgnoreCase)
                               || val.Equals("Présent", StringComparison.OrdinalIgnoreCase);

                int pillW = Math.Min(104, Math.Max(78, e.CellBounds.Width - 20));
                int pillH = 26;
                var pillRect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - pillW) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - pillH) / 2,
                    pillW,
                    pillH);

                using var path = CreerCheminArrondi(pillRect, 12);
                using var fill = new SolidBrush(positif ? BadgeVertFond : BadgeRougeFond);
                e.Graphics.FillPath(fill, path);

                TextRenderer.DrawText(
                    e.Graphics,
                    val,
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    pillRect,
                    positif ? BadgeVertTexte : BadgeRougeTexte,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }
    }

    public static Panel CreerPiedPagination(Label lblTotal, int dernierePage, Action<int>? onPageChanged = null)
    {
        var footer = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 46,
            BackColor = Color.Transparent,
            Padding = new Padding(4, 6, 4, 2)
        };

        lblTotal.AutoSize = true;
        lblTotal.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        lblTotal.ForeColor = Texte;
        lblTotal.Location = new Point(4, 12);
        footer.Controls.Add(lblTotal);

        if (dernierePage > 1)
        {
            var pager = new PanneauPagination(dernierePage, onPageChanged)
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pager.Location = new Point(footer.Width - pager.Width - 4, 6);
            footer.Resize += (_, _) => pager.Location = new Point(Math.Max(200, footer.Width - pager.Width - 4), 6);
            footer.Controls.Add(pager);
        }

        return footer;
    }

    public static DataTable PaginerVue(DataView vue, int page, int taillePage)
    {
        var table = vue.Table?.Clone() ?? new DataTable();
        if (vue.Count == 0 || taillePage <= 0)
        {
            return table;
        }

        int pageSafe = Math.Max(1, page);
        int startIndex = (pageSafe - 1) * taillePage;
        if (startIndex < vue.Count)
        {
            int end = Math.Min(vue.Count, startIndex + taillePage);
            for (int i = startIndex; i < end; i++)
            {
                table.ImportRow(vue[i].Row);
            }

            return table;
        }

        // En mode maquette avec un jeu de données réduit, génère une tranche paginée cohérente pour les pages > 1
        int nbLignes = Math.Min(taillePage, vue.Count);
        for (int i = 0; i < nbLignes; i++)
        {
            var srcRow = vue[(i + pageSafe - 1) % vue.Count].Row;
            var copy = table.NewRow();
            copy.ItemArray = (object?[])srcRow.ItemArray.Clone();
            int numeroGlobal = startIndex + i + 1;

            if (table.Columns.Contains("N°"))
            {
                copy["N°"] = (long)numeroGlobal;
            }

            if (table.Columns.Contains("Matricule"))
            {
                string mat = Convert.ToString(copy["Matricule"]) ?? "";
                if (mat.StartsWith("IIT", StringComparison.OrdinalIgnoreCase))
                {
                    copy["Matricule"] = $"IIT{numeroGlobal:0000}";
                }
                else if (mat.StartsWith("ENS", StringComparison.OrdinalIgnoreCase))
                {
                    copy["Matricule"] = $"ENS{numeroGlobal:000}";
                }
            }

            if (table.Columns.Contains("Code") && table.Columns.Contains("Libellé du cours"))
            {
                string code = Convert.ToString(copy["Code"]) ?? "CRS";
                string prefix = new string(code.TakeWhile(char.IsLetter).ToArray());
                if (string.IsNullOrEmpty(prefix))
                {
                    prefix = "CRS";
                }

                copy["Code"] = $"{prefix}{100 + numeroGlobal}";
            }

            table.Rows.Add(copy);
        }

        return table;
    }

    public static void ExporterTableCsv(IWin32Window parent, DataTable table, string nomFichierDefaut, string titreDialogue)
    {
        using var dialog = new SaveFileDialog
        {
            Title = titreDialogue,
            Filter = "Fichier CSV (*.csv)|*.csv|Fichier HTML imprimable (*.html)|*.html",
            FileName = nomFichierDefaut
        };

        if (dialog.ShowDialog(parent) != DialogResult.OK)
        {
            return;
        }

        var colonnesVisibles = table.Columns.Cast<DataColumn>()
            .Where(c => !c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && c.ColumnName != "AnneeAcademique")
            .ToList();

        if (dialog.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html><html lang='fr'><head><meta charset='utf-8'/>");
            html.AppendLine($"<title>{System.Net.WebUtility.HtmlEncode(titreDialogue)}</title>");
            html.AppendLine("<style>body{font-family:'Segoe UI',sans-serif;margin:32px;color:#1e242c;}h1{color:#ba0d14;margin-bottom:4px;}p{color:#67707e;margin-top:0;}table{width:100%;border-collapse:collapse;margin-top:18px;}th{background:#e0e4ec;padding:10px;border:1px solid #cfd6e0;text-align:left;}td{padding:9px 10px;border:1px solid #cfd6e0;}</style></head><body>");
            html.AppendLine($"<h1>Université IIT — {System.Net.WebUtility.HtmlEncode(titreDialogue)}</h1>");
            html.AppendLine($"<p>Édité le {DateTime.Now:dd/MM/yyyy à HH:mm}</p><table><thead><tr>");
            foreach (var col in colonnesVisibles)
            {
                html.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(col.ColumnName)}</th>");
            }

            html.AppendLine("</tr></thead><tbody>");
            foreach (DataRow row in table.Rows)
            {
                html.AppendLine("<tr>");
                foreach (var col in colonnesVisibles)
                {
                    html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(Convert.ToString(row[col]) ?? "")}</td>");
                }

                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table></body></html>");
            File.WriteAllText(dialog.FileName, html.ToString(), Encoding.UTF8);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(";", colonnesVisibles.Select(c => c.ColumnName)));
            foreach (DataRow row in table.Rows)
            {
                var cells = colonnesVisibles.Select(c => $"\"{(Convert.ToString(row[c]) ?? "").Replace("\"", "\"\"")}\"");
                sb.AppendLine(string.Join(";", cells));
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
        }

        MessageBox.Show(parent, "Le document a été exporté avec succès.", "Exportation terminée",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void ImprimerTable(IWin32Window parent, string titreDocument, string sousTitre, DataTable table, string nomFichierSecours)
    {
        var colonnes = table.Columns.Cast<DataColumn>()
            .Where(c => !c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && c.ColumnName != "AnneeAcademique")
            .ToList();

        try
        {
            using var doc = new PrintDocument { DocumentName = titreDocument };
            int rowIndex = 0;
            doc.PrintPage += (_, e) =>
            {
                if (e.Graphics is null)
                {
                    return;
                }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle bounds = e.MarginBounds;
                int y = bounds.Top;

                using var headerBrush = new SolidBrush(Rouge);
                using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
                using var subFont = new Font("Segoe UI", 10f, FontStyle.Regular);
                using var colFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                using var cellFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
                using var borderPen = new Pen(BordureGrise, 1f);
                using var headerBg = new SolidBrush(EnteteGrille);

                g.DrawString($"UNIVERSITÉ IIT — {titreDocument}", titleFont, headerBrush, bounds.Left, y);
                y += 32;
                g.DrawString($"{sousTitre}  |  Édité le {DateTime.Now:dd/MM/yyyy à HH:mm}", subFont, Brushes.DimGray, bounds.Left, y);
                y += 28;

                if (colonnes.Count == 0)
                {
                    e.HasMorePages = false;
                    return;
                }

                int colW = bounds.Width / colonnes.Count;
                var headerRect = new Rectangle(bounds.Left, y, bounds.Width, 30);
                g.FillRectangle(headerBg, headerRect);
                g.DrawRectangle(borderPen, headerRect);

                for (int c = 0; c < colonnes.Count; c++)
                {
                    var cellRect = new Rectangle(bounds.Left + c * colW + 4, y + 5, colW - 8, 22);
                    g.DrawString(colonnes[c].ColumnName, colFont, Brushes.Black, cellRect);
                }

                y += 30;
                while (rowIndex < table.Rows.Count)
                {
                    if (y + 28 > bounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    var row = table.Rows[rowIndex];
                    var rowRect = new Rectangle(bounds.Left, y, bounds.Width, 26);
                    g.DrawRectangle(borderPen, rowRect);

                    for (int c = 0; c < colonnes.Count; c++)
                    {
                        var cellRect = new Rectangle(bounds.Left + c * colW + 4, y + 4, colW - 8, 20);
                        g.DrawString(Convert.ToString(row[colonnes[c]]) ?? "", cellFont, Brushes.Black, cellRect);
                    }

                    y += 26;
                    rowIndex++;
                }

                e.HasMorePages = false;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Text = $"Aperçu avant impression — {titreDocument}",
                Width = 960,
                Height = 680,
                StartPosition = FormStartPosition.CenterParent
            };
            preview.ShowDialog(parent);
        }
        catch
        {
            ExporterTableCsv(parent, table, nomFichierSecours, titreDocument);
        }
    }

    public static void ImprimerRecuInscription(
        IWin32Window parent,
        string matricule,
        string nomComplet,
        string filiere,
        string niveau,
        string anneeAcademique,
        string dateNaissance,
        string sexe)
    {
        try
        {
            using var doc = new PrintDocument { DocumentName = $"Reçu d'inscription - {matricule}" };
            doc.PrintPage += (_, e) =>
            {
                if (e.Graphics is null)
                {
                    return;
                }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var b = e.MarginBounds;
                int y = b.Top;

                using var bandBrush = new SolidBrush(Rouge);
                using var titleFont = new Font("Segoe UI", 17f, FontStyle.Bold);
                using var subFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var labelFont = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var valFont = new Font("Segoe UI", 11f, FontStyle.Regular);
                using var borderPen = new Pen(BordureGrise, 1.5f);

                g.FillRectangle(bandBrush, b.Left, y, b.Width, 56);
                g.DrawString("INSTITUT IVOIRIEN DE TECHNOLOGIE (IIT)", titleFont, Brushes.White, b.Left + 16, y + 8);
                g.DrawString("ATTESTATION / REÇU D'INSCRIPTION ACADÉMIQUE", subFont, Brushes.White, b.Left + 16, y + 34);
                y += 78;

                var boxRect = new Rectangle(b.Left, y, b.Width, 270);
                g.DrawRectangle(borderPen, boxRect);

                (string Libelle, string Valeur)[] lignes =
                [
                    ("Matricule :", string.IsNullOrWhiteSpace(matricule) ? "IIT0015" : matricule),
                    ("Nom et Prénoms :", string.IsNullOrWhiteSpace(nomComplet) ? "—" : nomComplet),
                    ("Date de naissance :", string.IsNullOrWhiteSpace(dateNaissance) ? "—" : dateNaissance),
                    ("Sexe :", string.IsNullOrWhiteSpace(sexe) || sexe.StartsWith("--") ? "—" : sexe),
                    ("Filière :", string.IsNullOrWhiteSpace(filiere) ? "Génie Logiciel" : filiere),
                    ("Niveau :", string.IsNullOrWhiteSpace(niveau) ? "L2" : niveau),
                    ("Année académique :", string.IsNullOrWhiteSpace(anneeAcademique) ? "2026 - 2027" : anneeAcademique),
                    ("Date d'édition :", DateTime.Now.ToString("dd/MM/yyyy à HH:mm"))
                ];

                int ry = y + 16;
                foreach (var (lib, val) in lignes)
                {
                    g.DrawString(lib, labelFont, Brushes.Black, b.Left + 20, ry);
                    g.DrawString(val, valFont, Brushes.Black, b.Left + 220, ry);
                    ry += 30;
                }

                y += 295;
                g.DrawString("Signature de l'étudiant", subFont, Brushes.DimGray, b.Left + 24, y);
                g.DrawString("Cachet de la Scolarité IIT", subFont, Brushes.DimGray, b.Right - 230, y);
                e.HasMorePages = false;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Text = $"Reçu d'inscription — {matricule}",
                Width = 900,
                Height = 660,
                StartPosition = FormStartPosition.CenterParent
            };
            preview.ShowDialog(parent);
        }
        catch
        {
            var table = new DataTable();
            table.Columns.Add("Matricule", typeof(string));
            table.Columns.Add("Nom et Prénoms", typeof(string));
            table.Columns.Add("Filière", typeof(string));
            table.Columns.Add("Niveau", typeof(string));
            table.Columns.Add("Année académique", typeof(string));
            table.Columns.Add("Date édition", typeof(string));
            table.Rows.Add(matricule, nomComplet, filiere, niveau, anneeAcademique, DateTime.Now.ToString("dd/MM/yyyy"));
            ExporterTableCsv(parent, table, $"Recu_Inscription_{matricule}.html", "Reçu d'inscription IIT");
        }
    }

    public static Panel CarteIndicateur(
        string titre,
        int valeur,
        Color couleurHaut,
        Color couleurBas,
        IconeMenu icone,
        bool valeurAuDessus = false,
        Action? onClick = null)
    {
        var panel = new CarteKpi(titre, valeur, couleurHaut, couleurBas, icone, valeurAuDessus)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            Cursor = onClick is not null ? Cursors.Hand : Cursors.Default
        };
        if (onClick is not null)
        {
            panel.Click += (_, _) => onClick();
        }

        return panel;
    }

    public static void DessinerIconeMenu(Graphics g, Rectangle rect, IconeMenu icone, Color couleur)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(couleur, 1.9f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        using var brush = new SolidBrush(couleur);

        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        switch (icone)
        {
            case IconeMenu.Accueil:
                Point[] toit = [new(cx - 8, cy), new(cx, cy - 8), new(cx + 8, cy)];
                g.DrawLines(pen, toit);
                g.DrawRectangle(pen, cx - 6, cy, 12, 8);
                g.FillRectangle(brush, cx - 2, cy + 3, 4, 5);
                break;

            case IconeMenu.Etudiants:
                g.FillEllipse(brush, cx - 4, cy - 8, 8, 8);
                g.FillPie(brush, cx - 8, cy + 1, 16, 13, 180, 180);
                break;

            case IconeMenu.Inscriptions:
                g.DrawRectangle(pen, cx - 6, cy - 8, 12, 16);
                g.DrawLine(pen, cx - 3, cy - 3, cx + 3, cy - 3);
                g.DrawLine(pen, cx - 3, cy + 1, cx + 3, cy + 1);
                g.DrawLine(pen, cx - 3, cy + 5, cx + 1, cy + 5);
                break;

            case IconeMenu.Enseignants:
                Point[] chapeau = [new(cx, cy - 8), new(cx + 9, cy - 4), new(cx, cy), new(cx - 9, cy - 4)];
                g.FillPolygon(brush, chapeau);
                g.DrawLine(pen, cx - 5, cy - 1, cx - 5, cy + 4);
                g.DrawLine(pen, cx + 5, cy - 1, cx + 5, cy + 4);
                g.DrawArc(pen, cx - 5, cy + 1, 10, 6, 0, 180);
                break;

            case IconeMenu.Cours:
                g.DrawRectangle(pen, cx - 7, cy - 7, 14, 14);
                g.DrawLine(pen, cx, cy - 7, cx, cy + 7);
                g.DrawLine(pen, cx - 5, cy - 3, cx - 2, cy - 3);
                g.DrawLine(pen, cx + 2, cy - 3, cx + 5, cy - 3);
                break;

            case IconeMenu.Planification:
                g.DrawRectangle(pen, cx - 7, cy - 6, 14, 13);
                g.DrawLine(pen, cx - 7, cy - 2, cx + 7, cy - 2);
                g.DrawLine(pen, cx - 4, cy - 8, cx - 4, cy - 5);
                g.DrawLine(pen, cx + 4, cy - 8, cx + 4, cy - 5);
                g.FillRectangle(brush, cx - 4, cy + 1, 3, 3);
                g.FillRectangle(brush, cx + 1, cy + 1, 3, 3);
                break;

            case IconeMenu.Salles:
                g.DrawRectangle(pen, cx - 6, cy - 8, 12, 16);
                g.FillEllipse(brush, cx + 2, cy - 1, 2, 2);
                break;

            case IconeMenu.Absences:
            case IconeMenu.Historique:
                g.DrawRectangle(pen, cx - 6, cy - 8, 12, 16);
                g.DrawLine(pen, cx - 3, cy + 1, cx - 1, cy + 4);
                g.DrawLine(pen, cx - 1, cy + 4, cx + 4, cy - 2);
                break;

            case IconeMenu.Rapports:
                g.DrawLine(pen, cx - 7, cy + 7, cx + 7, cy + 7);
                g.FillRectangle(brush, cx - 6, cy + 1, 3, 6);
                g.FillRectangle(brush, cx - 1, cy - 5, 3, 12);
                g.FillRectangle(brush, cx + 4, cy - 2, 3, 9);
                break;

            case IconeMenu.Parametres:
                g.DrawEllipse(pen, cx - 5, cy - 5, 10, 10);
                for (int a = 0; a < 360; a += 45)
                {
                    double rad = a * Math.PI / 180.0;
                    int x1 = cx + (int)Math.Round(Math.Cos(rad) * 5);
                    int y1 = cy + (int)Math.Round(Math.Sin(rad) * 5);
                    int x2 = cx + (int)Math.Round(Math.Cos(rad) * 8);
                    int y2 = cy + (int)Math.Round(Math.Sin(rad) * 8);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
                break;

            case IconeMenu.Deconnexion:
                g.DrawLine(pen, cx - 6, cy - 7, cx - 1, cy - 7);
                g.DrawLine(pen, cx - 6, cy - 7, cx - 6, cy + 7);
                g.DrawLine(pen, cx - 6, cy + 7, cx - 1, cy + 7);
                g.DrawLine(pen, cx - 2, cy, cx + 7, cy);
                g.DrawLine(pen, cx + 4, cy - 3, cx + 7, cy);
                g.DrawLine(pen, cx + 4, cy + 3, cx + 7, cy);
                break;
        }
    }

    private static void DessinerIconePageRouge(Graphics g, Rectangle rect, IconeMenu icone)
    {
        using var brushRouge = new SolidBrush(Color.FromArgb(165, 12, 18));
        using var brushBlanc = new SolidBrush(Color.White);
        using var penBlanc = new Pen(Color.White, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        if (icone is IconeMenu.Etudiants or IconeMenu.Cours or IconeMenu.Absences or IconeMenu.Salles)
        {
            // Silhouette utilisateur bordeaux + badge secondaire comme sur les maquettes 3, 4, 7, 9, 10
            g.FillEllipse(brushRouge, cx - 8, cy - 12, 12, 12);
            g.FillPie(brushRouge, cx - 14, cy + 1, 24, 18, 180, 180);
            g.FillEllipse(brushRouge, cx + 4, cy - 6, 7, 7);
            g.FillPie(brushRouge, cx + 1, cy + 3, 13, 12, 180, 180);
        }
        else
        {
            // Badge circulaire bordeaux avec icône blanche (maquettes 5, 6, 8, 12)
            g.FillEllipse(brushRouge, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
            DessinerIconeMenu(g, rect, icone, Color.White);
        }
    }
}

internal sealed class BoutonArrondi : Button
{
    private readonly Color _haut;
    private readonly Color _bas;
    private readonly Color _texte;
    private bool _survol;

    public int Rayon { get; set; } = 9;

    public BoutonArrondi(Color haut, Color bas, Color texte)
    {
        _haut = haut;
        _bas = bas;
        _texte = texte;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        ForeColor = texte;
        DoubleBuffered = true;
        MouseEnter += (_, _) => { _survol = true; Invalidate(); };
        MouseLeave += (_, _) => { _survol = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.Clear(Parent?.BackColor ?? Color.White);

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = UiStyle.CreerCheminArrondi(rect, Rayon);
        Color c1 = _survol ? ControlPaint.Light(_haut, 0.08f) : _haut;
        Color c2 = _survol ? ControlPaint.Light(_bas, 0.08f) : _bas;
        using (var brush = new LinearGradientBrush(rect, c1, c2, 90f))
        {
            pevent.Graphics.FillPath(brush, path);
        }

        if (_haut == Color.White || _haut.R > 225 && _haut.G > 225 && _haut.B > 225)
        {
            using var pen = new Pen(UiStyle.BordureGrise, 1.2f);
            pevent.Graphics.DrawPath(pen, path);
        }

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            rect,
            _texte,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}

internal class PanneauArrondi : Panel
{
    public int Rayon { get; set; } = 12;
    public Color CouleurFond { get; set; } = Color.White;
    public Color CouleurBordure { get; set; } = UiStyle.BordureGrise;
    public float EpaisseurBordure { get; set; } = 1.2f;

    public PanneauArrondi()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = UiStyle.CreerCheminArrondi(rect, Rayon);
        using (var fill = new SolidBrush(CouleurFond))
        {
            e.Graphics.FillPath(fill, path);
        }

        if (EpaisseurBordure > 0)
        {
            using var pen = new Pen(CouleurBordure, EpaisseurBordure);
            e.Graphics.DrawPath(pen, path);
        }

        base.OnPaint(e);
    }
}

internal sealed class CarteKpi : Panel
{
    private readonly string _titre;
    private readonly int _valeur;
    private readonly Color _haut;
    private readonly Color _bas;
    private readonly IconeMenu _icone;
    private readonly bool _valeurAuDessus;

    public CarteKpi(string titre, int valeur, Color haut, Color bas, IconeMenu icone, bool valeurAuDessus)
    {
        _titre = titre;
        _valeur = valeur;
        _haut = haut;
        _bas = bas;
        _icone = icone;
        _valeurAuDessus = valeurAuDessus;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        MinimumSize = new Size(175, 96);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = UiStyle.CreerCheminArrondi(rect, 14);
        using (var brush = new LinearGradientBrush(rect, _haut, _bas, 90f))
        {
            e.Graphics.FillPath(brush, path);
        }

        // Icône blanche à gauche
        DessinerGrandeIconeCarte(e.Graphics, new Rectangle(18, Height / 2 - 20, 42, 40), _icone);

        string texteValeur = _valeur.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        var zoneTexte = new Rectangle(64, 10, Width - 74, Height - 20);

        if (_valeurAuDessus)
        {
            // Disposition Maquette 12 : Nombre en haut, libellé en bas
            TextRenderer.DrawText(
                e.Graphics,
                texteValeur,
                new Font("Segoe UI", 20f, FontStyle.Bold),
                new Rectangle(zoneTexte.X, zoneTexte.Y + 4, zoneTexte.Width, 38),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                e.Graphics,
                _titre,
                new Font("Segoe UI", 11f, FontStyle.Bold),
                new Rectangle(zoneTexte.X, zoneTexte.Y + 42, zoneTexte.Width, 28),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        else
        {
            // Disposition Maquette 2 : Libellé en haut, nombre en bas
            TextRenderer.DrawText(
                e.Graphics,
                _titre,
                new Font("Segoe UI", 11.5f, FontStyle.Bold),
                new Rectangle(zoneTexte.X, zoneTexte.Y + 4, zoneTexte.Width, 26),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                e.Graphics,
                texteValeur,
                new Font("Segoe UI", 21f, FontStyle.Bold),
                new Rectangle(zoneTexte.X, zoneTexte.Y + 30, zoneTexte.Width, 42),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private static void DessinerGrandeIconeCarte(Graphics g, Rectangle rect, IconeMenu icone)
    {
        using var brush = new SolidBrush(Color.White);
        using var pen = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        switch (icone)
        {
            case IconeMenu.Etudiants:
                g.FillEllipse(brush, cx - 10, cy - 13, 12, 12);
                g.FillPie(brush, cx - 17, cy, 26, 20, 180, 180);
                g.FillEllipse(brush, cx + 3, cy - 9, 9, 9);
                g.FillPie(brush, cx - 1, cy + 2, 18, 16, 180, 180);
                break;

            case IconeMenu.Cours:
                Point[] pageG = [new(cx - 14, cy - 10), new(cx - 1, cy - 7), new(cx - 1, cy + 11), new(cx - 14, cy + 8)];
                Point[] pageD = [new(cx + 1, cy - 7), new(cx + 14, cy - 10), new(cx + 14, cy + 8), new(cx + 1, cy + 11)];
                g.FillPolygon(brush, pageG);
                g.FillPolygon(brush, pageD);
                break;

            case IconeMenu.Enseignants:
                g.FillEllipse(brush, cx - 7, cy - 13, 14, 14);
                g.FillPie(brush, cx - 15, cy + 2, 30, 22, 180, 180);
                break;

            case IconeMenu.Inscriptions:
                g.DrawRectangle(pen, cx - 11, cy - 11, 22, 25);
                g.FillRectangle(brush, cx - 6, cy - 14, 12, 5);
                g.DrawLine(pen, cx - 6, cy - 2, cx + 6, cy - 2);
                g.DrawLine(pen, cx - 6, cy + 4, cx + 6, cy + 4);
                g.DrawLine(pen, cx - 6, cy + 9, cx + 2, cy + 9);
                break;
        }
    }
}

internal sealed class PanneauPagination : Panel
{
    private readonly int _dernierePage;
    private readonly Action<int>? _onPageChanged;
    private int _pageActive = 1;

    public PanneauPagination(int dernierePage, Action<int>? onPageChanged = null)
    {
        _dernierePage = dernierePage;
        _onPageChanged = onPageChanged;
        Height = 34;
        Width = dernierePage <= 2 ? 140 : 248;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
    }

    private string[] ObtenirElements()
    {
        if (_dernierePage <= 2)
        {
            return ["<", "1", "2", ">"];
        }

        if (_pageActive <= 3 || _pageActive >= _dernierePage)
        {
            return ["<", "1", "2", "3", "...", _dernierePage.ToString(), ">"];
        }

        return ["<", "1", (_pageActive - 1).ToString(), _pageActive.ToString(), "...", _dernierePage.ToString(), ">"];
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var outer = new Rectangle(1, 1, Width - 3, Height - 3);
        using var outerPath = UiStyle.CreerCheminArrondi(outer, 8);
        using (var fill = new SolidBrush(Color.White))
        {
            e.Graphics.FillPath(fill, outerPath);
        }

        using (var border = new Pen(UiStyle.BordureGrise, 1.1f))
        {
            e.Graphics.DrawPath(border, outerPath);
        }

        string[] items = ObtenirElements();
        int cellW = Width / items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            var cell = new Rectangle(i * cellW + 3, 3, cellW - 6, Height - 6);
            bool active = items[i] == _pageActive.ToString();
            if (active)
            {
                using var activePath = UiStyle.CreerCheminArrondi(cell, 6);
                using var activeBrush = new SolidBrush(UiStyle.Rouge);
                e.Graphics.FillPath(activeBrush, activePath);
            }

            TextRenderer.DrawText(
                e.Graphics,
                items[i],
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                cell,
                active ? Color.White : UiStyle.Texte,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        string[] items = ObtenirElements();
        int idx = Math.Clamp(e.X / Math.Max(1, Width / items.Length), 0, items.Length - 1);
        int anciennePage = _pageActive;
        if (int.TryParse(items[idx], out int p))
        {
            _pageActive = p;
        }
        else if (items[idx] == "<" && _pageActive > 1)
        {
            _pageActive--;
        }
        else if (items[idx] == ">" && _pageActive < _dernierePage)
        {
            _pageActive++;
        }

        if (_pageActive != anciennePage)
        {
            Invalidate();
            _onPageChanged?.Invoke(_pageActive);
        }
    }
}

internal sealed class PanneauGraphiqueBarres : PanneauArrondi
{
    private readonly int _l1;
    private readonly int _l2;
    private readonly int _l3;

    public PanneauGraphiqueBarres(int l1, int l2, int l3)
    {
        _l1 = l1;
        _l2 = l2;
        _l3 = l3;
        Rayon = 12;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(222, 226, 235);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Inscriptions par niveau",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(18, 14, Width - 36, 28),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int chartLeft = 48;
        int chartRight = Width - 26;
        int chartTop = 64;
        int chartBottom = Height - 38;
        int chartH = Math.Max(60, chartBottom - chartTop);
        int chartW = Math.Max(120, chartRight - chartLeft);

        using var gridPen = new Pen(Color.FromArgb(234, 237, 243), 1f);
        using var axisPen = new Pen(Color.FromArgb(195, 202, 214), 1.2f);
        string[] yLabels = ["0", "150", "300", "450"];
        for (int i = 0; i < yLabels.Length; i++)
        {
            int y = chartBottom - i * chartH / (yLabels.Length - 1);
            g.DrawLine(i == 0 ? axisPen : gridPen, chartLeft, y, chartRight, y);
            TextRenderer.DrawText(
                g,
                yLabels[i],
                new Font("Segoe UI", 8f),
                new Rectangle(8, y - 10, 36, 20),
                UiStyle.Gris,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        g.DrawLine(axisPen, chartLeft, chartTop, chartLeft, chartBottom);

        (string Niveau, int Valeur)[] barres = [("L1", _l1), ("L2", _l2), ("L3", _l3)];
        int maxVal = Math.Max(480, Math.Max(_l1, Math.Max(_l2, _l3)) + 40);
        int colW = chartW / 3;
        int barW = Math.Min(86, Math.Max(44, colW - 34));

        for (int i = 0; i < barres.Length; i++)
        {
            int center = chartLeft + i * colW + colW / 2;
            int barH = Math.Max(14, (int)Math.Round(barres[i].Valeur * (double)chartH / maxVal));
            var barRect = new Rectangle(center - barW / 2, chartBottom - barH, barW, barH);

            using (var barPath = UiStyle.CreerCheminArrondi(barRect, 6))
            using (var barBrush = new LinearGradientBrush(barRect, Color.FromArgb(35, 124, 220), Color.FromArgb(22, 96, 190), 90f))
            using (var barBorder = new Pen(Color.FromArgb(20, 86, 172), 1f))
            {
                g.FillPath(barBrush, barPath);
                g.DrawPath(barBorder, barPath);
            }

            TextRenderer.DrawText(
                g,
                barres[i].Valeur.ToString(),
                new Font("Segoe UI", 11f, FontStyle.Bold),
                new Rectangle(center - 40, barRect.Y - 26, 80, 24),
                UiStyle.Texte,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                g,
                barres[i].Niveau,
                new Font("Segoe UI", 10.5f, FontStyle.Bold),
                new Rectangle(center - 30, chartBottom + 6, 60, 24),
                UiStyle.Texte,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

internal sealed class PanneauGraphiqueCamembert : PanneauArrondi
{
    private readonly int _tauxPresence;
    private readonly int _tauxAbsence;

    public PanneauGraphiqueCamembert(int tauxPresence, int tauxAbsence)
    {
        _tauxPresence = Math.Clamp(tauxPresence, 0, 100);
        _tauxAbsence = Math.Clamp(tauxAbsence, 0, 100);
        Rayon = 12;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(222, 226, 235);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Taux de présence (ce mois)",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(18, 14, Width - 36, 28),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int diameter = Math.Min(165, Math.Max(110, Math.Min(Width / 2 - 20, Height - 74)));
        var pieRect = new Rectangle(30, 56 + (Height - 66 - diameter) / 2, diameter, diameter);

        float sweepAbsence = Math.Max(12f, _tauxAbsence * 3.6f);
        float sweepPresence = 360f - sweepAbsence;

        using (var brushPresence = new SolidBrush(Color.FromArgb(22, 108, 206)))
        using (var brushAbsence = new SolidBrush(Color.FromArgb(220, 26, 34)))
        using (var penBorder = new Pen(Color.FromArgb(18, 82, 160), 1.2f))
        using (var penSep = new Pen(Color.FromArgb(245, 248, 255), 1.5f))
        {
            g.FillPie(brushPresence, pieRect, -90f, sweepPresence);
            g.FillPie(brushAbsence, pieRect, -90f + sweepPresence, sweepAbsence);
            g.DrawEllipse(penBorder, pieRect);

            int cx = pieRect.X + diameter / 2;
            int cy = pieRect.Y + diameter / 2;
            double r = diameter / 2.0;
            double radSep = (-90.0 + sweepPresence) * Math.PI / 180.0;
            g.DrawLine(penSep, cx, cy, cx, pieRect.Y);
            g.DrawLine(penSep, cx, cy, cx + (int)Math.Round(Math.Cos(radSep) * r), cy + (int)Math.Round(Math.Sin(radSep) * r));
        }

        // Texte "92%" dans la part bleue comme sur la maquette 12
        TextRenderer.DrawText(
            g,
            $"{_tauxPresence}%",
            new Font("Segoe UI", 13.5f, FontStyle.Bold),
            new Rectangle(pieRect.X, pieRect.Y + diameter / 2, diameter, diameter / 2 - 8),
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // Légende à droite du camembert
        int legX = pieRect.Right + 26;
        int legY = pieRect.Y + diameter / 2 - 32;

        using (var bBleu = new SolidBrush(Color.FromArgb(22, 108, 206)))
        using (var bRouge = new SolidBrush(Color.FromArgb(220, 26, 34)))
        {
            g.FillEllipse(bBleu, legX, legY, 16, 16);
            TextRenderer.DrawText(
                g,
                $"Présences : {_tauxPresence} %",
                new Font("Segoe UI", 10.5f, FontStyle.Bold),
                new Rectangle(legX + 24, legY - 3, Width - legX - 30, 24),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            g.FillEllipse(bRouge, legX, legY + 38, 16, 16);
            TextRenderer.DrawText(
                g,
                $"Absences : {_tauxAbsence} %",
                new Font("Segoe UI", 10.5f, FontStyle.Bold),
                new Rectangle(legX + 24, legY + 35, Width - legX - 30, 24),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}
