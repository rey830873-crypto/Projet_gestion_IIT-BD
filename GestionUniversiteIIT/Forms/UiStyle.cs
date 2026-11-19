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

    public static void AppliquerFondDesign(Control conteneur)
    {
        conteneur.BackColor = Color.FromArgb(246, 248, 252);
        conteneur.Resize += (_, _) => conteneur.Invalidate();
        conteneur.Paint += (_, e) =>
        {
            var r = conteneur.ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new LinearGradientBrush(r, Color.FromArgb(249, 250, 252), Color.FromArgb(238, 242, 248), 35f))
            {
                g.FillRectangle(bg, r);
            }

            // Trame de points géométrique subtile en haut à droite et en bas à gauche
            using (var dotBrush = new SolidBrush(Color.FromArgb(22, 100, 116, 139)))
            {
                for (int x = Math.Max(0, r.Width - 260); x < r.Width - 16; x += 18)
                {
                    for (int y = 16; y < Math.Min(r.Height, 200); y += 18)
                    {
                        g.FillEllipse(dotBrush, x, y, 2.2f, 2.2f);
                    }
                }
                for (int x = 16; x < Math.Min(r.Width, 260); x += 18)
                {
                    for (int y = Math.Max(0, r.Height - 200); y < r.Height - 16; y += 18)
                    {
                        g.FillEllipse(dotBrush, x, y, 2.2f, 2.2f);
                    }
                }
            }

            // Rubans courbes translucides en haut à droite (bordeaux & bleu marine IIT)
            using (var pathTop1 = new GraphicsPath())
            {
                pathTop1.AddBezier(r.Width * 0.58f, -10, r.Width * 0.72f, 55, r.Width * 0.88f, 22, r.Width + 20, 145);
                pathTop1.AddLine(r.Width + 20, -10, r.Width * 0.58f, -10);
                pathTop1.CloseFigure();
                using var bTop1 = new SolidBrush(Color.FromArgb(22, 136, 10, 16));
                g.FillPath(bTop1, pathTop1);
            }
            using (var pathTop2 = new GraphicsPath())
            {
                pathTop2.AddBezier(r.Width * 0.68f, -10, r.Width * 0.80f, 38, r.Width * 0.92f, 75, r.Width + 20, 220);
                pathTop2.AddLine(r.Width + 20, -10, r.Width * 0.68f, -10);
                pathTop2.CloseFigure();
                using var bTop2 = new SolidBrush(Color.FromArgb(16, 30, 52, 82));
                g.FillPath(bTop2, pathTop2);
            }

            // Rubans courbes translucides en bas à gauche
            using (var pathBot1 = new GraphicsPath())
            {
                pathBot1.AddBezier(-20, r.Height - 170, r.Width * 0.08f, r.Height - 55, r.Width * 0.24f, r.Height - 85, r.Width * 0.38f, r.Height + 10);
                pathBot1.AddLine(-20, r.Height + 10, -20, r.Height - 170);
                pathBot1.CloseFigure();
                using var bBot1 = new SolidBrush(Color.FromArgb(20, 136, 10, 16));
                g.FillPath(bBot1, pathBot1);
            }
            using (var pathBot2 = new GraphicsPath())
            {
                pathBot2.AddBezier(-20, r.Height - 240, r.Width * 0.05f, r.Height - 105, r.Width * 0.16f, r.Height - 40, r.Width * 0.28f, r.Height + 10);
                pathBot2.AddLine(-20, r.Height + 10, -20, r.Height - 240);
                pathBot2.CloseFigure();
                using var bBot2 = new SolidBrush(Color.FromArgb(15, 30, 52, 82));
                g.FillPath(bBot2, pathBot2);
            }
        };
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
            UseMnemonic = false,
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

    public static int CalculerTaillePage(DataGridView grid, int minimum = 8)
    {
        int headerH = grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
        int rowH = Math.Max(1, grid.RowTemplate.Height);
        int dispo = grid.ClientSize.Height - headerH - 4;
        if (dispo <= rowH)
        {
            return minimum;
        }

        return Math.Max(minimum, dispo / rowH);
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

    public static Panel CarteIndicateurModerne(
        string titre,
        int valeur,
        Color couleurHaut,
        Color couleurBas,
        IconeMenu icone,
        bool valeurAuDessus = true,
        Action? onClick = null)
    {
        var panel = new CarteKpiModerne(titre, valeur, couleurHaut, couleurBas, icone, valeurAuDessus)
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

        DessinerGrandeIconeCarte(e.Graphics, new Rectangle(18, Height / 2 - 20, 42, 40), _icone);

        string texteValeur = _valeur.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        var zoneTexte = new Rectangle(64, 10, Width - 74, Height - 20);

        if (_valeurAuDessus)
        {
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

internal sealed class CarteKpiModerne : Panel
{
    private readonly string _titre;
    private readonly int _valeur;
    private readonly Color _haut;
    private readonly Color _bas;
    private readonly IconeMenu _icone;
    private readonly bool _valeurAuDessus;
    private bool _survole;

    public CarteKpiModerne(string titre, int valeur, Color haut, Color bas, IconeMenu icone, bool valeurAuDessus)
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

        MouseEnter += (_, _) => { _survole = true; Invalidate(); };
        MouseLeave += (_, _) => { _survole = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(2, 2, Width - 5, Height - 5);
        using var path = UiStyle.CreerCheminArrondi(rect, 14);

        // Ombre douce sous la carte
        var shadowRect = new Rectangle(rect.X + 1, rect.Y + 3, rect.Width, rect.Height);
        using (var shadowPath = UiStyle.CreerCheminArrondi(shadowRect, 14))
        using (var shadowBrush = new SolidBrush(Color.FromArgb(_survole ? 22 : 12, 15, 23, 42)))
        {
            e.Graphics.FillPath(shadowBrush, shadowPath);
        }

        // Fond blanc moderne (légère teinte au survol)
        Color bg = _survole ? Color.FromArgb(250, 251, 255) : Color.White;
        using (var fill = new SolidBrush(bg))
        {
            e.Graphics.FillPath(fill, path);
        }

        // Bordure fine (accentuée au survol)
        using (var border = new Pen(_survole ? Color.FromArgb(160, _haut) : Color.FromArgb(226, 232, 240), _survole ? 1.5f : 1.1f))
        {
            e.Graphics.DrawPath(border, path);
        }

        // Barre d'accentuation colorée à gauche
        var accentRect = new Rectangle(rect.X + 10, rect.Y + 14, 5, rect.Height - 28);
        using (var accentPath = UiStyle.CreerCheminArrondi(accentRect, 3))
        using (var accentBrush = new LinearGradientBrush(accentRect, _haut, _bas, 90f))
        {
            e.Graphics.FillPath(accentBrush, accentPath);
        }

        // Pastille pastel avec icône à droite
        int iconSize = 46;
        var iconCircle = new Rectangle(rect.Right - iconSize - 16, rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize);
        using (var circleBrush = new SolidBrush(Color.FromArgb(26, _haut)))
        {
            e.Graphics.FillEllipse(circleBrush, iconCircle);
        }
        DessinerGrandeIconeCarte(e.Graphics, iconCircle, _icone, _haut);

        string texteValeur = _valeur.ToString("N0").Replace(",", " ").Replace("\u00A0", " ");
        int textLeft = rect.X + 26;
        int textWidth = Math.Max(80, iconCircle.Left - textLeft - 8);

        if (_valeurAuDessus)
        {
            TextRenderer.DrawText(
                e.Graphics,
                texteValeur,
                new Font("Segoe UI", 21f, FontStyle.Bold),
                new Rectangle(textLeft, rect.Y + 14, textWidth, 38),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                e.Graphics,
                _titre.ToUpperInvariant(),
                new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(textLeft, rect.Y + 52, textWidth, 24),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
        else
        {
            TextRenderer.DrawText(
                e.Graphics,
                _titre.ToUpperInvariant(),
                new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(textLeft, rect.Y + 14, textWidth, 22),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                e.Graphics,
                texteValeur,
                new Font("Segoe UI", 22f, FontStyle.Bold),
                new Rectangle(textLeft, rect.Y + 36, textWidth, 40),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    private static void DessinerGrandeIconeCarte(Graphics g, Rectangle rect, IconeMenu icone, Color couleur)
    {
        using var brush = new SolidBrush(couleur);
        using var pen = new Pen(couleur, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        switch (icone)
        {
            case IconeMenu.Etudiants:
                g.FillEllipse(brush, cx - 7, cy - 10, 9, 9);
                g.FillPie(brush, cx - 12, cy, 19, 15, 180, 180);
                g.FillEllipse(brush, cx + 2, cy - 7, 7, 7);
                g.FillPie(brush, cx - 1, cy + 2, 14, 12, 180, 180);
                break;

            case IconeMenu.Cours:
                Point[] pageG = [new(cx - 10, cy - 8), new(cx - 1, cy - 5), new(cx - 1, cy + 8), new(cx - 10, cy + 6)];
                Point[] pageD = [new(cx + 1, cy - 5), new(cx + 10, cy - 8), new(cx + 10, cy + 6), new(cx + 1, cy + 8)];
                g.FillPolygon(brush, pageG);
                g.FillPolygon(brush, pageD);
                break;

            case IconeMenu.Enseignants:
                g.FillEllipse(brush, cx - 5, cy - 10, 10, 10);
                g.FillPie(brush, cx - 11, cy + 1, 22, 16, 180, 180);
                break;

            case IconeMenu.Inscriptions:
                g.DrawRectangle(pen, cx - 8, cy - 8, 16, 18);
                g.FillRectangle(brush, cx - 4, cy - 11, 8, 4);
                g.DrawLine(pen, cx - 4, cy - 1, cx + 4, cy - 1);
                g.DrawLine(pen, cx - 4, cy + 3, cx + 4, cy + 3);
                g.DrawLine(pen, cx - 4, cy + 7, cx + 1, cy + 7);
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
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
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
            new Rectangle(20, 14, Width - 40, 26),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int total = Math.Max(1, _l1 + _l2 + _l3);
        TextRenderer.DrawText(
            g,
            $"Total : {total} inscrits",
            new Font("Segoe UI", 9f),
            new Rectangle(20, 38, Width - 40, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int chartLeft = 48;
        int chartRight = Width - 26;
        int chartTop = 78;
        int chartBottom = Height - 44;
        int chartH = Math.Max(60, chartBottom - chartTop);
        int chartW = Math.Max(120, chartRight - chartLeft);

        int rawMax = Math.Max(4, Math.Max(_l1, Math.Max(_l2, _l3)));
        int step = rawMax <= 12 ? 4 : rawMax <= 30 ? 5 : rawMax <= 100 ? 25 : 150;
        int maxVal = ((rawMax + step - 1) / step) * step;
        if (maxVal == rawMax) maxVal += step;

        using var gridPen = new Pen(Color.FromArgb(238, 242, 247), 1f) { DashStyle = DashStyle.Dash };
        using var axisPen = new Pen(Color.FromArgb(203, 213, 225), 1.2f);
        const int ticks = 4;
        for (int i = 0; i <= ticks; i++)
        {
            int valTick = (int)Math.Round(maxVal * (i / (double)ticks));
            int y = chartBottom - i * chartH / ticks;
            g.DrawLine(i == 0 ? axisPen : gridPen, chartLeft, y, chartRight, y);
            TextRenderer.DrawText(
                g,
                valTick.ToString(),
                new Font("Segoe UI", 8.5f),
                new Rectangle(6, y - 10, 36, 20),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        (string Niveau, int Valeur, Color Haut, Color Bas)[] barres =
        [
            ("Licence 1", _l1, Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235)),
            ("Licence 2", _l2, Color.FromArgb(214, 34, 40), Color.FromArgb(156, 16, 22)),
            ("Licence 3", _l3, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105))
        ];

        int colW = chartW / 3;
        int barW = Math.Min(68, Math.Max(36, colW - 44));

        for (int i = 0; i < barres.Length; i++)
        {
            int center = chartLeft + i * colW + colW / 2;

            // Piste de fond subtile
            var trackRect = new Rectangle(center - barW / 2, chartTop, barW, chartH);
            using (var trackPath = UiStyle.CreerCheminArrondi(trackRect, 8))
            using (var trackBrush = new SolidBrush(Color.FromArgb(244, 247, 250)))
            {
                g.FillPath(trackBrush, trackPath);
            }

            int barH = barres[i].Valeur == 0 ? 4 : Math.Max(14, (int)Math.Round(barres[i].Valeur * (double)chartH / maxVal));
            var barRect = new Rectangle(center - barW / 2, chartBottom - barH, barW, barH);

            using (var barPath = UiStyle.CreerCheminArrondi(barRect, 8))
            using (var barBrush = new LinearGradientBrush(barRect, barres[i].Haut, barres[i].Bas, 90f))
            {
                g.FillPath(barBrush, barPath);
            }

            int pct = (int)Math.Round(barres[i].Valeur * 100.0 / total);
            TextRenderer.DrawText(
                g,
                $"{barres[i].Valeur} ({pct}%)",
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                new Rectangle(center - 46, Math.Max(chartTop - 22, barRect.Y - 24), 92, 22),
                UiStyle.Texte,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                g,
                barres[i].Niveau,
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                new Rectangle(center - 44, chartBottom + 8, 88, 22),
                Color.FromArgb(51, 65, 85),
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
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Taux de présence",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(20, 14, Width - 40, 26),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            "Suivi global des séances enregistrées",
            new Font("Segoe UI", 9f),
            new Rectangle(20, 38, Width - 40, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int diameter = Math.Min(172, Math.Max(110, Math.Min(Width / 2 + 10, Height - 96)));
        var donutRect = new Rectangle(28, 68 + Math.Max(0, (Height - 84 - diameter) / 2), diameter, diameter);

        Color colPresence = Color.FromArgb(16, 185, 129);
        Color colAbsence = Color.FromArgb(239, 68, 68);

        float sweepAbsence = _tauxAbsence <= 0 ? 0f : Math.Max(8f, _tauxAbsence * 3.6f);
        float sweepPresence = 360f - sweepAbsence;

        using (var penTrack = new Pen(Color.FromArgb(241, 245, 249), 22f))
        {
            g.DrawEllipse(penTrack, donutRect);
        }

        var arcRect = Rectangle.Inflate(donutRect, -11, -11);
        using (var penPres = new Pen(colPresence, 22f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        using (var penAbs = new Pen(colAbsence, 22f))
        {
            if (sweepAbsence > 0f)
            {
                g.DrawArc(penAbs, arcRect, -90f + sweepPresence, sweepAbsence);
            }
            g.DrawArc(penPres, arcRect, -90f, sweepPresence);
        }

        // Valeur centrale dans le Donut (sans chevauchement)
        TextRenderer.DrawText(
            g,
            $"{_tauxPresence}%",
            new Font("Segoe UI", 18f, FontStyle.Bold),
            new Rectangle(donutRect.X, donutRect.Y + diameter / 2 - 26, diameter, 28),
            UiStyle.Texte,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            "Présence",
            new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(donutRect.X, donutRect.Y + diameter / 2 + 4, diameter, 18),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // Légende moderne à droite
        int legX = donutRect.Right + 24;
        int legW = Math.Max(110, Width - legX - 18);
        int legY = donutRect.Y + diameter / 2 - 36;

        DessinerLigneLegende(g, legX, legY, legW, colPresence, "Présents", $"{_tauxPresence} %");
        DessinerLigneLegende(g, legX, legY + 42, legW, colAbsence, "Absents", $"{_tauxAbsence} %");
    }

    private static void DessinerLigneLegende(Graphics g, int x, int y, int width, Color couleur, string libelle, string valeur)
    {
        var box = new Rectangle(x, y, width, 32);
        using (var path = UiStyle.CreerCheminArrondi(box, 8))
        using (var fill = new SolidBrush(Color.FromArgb(248, 250, 252)))
        {
            g.FillPath(fill, path);
        }
        using (var dot = new SolidBrush(couleur))
        {
            g.FillEllipse(dot, x + 10, y + 11, 10, 10);
        }
        TextRenderer.DrawText(
            g,
            libelle,
            new Font("Segoe UI", 9.5f, FontStyle.Bold),
            new Rectangle(x + 26, y, width - 80, 32),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(
            g,
            valeur,
            new Font("Segoe UI", 9.5f, FontStyle.Bold),
            new Rectangle(x + width - 56, y, 48, 32),
            couleur,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PanneauRepartitionFilieres : PanneauArrondi
{
    private readonly IReadOnlyList<(string Filiere, int Effectif)> _filieres;

    public PanneauRepartitionFilieres(IReadOnlyList<(string Filiere, int Effectif)>? filieres)
    {
        _filieres = filieres ?? [];
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Effectifs par filière",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(20, 14, Width - 40, 26),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            "Répartition des étudiants actifs",
            new Font("Segoe UI", 9f),
            new Rectangle(20, 38, Width - 40, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        if (_filieres.Count == 0)
        {
            TextRenderer.DrawText(
                g,
                "Aucune donnée disponible",
                new Font("Segoe UI", 9.5f),
                new Rectangle(20, 80, Width - 40, 40),
                UiStyle.Gris,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            return;
        }

        Color[] palette =
        [
            Color.FromArgb(186, 13, 20),
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11),
            Color.FromArgb(139, 92, 246),
            Color.FromArgb(20, 184, 166)
        ];

        int maxEff = Math.Max(1, _filieres.Max(f => f.Effectif));
        int top = 72;
        int rowH = Math.Min(42, Math.Max(28, (Height - top - 16) / _filieres.Count));

        for (int i = 0; i < _filieres.Count; i++)
        {
            var (nom, eff) = _filieres[i];
            int y = top + i * rowH;
            Color col = palette[i % palette.Length];

            TextRenderer.DrawText(
                g,
                nom,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                new Rectangle(20, y, Width - 88, 18),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(
                g,
                eff.ToString(),
                new Font("Segoe UI", 9f, FontStyle.Bold),
                new Rectangle(Width - 64, y, 44, 18),
                col,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

            var track = new Rectangle(20, y + 20, Width - 40, 7);
            using (var trackPath = UiStyle.CreerCheminArrondi(track, 3))
            using (var trackBrush = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                g.FillPath(trackBrush, trackPath);
            }

            int fillW = eff == 0 ? 0 : Math.Max(10, (int)Math.Round(track.Width * (eff / (double)maxEff)));
            if (fillW > 0)
            {
                var fillRect = new Rectangle(track.X, track.Y, fillW, track.Height);
                using var fillPath = UiStyle.CreerCheminArrondi(fillRect, 3);
                using var fillBrush = new SolidBrush(col);
                g.FillPath(fillBrush, fillPath);
            }
        }
    }
}

internal sealed class PanneauJaugeAssiduite : Panel
{
    private readonly int _tauxPresence;
    private readonly int _presents;
    private readonly int _absents;

    public PanneauJaugeAssiduite(int tauxPresence, int presents, int absents)
    {
        _tauxPresence = Math.Clamp(tauxPresence, 0, 100);
        _presents = presents;
        _absents = absents;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(2, 2, Width - 5, Height - 5);
        using var path = UiStyle.CreerCheminArrondi(rect, 16);
        using (var bgBrush = new LinearGradientBrush(rect, Color.FromArgb(24, 31, 46), Color.FromArgb(48, 18, 28), 35f))
        {
            g.FillPath(bgBrush, path);
        }

        TextRenderer.DrawText(
            g,
            "INDICE D'ASSIDUITÉ",
            new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(20, 14, Width - 40, 20),
            Color.FromArgb(148, 163, 184),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int gaugeSize = Math.Min(196, Math.Max(130, Math.Min(Width - 50, (Height - 66) * 2 - 24)));
        int gx = (Width - gaugeSize) / 2;
        int gy = 40;
        var arcRect = new Rectangle(gx, gy, gaugeSize, gaugeSize);

        using (var penTrack = new Pen(Color.FromArgb(55, 255, 255, 255), 16f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            g.DrawArc(penTrack, arcRect, 180f, 180f);
        }

        float sweep = Math.Max(4f, 180f * (_tauxPresence / 100f));
        Color arcColor = _tauxPresence >= 75 ? Color.FromArgb(52, 211, 153) : Color.FromArgb(251, 191, 36);
        using (var penVal = new Pen(arcColor, 16f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            g.DrawArc(penVal, arcRect, 180f, sweep);
        }

        TextRenderer.DrawText(
            g,
            $"{_tauxPresence}%",
            new Font("Segoe UI", 24f, FontStyle.Bold),
            new Rectangle(gx, gy + gaugeSize / 4 - 8, gaugeSize, 40),
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            "Taux de présence",
            new Font("Segoe UI", 8.5f),
            new Rectangle(gx, gy + gaugeSize / 4 + 32, gaugeSize, 20),
            Color.FromArgb(203, 213, 225),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        // Compteurs en bas
        int pillY = Height - 42;
        int halfW = (Width - 52) / 2;
        DessinerMiniBadge(g, 20, pillY, halfW, Color.FromArgb(52, 211, 153), $"{_presents} Présents");
        DessinerMiniBadge(g, 32 + halfW, pillY, halfW, Color.FromArgb(248, 113, 113), $"{_absents} Absents");
    }

    private static void DessinerMiniBadge(Graphics g, int x, int y, int w, Color dot, string texte)
    {
        var r = new Rectangle(x, y, w, 26);
        using (var p = UiStyle.CreerCheminArrondi(r, 8))
        using (var b = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
        {
            g.FillPath(b, p);
        }
        using (var db = new SolidBrush(dot))
        {
            g.FillEllipse(db, x + 10, y + 9, 8, 8);
        }
        TextRenderer.DrawText(
            g,
            texte,
            new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(x + 22, y, w - 26, 26),
            Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PanneauCourbeTendance : PanneauArrondi
{
    private readonly IReadOnlyList<(string DateLabel, int Presents, int Absents)> _points;

    public PanneauCourbeTendance(IReadOnlyList<(string DateLabel, int Presents, int Absents)>? points)
    {
        _points = points is { Count: > 0 }
            ? points
            : [("03/11", 8, 2), ("05/11", 10, 2), ("06/11", 12, 3), ("09/11", 11, 1), ("11/11", 14, 2), ("12/11", 34, 6)];
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Évolution des présences & absences",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(20, 14, Width - 220, 26),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        TextRenderer.DrawText(
            g,
            "Courbe chronologique des séances enregistrées",
            new Font("Segoe UI", 9f),
            new Rectangle(20, 38, Width - 220, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        // Légende en haut à droite
        using (var bPres = new SolidBrush(Color.FromArgb(16, 185, 129)))
        using (var bAbs = new SolidBrush(Color.FromArgb(239, 68, 68)))
        {
            g.FillEllipse(bPres, Width - 185, 22, 10, 10);
            TextRenderer.DrawText(g, "Présents", new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(Width - 170, 16, 70, 22), UiStyle.Texte, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            g.FillEllipse(bAbs, Width - 95, 22, 10, 10);
            TextRenderer.DrawText(g, "Absents", new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(Width - 80, 16, 68, 22), UiStyle.Texte, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        int left = 46;
        int right = Width - 24;
        int top = 74;
        int bottom = Height - 42;
        int h = Math.Max(60, bottom - top);
        int w = Math.Max(120, right - left);

        int maxVal = Math.Max(4, _points.Max(p => Math.Max(p.Presents, p.Absents)));
        int step = maxVal <= 12 ? 4 : maxVal <= 30 ? 5 : 10;
        maxVal = ((maxVal + step - 1) / step) * step;

        using var gridPen = new Pen(Color.FromArgb(238, 242, 247), 1f) { DashStyle = DashStyle.Dash };
        using var axisPen = new Pen(Color.FromArgb(203, 213, 225), 1.2f);
        for (int i = 0; i <= 4; i++)
        {
            int y = bottom - i * h / 4;
            int val = (int)Math.Round(maxVal * (i / 4.0));
            g.DrawLine(i == 0 ? axisPen : gridPen, left, y, right, y);
            TextRenderer.DrawText(g, val.ToString(), new Font("Segoe UI", 8.5f),
                new Rectangle(6, y - 10, 34, 20), Color.FromArgb(100, 116, 139), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        int n = _points.Count;
        PointF[] ptsPres = new PointF[n];
        PointF[] ptsAbs = new PointF[n];

        for (int i = 0; i < n; i++)
        {
            float x = n == 1 ? left + w / 2f : left + i * (w / (float)(n - 1));
            ptsPres[i] = new PointF(x, bottom - (float)(_points[i].Presents * h / (double)maxVal));
            ptsAbs[i] = new PointF(x, bottom - (float)(_points[i].Absents * h / (double)maxVal));

            TextRenderer.DrawText(
                g,
                _points[i].DateLabel,
                new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle((int)x - 26, bottom + 8, 52, 20),
                Color.FromArgb(71, 85, 105),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        if (n >= 2)
        {
            // Remplissage dégradé sous la courbe des présences
            using var areaPath = new GraphicsPath();
            areaPath.AddCurve(ptsPres, 0.35f);
            areaPath.AddLine(ptsPres[^1].X, bottom, ptsPres[0].X, bottom);
            areaPath.CloseFigure();
            using var areaBrush = new LinearGradientBrush(
                new Rectangle(left, top, w, h),
                Color.FromArgb(70, 16, 185, 129),
                Color.FromArgb(5, 16, 185, 129),
                90f);
            g.FillPath(areaBrush, areaPath);

            using var penPres = new Pen(Color.FromArgb(16, 185, 129), 3f);
            using var penAbs = new Pen(Color.FromArgb(239, 68, 68), 2.4f) { DashStyle = DashStyle.Solid };
            g.DrawCurve(penPres, ptsPres, 0.35f);
            g.DrawCurve(penAbs, ptsAbs, 0.35f);
        }

        // Points sur les courbes
        for (int i = 0; i < n; i++)
        {
            g.FillEllipse(Brushes.White, ptsPres[i].X - 4.5f, ptsPres[i].Y - 4.5f, 9f, 9f);
            using (var p = new Pen(Color.FromArgb(16, 185, 129), 2.5f))
                g.DrawEllipse(p, ptsPres[i].X - 4.5f, ptsPres[i].Y - 4.5f, 9f, 9f);

            g.FillEllipse(Brushes.White, ptsAbs[i].X - 3.5f, ptsAbs[i].Y - 3.5f, 7f, 7f);
            using (var p = new Pen(Color.FromArgb(239, 68, 68), 2f))
                g.DrawEllipse(p, ptsAbs[i].X - 3.5f, ptsAbs[i].Y - 3.5f, 7f, 7f);
        }
    }
}

internal sealed class PanneauNiveauxEtPlanning : PanneauArrondi
{
    private readonly int _l1;
    private readonly int _l2;
    private readonly int _l3;
    private readonly IReadOnlyList<(string Jour, int Seances)> _seances;

    public PanneauNiveauxEtPlanning(int l1, int l2, int l3, IReadOnlyList<(string Jour, int Seances)>? seances)
    {
        _l1 = l1;
        _l2 = l2;
        _l3 = l3;
        _seances = seances is { Count: > 0 }
            ? seances
            : [("Lun", 3), ("Mar", 3), ("Mer", 3), ("Jeu", 3), ("Ven", 3)];
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Barre segmentée proportionnelle L1 / L2 / L3
        TextRenderer.DrawText(
            g,
            "Répartition par niveau",
            new Font("Segoe UI", 11.5f, FontStyle.Bold),
            new Rectangle(20, 12, Width - 40, 24),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int total = Math.Max(1, _l1 + _l2 + _l3);
        int barW = Width - 40;
        int barY = 44;
        int w1 = Math.Max(18, (int)Math.Round(barW * (_l1 / (double)total)));
        int w2 = Math.Max(18, (int)Math.Round(barW * (_l2 / (double)total)));
        int w3 = Math.Max(18, barW - w1 - w2);

        Color c1 = Color.FromArgb(37, 99, 235);
        Color c2 = Color.FromArgb(186, 13, 20);
        Color c3 = Color.FromArgb(16, 185, 129);

        using (var b1 = new SolidBrush(c1))
        using (var b2 = new SolidBrush(c2))
        using (var b3 = new SolidBrush(c3))
        using (var clipPath = UiStyle.CreerCheminArrondi(new Rectangle(20, barY, barW, 18), 9))
        {
            g.SetClip(clipPath);
            g.FillRectangle(b1, 20, barY, w1, 18);
            g.FillRectangle(b2, 20 + w1, barY, w2, 18);
            g.FillRectangle(b3, 20 + w1 + w2, barY, w3, 18);
            g.ResetClip();
        }

        int legY = barY + 24;
        int itemW = barW / 3;
        DessinerLegendeNiveau(g, 20, legY, itemW, c1, $"L1 : {_l1}");
        DessinerLegendeNiveau(g, 20 + itemW, legY, itemW, c2, $"L2 : {_l2}");
        DessinerLegendeNiveau(g, 20 + itemW * 2, legY, itemW, c3, $"L3 : {_l3}");

        // 2. Diagramme Lollipop hebdomadaire (Séances par jour)
        int top2 = 104;
        TextRenderer.DrawText(
            g,
            "Charge hebdomadaire (séances / jour)",
            new Font("Segoe UI", 11.5f, FontStyle.Bold),
            new Rectangle(20, top2, Width - 40, 24),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        int chartTop = top2 + 46;
        int chartBottom = Height - 32;
        int chartH = Math.Max(40, chartBottom - chartTop);
        int maxS = Math.Max(2, _seances.Max(s => s.Seances));
        int colW = barW / Math.Max(1, _seances.Count);

        using var lineBase = new Pen(Color.FromArgb(226, 232, 240), 1.5f);
        g.DrawLine(lineBase, 20, chartBottom, Width - 20, chartBottom);

        for (int i = 0; i < _seances.Count; i++)
        {
            var (jour, nb) = _seances[i];
            int cx = 20 + i * colW + colW / 2;
            int stemH = nb == 0 ? 4 : Math.Max(14, (int)Math.Round(chartH * (nb / (double)maxS)));
            int cy = chartBottom - stemH;

            using var stemPen = new Pen(Color.FromArgb(148, 163, 184), 3f);
            g.DrawLine(stemPen, cx, chartBottom, cx, cy);

            using var headBrush = new SolidBrush(Color.FromArgb(186, 13, 20));
            g.FillEllipse(headBrush, cx - 11, cy - 11, 22, 22);

            TextRenderer.DrawText(g, nb.ToString(), new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(cx - 12, cy - 12, 24, 24), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(g, jour, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(cx - 22, chartBottom + 5, 44, 20), Color.FromArgb(71, 85, 105), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private static void DessinerLegendeNiveau(Graphics g, int x, int y, int w, Color c, string txt)
    {
        using var b = new SolidBrush(c);
        g.FillEllipse(b, x, y + 5, 8, 8);
        TextRenderer.DrawText(g, txt, new Font("Segoe UI", 8.5f, FontStyle.Bold),
            new Rectangle(x + 12, y, w - 14, 18), UiStyle.Texte, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}

internal sealed class PanneauDiagrammeRadial : PanneauArrondi
{
    private readonly IReadOnlyList<(string Filiere, int Effectif)> _filieres;
    private readonly int _totalEtudiants;
    private readonly int _l1;
    private readonly int _l2;
    private readonly int _l3;

    public PanneauDiagrammeRadial(
        IReadOnlyList<(string Filiere, int Effectif)>? filieres,
        int totalEtudiants,
        int l1,
        int l2,
        int l3)
    {
        _filieres = filieres is { Count: > 0 }
            ? filieres.Take(6).ToList()
            : [("Génie Logiciel", 12), ("Informatique", 10), ("Marketing", 9), ("Réseaux", 8), ("Data Science & IA", 7), ("Cybersécurité", 5)];
        _totalEtudiants = Math.Max(1, totalEtudiants);
        _l1 = l1;
        _l2 = l2;
        _l3 = l3;
        Rayon = 14;
        CouleurFond = Color.White;
        CouleurBordure = Color.FromArgb(226, 232, 240);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderer.DrawText(
            g,
            "Répartition radiale des filières",
            new Font("Segoe UI", 12.5f, FontStyle.Bold),
            new Rectangle(20, 14, Width - 40, 26),
            UiStyle.Texte,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        TextRenderer.DrawText(
            g,
            $"Anneaux proportionnels  •  L1 : {_l1}  •  L2 : {_l2}  •  L3 : {_l3}",
            new Font("Segoe UI", 9f),
            new Rectangle(20, 43, Width - 40, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        Color[] palette =
        [
            Color.FromArgb(186, 13, 20),
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11),
            Color.FromArgb(139, 92, 246),
            Color.FromArgb(20, 184, 166)
        ];

        int footerH = 74;
        int availH = Math.Max(180, Height - 74 - footerH);
        int maxOuter = Math.Min(260, Math.Max(155, Math.Min(Width / 2 - 14, availH - 12)));
        int cx = 24 + maxOuter / 2;
        int cy = 70 + availH / 2;
        int maxEff = Math.Max(1, _filieres.Max(f => f.Effectif));

        int ringThickness = Math.Max(7, Math.Min(11, (maxOuter / 2 - 36) / Math.Max(1, _filieres.Count * 2)));
        int ringStep = ringThickness + 5;

        for (int i = 0; i < _filieres.Count; i++)
        {
            int radius = maxOuter / 2 - i * ringStep;
            if (radius < 30) break;

            var rect = new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);
            using (var trackPen = new Pen(Color.FromArgb(241, 245, 249), ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawEllipse(trackPen, rect);
            }

            float ratio = Math.Clamp(_filieres[i].Effectif / (float)maxEff, 0.14f, 1f);
            float sweep = 275f * ratio;
            using var arcPen = new Pen(palette[i % palette.Length], ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(arcPen, rect, -90f, sweep);
        }

        // Total au centre des anneaux
        TextRenderer.DrawText(
            g,
            _totalEtudiants.ToString(),
            new Font("Segoe UI", 14.5f, FontStyle.Bold),
            new Rectangle(cx - 36, cy - 26, 72, 28),
            UiStyle.Texte,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        TextRenderer.DrawText(
            g,
            "inscrits",
            new Font("Segoe UI", 8f, FontStyle.Bold),
            new Rectangle(cx - 36, cy + 4, 72, 16),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        // Légende détaillée à droite
        int legX = cx + maxOuter / 2 + 20;
        int legW = Math.Max(130, Width - legX - 18);
        int rowH = Math.Min(42, Math.Max(30, availH / Math.Max(1, _filieres.Count)));
        int legTop = cy - (_filieres.Count * rowH) / 2;

        for (int i = 0; i < _filieres.Count; i++)
        {
            var (nom, eff) = _filieres[i];
            int y = legTop + i * rowH;
            Color col = palette[i % palette.Length];
            int pct = (int)Math.Round(eff * 100.0 / _totalEtudiants);

            var card = new Rectangle(legX, y, legW, rowH - 6);
            using (var p = UiStyle.CreerCheminArrondi(card, 8))
            using (var b = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.FillPath(b, p);
            }

            using (var dot = new SolidBrush(col))
            {
                g.FillEllipse(dot, legX + 10, y + (card.Height - 10) / 2, 10, 10);
            }

            const int valColW = 78;
            TextRenderer.DrawText(
                g,
                nom,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                new Rectangle(legX + 26, y, Math.Max(40, legW - 26 - valColW - 8), card.Height),
                UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(
                g,
                $"{eff} ({pct}%)",
                new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(legX + legW - valColW - 8, y, valColW, card.Height),
                col,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // Bandeau inférieur des 3 niveaux (L1 / L2 / L3) pour équilibrer la carte
        int footY = Height - footerH + 6;
        int boxGap = 10;
        int boxW = (Width - 40 - boxGap * 2) / 3;
        (string Titre, int Val, Color Col)[] niveaux =
        [
            ("LICENCE 1", _l1, Color.FromArgb(37, 99, 235)),
            ("LICENCE 2", _l2, Color.FromArgb(186, 13, 20)),
            ("LICENCE 3", _l3, Color.FromArgb(16, 185, 129))
        ];
        int totalNiv = Math.Max(1, _l1 + _l2 + _l3);
        for (int i = 0; i < 3; i++)
        {
            int bx = 20 + i * (boxW + boxGap);
            var bRect = new Rectangle(bx, footY, boxW, 52);
            using (var bp = UiStyle.CreerCheminArrondi(bRect, 10))
            using (var bb = new SolidBrush(Color.FromArgb(248, 250, 252)))
            using (var bPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
            {
                g.FillPath(bb, bp);
                g.DrawPath(bPen, bp);
            }

            TextRenderer.DrawText(g, niveaux[i].Titre, new Font("Segoe UI", 7.5f, FontStyle.Bold),
                new Rectangle(bx + 12, footY + 6, boxW - 56, 16), Color.FromArgb(100, 116, 139),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(g, niveaux[i].Val.ToString(), new Font("Segoe UI", 11.5f, FontStyle.Bold),
                new Rectangle(bx + boxW - 48, footY + 4, 38, 22), niveaux[i].Col,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

            var track = new Rectangle(bx + 12, footY + 34, boxW - 24, 6);
            using (var tp = UiStyle.CreerCheminArrondi(track, 3))
            using (var tb = new SolidBrush(Color.FromArgb(226, 232, 240)))
                g.FillPath(tb, tp);

            int fw = Math.Max(8, (int)Math.Round(track.Width * (niveaux[i].Val / (double)totalNiv)));
            using (var fp = UiStyle.CreerCheminArrondi(new Rectangle(track.X, track.Y, fw, track.Height), 3))
            using (var fb = new SolidBrush(niveaux[i].Col))
                g.FillPath(fb, fp);
        }
    }
}
