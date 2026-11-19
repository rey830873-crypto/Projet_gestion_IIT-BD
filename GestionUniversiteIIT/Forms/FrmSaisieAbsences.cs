using System.Data;
using System.Drawing.Drawing2D;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmSaisieAbsences : Form
{
    private readonly AbsenceService _service = new();
    private readonly Action? _ouvrirHistorique;
    private DateTime _dateValeur = DateTime.Today;
    private DataTable? _etudiants;

    public FrmSaisieAbsences() : this(null) { }

    public FrmSaisieAbsences(Action? ouvrirHistorique)
    {
        _ouvrirHistorique = ouvrirHistorique;
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            ChargerFiltres();
            ChargerEtudiants();
        }
    }

    private void ConstruireInterface()
    {
        BackColor = Color.FromArgb(246, 248, 252);
        _dateTexte.Text = _dateValeur.ToString("dd/MM/yyyy");

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 18),
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.FromArgb(246, 248, 252)
        };
        UiStyle.AppliquerFondDesign(root);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var entete = UiStyle.EntetePage(IconeMenu.Absences, "Saisie des Absences");
        if (_ouvrirHistorique is not null)
        {
            entete.DoubleClick += (_, _) => _ouvrirHistorique();
        }

        root.Controls.Add(entete, 0, 0);

        var filtersGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 2, 0, 6)
        };
        filtersGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76f));
        filtersGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        filtersGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
        filtersGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        filtersGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
        filtersGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

        filtersGrid.Controls.Add(CreerLabelFiltre("Cours :"), 0, 0);
        var boxCours = UiStyle.EncadrerChamp(_cours, 310, 38);
        boxCours.Dock = DockStyle.Fill;
        boxCours.Margin = new Padding(0, 4, 24, 4);
        filtersGrid.Controls.Add(boxCours, 1, 0);

        filtersGrid.Controls.Add(CreerLabelFiltre("Date :"), 2, 0);
        var boxDate = CreerChampDateCours();
        boxDate.Dock = DockStyle.Fill;
        boxDate.Margin = new Padding(0, 4, 4, 4);
        filtersGrid.Controls.Add(boxDate, 3, 0);

        filtersGrid.Controls.Add(CreerLabelFiltre("Filière :"), 0, 1);
        var boxFiliere = UiStyle.EncadrerChamp(_filiere, 310, 38);
        boxFiliere.Dock = DockStyle.Fill;
        boxFiliere.Margin = new Padding(0, 4, 24, 4);
        filtersGrid.Controls.Add(boxFiliere, 1, 1);

        filtersGrid.Controls.Add(CreerLabelFiltre("Niveau :"), 2, 1);
        var boxNiveau = UiStyle.EncadrerChamp(_niveau, 240, 38);
        boxNiveau.Dock = DockStyle.Fill;
        boxNiveau.Margin = new Padding(0, 4, 4, 4);
        filtersGrid.Controls.Add(boxNiveau, 3, 1);

        root.Controls.Add(filtersGrid, 0, 1);

        ConfigurerGrillePresence();
        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_grille);
        root.Controls.Add(card, 0, 2);

        var footerBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        var leftBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };
        _lblCompteur.AutoSize = true;
        _lblCompteur.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _lblCompteur.ForeColor = UiStyle.Texte;
        _lblCompteur.Margin = new Padding(2, 8, 16, 0);
        leftBar.Controls.Add(_lblCompteur);

        var btnTousPresents = UiStyle.BoutonArdoise("Tous présents", 136, 36);
        btnTousPresents.Click += (_, _) => DefinirPresencePourTous(true);
        var btnTousAbsents = UiStyle.BoutonArdoise("Tous absents", 130, 36);
        btnTousAbsents.Click += (_, _) => DefinirPresencePourTous(false);
        leftBar.Controls.Add(btnTousPresents);
        leftBar.Controls.Add(btnTousAbsents);
        footerBar.Controls.Add(leftBar);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.Transparent
        };

        var btnAnnuler = UiStyle.BoutonArdoise("Annuler", 132, 42);
        var btnEnregistrer = UiStyle.BoutonMaquette(
            "Enregistrer",
            UiStyle.Rouge,
            Color.FromArgb(148, 9, 15),
            Color.White,
            148,
            42);

        btnAnnuler.Click += (_, _) => ChargerEtudiants();
        btnEnregistrer.Click += (_, _) => Enregistrer();

        actions.Controls.Add(btnAnnuler);
        actions.Controls.Add(btnEnregistrer);
        footerBar.Controls.Add(actions);
        root.Controls.Add(footerBar, 0, 3);

        Controls.Add(root);
    }


    private static Label CreerLabelFiltre(string texte) => new()
    {
        Text = texte,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        ForeColor = UiStyle.Texte
    };

    private Panel CreerChampDateCours()
    {
        var box = new PanneauArrondi
        {
            Size = new Size(240, 38),
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = UiStyle.BordureGrise,
            Padding = new Padding(12, 8, 36, 6)
        };

        _dateTexte.BorderStyle = BorderStyle.None;
        _dateTexte.Dock = DockStyle.Fill;
        _dateTexte.Font = new Font("Segoe UI", 10f);
        _dateTexte.BackColor = Color.White;
        _dateTexte.ForeColor = UiStyle.Texte;
        _dateTexte.Leave += (_, _) => AnalyserDateEtRecharger();
        _dateTexte.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                AnalyserDateEtRecharger();
            }
        };

        box.Controls.Add(_dateTexte);
        box.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = box.Width - 20;
            int cy = box.Height / 2;
            UiStyle.DessinerIconeMenu(e.Graphics, new Rectangle(cx - 10, cy - 10, 20, 20), IconeMenu.Planification, UiStyle.Texte);
        };

        box.MouseClick += (_, e) =>
        {
            if (e.X >= box.Width - 36)
            {
                _dateValeur = _dateValeur.AddDays(1);
                _dateTexte.Text = _dateValeur.ToString("dd/MM/yyyy");
                ChargerEtudiants();
            }
        };

        return box;
    }

    private void AnalyserDateEtRecharger()
    {
        if (DateTime.TryParse(_dateTexte.Text.Trim(), out var parsed))
        {
            _dateValeur = parsed.Date;
            _dateTexte.Text = _dateValeur.ToString("dd/MM/yyyy");
            ChargerEtudiants();
        }
    }

    private void ConfigurerGrillePresence()
    {
        UiStyle.ConfigurerGrille(_grille);
        _grille.Dock = DockStyle.Fill;
        _grille.AutoGenerateColumns = false;

        _grille.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "EtudiantId",
            Name = "EtudiantId",
            Visible = false
        });
        _grille.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Matricule",
            Name = "Matricule",
            HeaderText = "Matricule",
            FillWeight = 28
        });
        _grille.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Nom et Prénoms",
            Name = "NomEtPrenoms",
            HeaderText = "Nom et Prénoms",
            FillWeight = 44
        });

        var colPresence = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Present",
            Name = "Presence",
            HeaderText = "Présence",
            FillWeight = 28
        };
        colPresence.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPresence.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grille.Columns.Add(colPresence);

        // Rendu personnalisé Case à cocher bleue + Badge vert "Présent" / rouge "Absent" (10_saisie_absences.png)
        _grille.CellPainting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grille.Columns[e.ColumnIndex].Name != "Presence" || e.Graphics is null)
            {
                return;
            }

            e.PaintBackground(e.CellBounds, true);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            bool present = e.Value is not null && e.Value is not DBNull && Convert.ToBoolean(e.Value);

            int checkSize = 22;
            int pillW = 92;
            int pillH = 26;
            int gap = 8;
            int totalW = checkSize + gap + pillW;
            int startX = e.CellBounds.X + (e.CellBounds.Width - totalW) / 2;
            int centerY = e.CellBounds.Y + e.CellBounds.Height / 2;

            var checkRect = new Rectangle(startX, centerY - checkSize / 2, checkSize, checkSize);
            using (var checkPath = UiStyle.CreerCheminArrondi(checkRect, 5))
            {
                if (present)
                {
                    using var checkFill = new SolidBrush(Color.FromArgb(24, 108, 212));
                    e.Graphics.FillPath(checkFill, checkPath);
                    using var penCheck = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    e.Graphics.DrawLine(penCheck, checkRect.X + 5, centerY, checkRect.X + 9, centerY + 4);
                    e.Graphics.DrawLine(penCheck, checkRect.X + 9, centerY + 4, checkRect.X + 17, centerY - 4);
                }
                else
                {
                    using var whiteFill = new SolidBrush(Color.White);
                    using var borderPen = new Pen(Color.FromArgb(140, 150, 166), 1.5f);
                    e.Graphics.FillPath(whiteFill, checkPath);
                    e.Graphics.DrawPath(borderPen, checkPath);
                }
            }

            var pillRect = new Rectangle(startX + checkSize + gap, centerY - pillH / 2, pillW, pillH);
            using (var pillPath = UiStyle.CreerCheminArrondi(pillRect, 12))
            using (var pillBrush = new SolidBrush(present ? UiStyle.BadgeVertFond : UiStyle.BadgeRougeFond))
            {
                e.Graphics.FillPath(pillBrush, pillPath);
            }

            TextRenderer.DrawText(
                e.Graphics,
                present ? "Présent" : "Absent",
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                pillRect,
                present ? UiStyle.BadgeVertTexte : UiStyle.BadgeRougeTexte,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Handled = true;
        };

        _grille.CellMouseMove += (_, e) =>
        {
            _grille.Cursor = e.RowIndex >= 0 && e.ColumnIndex >= 0 && _grille.Columns[e.ColumnIndex].Name == "Presence"
                ? Cursors.Hand
                : Cursors.Default;
        };

        _grille.CellMouseClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grille.Columns[e.ColumnIndex].Name != "Presence" || _etudiants is null)
            {
                return;
            }

            bool actuel = Convert.ToBoolean(_etudiants.Rows[e.RowIndex]["Present"]);
            _etudiants.Rows[e.RowIndex]["Present"] = !actuel;
            _grille.InvalidateCell(e.ColumnIndex, e.RowIndex);
            MettreAJourCompteur();
        };
    }

    private void DefinirPresencePourTous(bool present)
    {
        if (_etudiants is null) return;
        foreach (DataRow row in _etudiants.Rows)
        {
            row["Present"] = present;
        }
        _grille.Invalidate();
        MettreAJourCompteur();
    }

    private void MettreAJourCompteur()
    {
        if (_etudiants is null)
        {
            _lblCompteur.Text = "0 étudiant";
            return;
        }

        int total = _etudiants.Rows.Count;
        int presents = _etudiants.AsEnumerable().Count(r => r["Present"] is not DBNull && Convert.ToBoolean(r["Present"]));
        int absents = total - presents;
        _lblCompteur.Text = $"{total} étudiants  •  {presents} présents  •  {absents} absents";
    }

    private void ChargerFiltres()
    {
        try
        {
            var tableCours = DbHelper.Query("SELECT CoursId, Libelle + N' (' + Code + N')' AS Libelle FROM dbo.Cours ORDER BY Code");
            if (tableCours.Rows.Count > 0)
            {
                _cours.DataSource = tableCours;
                _cours.DisplayMember = "Libelle";
                _cours.ValueMember = "CoursId";
            }
        }
        catch
        {
            // Mode maquette hors-ligne
        }

        if (_cours.DataSource is null)
        {
            var secoursCours = new DataTable();
            secoursCours.Columns.Add("CoursId", typeof(int));
            secoursCours.Columns.Add("Libelle", typeof(string));
            secoursCours.Rows.Add(1, "Programmation C# (GL101)");
            secoursCours.Rows.Add(2, "Base de données (SQL) (GL102)");
            secoursCours.Rows.Add(3, "Marketing de base (MT101)");
            secoursCours.Rows.Add(4, "Algorithmique (INF101)");
            _cours.DataSource = secoursCours;
            _cours.DisplayMember = "Libelle";
            _cours.ValueMember = "CoursId";
        }

        _filiere.Items.Clear();
        _filiere.Items.Add("Toutes");
        try
        {
            var dtFil = DbHelper.Query("SELECT Nom FROM dbo.Filieres ORDER BY Nom");
            foreach (DataRow r in dtFil.Rows)
            {
                string nom = Convert.ToString(r["Nom"]) ?? "";
                if (!string.IsNullOrWhiteSpace(nom))
                {
                    _filiere.Items.Add(nom);
                }
            }
        }
        catch
        {
            _filiere.Items.AddRange(["Génie Logiciel", "Informatique", "Marketing", "Réseaux", "Data Science & IA", "Cybersécurité"]);
        }
        _filiere.SelectedIndex = _filiere.Items.Count > 1 ? 1 : 0;

        _niveau.Items.Clear();
        _niveau.Items.AddRange(["Tous", "L1", "L2", "L3"]);
        _niveau.SelectedIndex = 0;

        _cours.SelectedIndexChanged += (_, _) => ChargerEtudiants();
        _filiere.SelectedIndexChanged += (_, _) => ChargerEtudiants();
        _niveau.SelectedIndexChanged += (_, _) => ChargerEtudiants();
    }

    private void ChargerEtudiants()
    {
        int coursId = _cours.SelectedValue is int id ? id : 1;
        string? filiere = Convert.ToString(_filiere.SelectedItem);
        string? niveau = Convert.ToString(_niveau.SelectedItem);
        _etudiants = _service.EtudiantsDuCours(coursId, _dateValeur, filiere, niveau);
        _grille.DataSource = _etudiants;
        MettreAJourCompteur();
    }

    private void Enregistrer()
    {
        if (_etudiants is null)
        {
            return;
        }

        int coursId = _cours.SelectedValue is int id ? id : 1;
        string libelleCours = _cours.Text;
        _service.EnregistrerPresence(coursId, _dateValeur, _etudiants, libelleCours);
        MessageBox.Show("Les présences et absences ont été enregistrées avec succès.", "Saisie des Absences",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
