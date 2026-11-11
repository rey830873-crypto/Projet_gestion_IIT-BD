using System.Data;
using System.Drawing.Drawing2D;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmAjouterInscription : Form
{
    private readonly Action? _retour;
    private readonly TextBox _matricule = new() { Text = "IIT0015" };
    private readonly TextBox _nom = new();
    private readonly TextBox _prenoms = new();
    private readonly TextBox _dateNaissance = new() { PlaceholderText = "jj/mm/aaaa" };
    private readonly DateTimePicker _datePickerCache = new() { Format = DateTimePickerFormat.Short, Visible = false };
    private readonly ComboBox _sexe = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _filiere = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _niveau = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _annee = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly PanneauPhotoEtudiant _photoBox = new();
    private string? _cheminPhoto;

    public FrmAjouterInscription(Action? retour = null)
    {
        _retour = retour;
        ConstruireInterface();
        ChargerListes();
    }

    private void ConstruireInterface()
    {
        Text = "Nouvelle Inscription";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 22),
            RowCount = 2,
            ColumnCount = 1,
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 1. En-tête "Nouvelle Inscription" avec icône rouge
        root.Controls.Add(UiStyle.EntetePage(IconeMenu.Etudiants, "Nouvelle Inscription"), 0, 0);

        // 2. Carte principale arrondie "Informations de l'étudiant"
        var card = new PanneauArrondi
        {
            Dock = DockStyle.Fill,
            Rayon = 12,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(218, 223, 232),
            Padding = new Padding(24, 18, 24, 18)
        };

        var cardGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White
        };
        cardGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        cardGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cardGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        cardGrid.Controls.Add(new Label
        {
            Text = "Informations de l'étudiant",
            AutoSize = true,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(2, 2, 2, 8)
        }, 0, 0);

        // Corps à 3 colonnes + séparateur vertical comme sur 04_formulaire_inscription.png
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.White
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18f));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

        // Colonne gauche : Matricule, Nom *, Prénoms *, Date de naissance, Sexe
        var leftForm = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(2, 6, 8, 6)
        };
        leftForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138f));
        leftForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (int i = 0; i < 5; i++)
        {
            leftForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
        }

        AjouterLigneGauche(leftForm, 0, "Matricule", oblig: false, EncadrerTextBox(_matricule));
        AjouterLigneGauche(leftForm, 1, "Nom", oblig: true, EncadrerTextBox(_nom));
        AjouterLigneGauche(leftForm, 2, "Prénoms", oblig: true, EncadrerTextBox(_prenoms));
        AjouterLigneGauche(leftForm, 3, "Date de naissance", oblig: false, CreerChampDateNaissance());
        AjouterLigneGauche(leftForm, 4, "Sexe", oblig: false, EncadrerCombo(_sexe));

        body.Controls.Add(leftForm, 0, 0);

        // Séparateur vertical gris clair
        var separator = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        separator.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(224, 229, 238), 1.2f);
            e.Graphics.DrawLine(pen, separator.Width / 2, 8, separator.Width / 2, separator.Height - 16);
        };
        body.Controls.Add(separator, 1, 0);

        // Colonne centrale : Filière *, Niveau *, Année académique *
        var middleForm = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(8, 2, 8, 6)
        };
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        middleForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

        middleForm.Controls.Add(CreerLibelleChamp("Filière", true), 0, 0);
        middleForm.Controls.Add(EncadrerCombo(_filiere), 0, 1);
        middleForm.Controls.Add(CreerLibelleChamp("Niveau", true), 0, 2);
        middleForm.Controls.Add(EncadrerCombo(_niveau), 0, 3);
        middleForm.Controls.Add(CreerLibelleChamp("Année académique", true), 0, 4);
        middleForm.Controls.Add(EncadrerCombo(_annee), 0, 5);

        body.Controls.Add(middleForm, 2, 0);

        // Colonne droite : Cadre Photo + Bouton "Choisir une photo"
        var rightPhoto = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 26, 4, 4)
        };
        _photoBox.Size = new Size(146, 150);
        _photoBox.Margin = new Padding(0, 0, 0, 10);

        var btnPhoto = UiStyle.BoutonArdoise("Choisir une photo", 146, 36);
        btnPhoto.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        btnPhoto.Margin = new Padding(0);
        btnPhoto.Click += (_, _) => ChoisirPhoto();

        rightPhoto.Controls.Add(_photoBox);
        rightPhoto.Controls.Add(btnPhoto);
        body.Controls.Add(rightPhoto, 3, 0);

        cardGrid.Controls.Add(body, 0, 1);

        // Bas à droite : boutons "Enregistrer" (rouge) et "Annuler" (ardoise)
        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 6, 4, 0)
        };
        var btnAnnuler = UiStyle.BoutonArdoise("Annuler", 132, 42);
        var btnEnregistrer = UiStyle.BoutonMaquette(
            "Enregistrer",
            UiStyle.Rouge,
            Color.FromArgb(148, 9, 15),
            Color.White,
            148,
            42);
        var btnImprimerRecu = UiStyle.BoutonBleuExport("Imprimer reçu", 162, 42);

        btnAnnuler.Click += (_, _) => FermerOuRetourner(DialogResult.Cancel);
        btnEnregistrer.Click += (_, _) => EnregistrerInscription();
        btnImprimerRecu.Click += (_, _) => ImprimerRecuCourant();

        actionsPanel.Controls.Add(btnAnnuler);
        actionsPanel.Controls.Add(btnEnregistrer);
        actionsPanel.Controls.Add(btnImprimerRecu);
        cardGrid.Controls.Add(actionsPanel, 0, 2);

        card.Controls.Add(cardGrid);
        root.Controls.Add(card, 0, 1);
        Controls.Add(root);
    }

    private static void AjouterLigneGauche(TableLayoutPanel table, int row, string texte, bool oblig, Control champ)
    {
        var lbl = CreerLibelleChamp(texte, oblig);
        lbl.Anchor = AnchorStyles.Left;
        champ.Dock = DockStyle.Fill;
        table.Controls.Add(lbl, 0, row);
        table.Controls.Add(champ, 1, row);
    }

    private static Control CreerLibelleChamp(string texte, bool obligatoire)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(2, 4, 2, 2)
        };
        var lbl = new Label
        {
            Text = texte,
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(0, 2, 0, 0)
        };
        panel.Controls.Add(lbl);
        if (obligatoire)
        {
            var star = new Label
            {
                Text = "*",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UiStyle.Rouge,
                Margin = new Padding(2, 2, 0, 0)
            };
            panel.Controls.Add(star);
        }

        return panel;
    }

    private static Panel EncadrerTextBox(TextBox tb)
    {
        var box = new PanneauArrondi
        {
            Height = 38,
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = UiStyle.BordureGrise,
            Padding = new Padding(12, 8, 12, 6),
            Margin = new Padding(0, 4, 0, 4)
        };
        tb.BorderStyle = BorderStyle.None;
        tb.Dock = DockStyle.Fill;
        tb.Font = new Font("Segoe UI", 10f);
        tb.BackColor = Color.White;
        tb.ForeColor = UiStyle.Texte;
        box.Controls.Add(tb);
        return box;
    }

    private Panel CreerChampDateNaissance()
    {
        var box = new PanneauArrondi
        {
            Height = 38,
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = UiStyle.BordureGrise,
            Padding = new Padding(12, 8, 36, 6),
            Margin = new Padding(0, 4, 0, 4)
        };
        _dateNaissance.BorderStyle = BorderStyle.None;
        _dateNaissance.Dock = DockStyle.Fill;
        _dateNaissance.Font = new Font("Segoe UI", 10f);
        _dateNaissance.BackColor = Color.White;
        _dateNaissance.ForeColor = UiStyle.Texte;
        box.Controls.Add(_dateNaissance);
        box.Controls.Add(_datePickerCache);

        _datePickerCache.ValueChanged += (_, _) =>
        {
            _dateNaissance.Text = _datePickerCache.Value.ToString("dd/MM/yyyy");
        };

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
                _dateNaissance.Text = DateTime.Today.AddYears(-20).ToString("dd/MM/yyyy");
            }
        };

        return box;
    }

    private static Panel EncadrerCombo(ComboBox combo)
    {
        var box = new PanneauArrondi
        {
            Height = 38,
            Rayon = 8,
            CouleurFond = Color.White,
            CouleurBordure = UiStyle.BordureGrise,
            Padding = new Padding(8, 5, 8, 5),
            Margin = new Padding(0, 4, 0, 4)
        };
        combo.FlatStyle = FlatStyle.Flat;
        combo.Dock = DockStyle.Fill;
        combo.Font = new Font("Segoe UI", 10f);
        combo.BackColor = Color.White;
        combo.ForeColor = UiStyle.Texte;
        box.Controls.Add(combo);
        return box;
    }

    private void ChargerListes()
    {
        _sexe.Items.Clear();
        _sexe.Items.AddRange(["-- Sélectionner --", "M", "F"]);
        _sexe.SelectedIndex = 0;

        _niveau.Items.Clear();
        _niveau.Items.AddRange(["L1", "L2", "L3", "M1", "M2"]);
        _niveau.SelectedItem = "L2";

        _annee.Items.Clear();
        _annee.Items.AddRange(["2026 - 2027", "2025 - 2026", "2024 - 2025"]);
        _annee.SelectedIndex = 0;

        var service = new CatalogueService();
        var filieres = service.ChargerListe("SELECT FiliereId AS Id, Nom AS Libelle FROM dbo.Filieres ORDER BY FiliereId");
        _filiere.DataSource = filieres;
        _filiere.DisplayMember = "Libelle";
        _filiere.ValueMember = "Id";
    }

    private void ChoisirPhoto()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choisir une photo de l'étudiant",
            Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _cheminPhoto = dialog.FileName;
            _photoBox.ChargerImage(dialog.FileName);
        }
    }

    private void EnregistrerInscription()
    {
        string matricule = _matricule.Text.Trim();
        string nom = _nom.Text.Trim();
        string prenoms = _prenoms.Text.Trim();
        string niveau = Convert.ToString(_niveau.SelectedItem) ?? "L2";
        string annee = Convert.ToString(_annee.SelectedItem) ?? "2026 - 2027";
        string? sexe = _sexe.SelectedIndex > 0 ? Convert.ToString(_sexe.SelectedItem) : null;
        int filiereId = _filiere.SelectedValue is not null and not DBNull ? Convert.ToInt32(_filiere.SelectedValue) : 1;
        string nomFiliere = _filiere.Text;

        if (string.IsNullOrWhiteSpace(matricule) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenoms))
        {
            MessageBox.Show("Veuillez renseigner le matricule, le nom et les prénoms de l'étudiant.", "Champs obligatoires",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DateTime? dateNaissance = null;
        if (!string.IsNullOrWhiteSpace(_dateNaissance.Text) && DateTime.TryParse(_dateNaissance.Text, out var parsedDate))
        {
            dateNaissance = parsedDate.Date;
        }

        try
        {
            var existingId = DbHelper.Scalar(
                "SELECT EtudiantId FROM dbo.Etudiants WHERE Matricule = @Matricule",
                new Dictionary<string, object?> { ["@Matricule"] = matricule });

            int etudiantId;
            if (existingId is null)
            {
                etudiantId = Convert.ToInt32(DbHelper.Scalar(
                    """
                    INSERT dbo.Etudiants(Matricule, Nom, Prenom, FiliereId, Niveau, DateNaissance, Sexe, PhotoPath, Actif)
                    OUTPUT INSERTED.EtudiantId
                    VALUES(@Matricule, @Nom, @Prenom, @FiliereId, @Niveau, @DateNaissance, @Sexe, @PhotoPath, 1)
                    """,
                    new Dictionary<string, object?>
                    {
                        ["@Matricule"] = matricule,
                        ["@Nom"] = nom,
                        ["@Prenom"] = prenoms,
                        ["@FiliereId"] = filiereId,
                        ["@Niveau"] = niveau,
                        ["@DateNaissance"] = dateNaissance,
                        ["@Sexe"] = sexe,
                        ["@PhotoPath"] = _cheminPhoto
                    }));
            }
            else
            {
                etudiantId = Convert.ToInt32(existingId);
            }

            DbHelper.Execute(
                """
                IF NOT EXISTS (SELECT 1 FROM dbo.Inscriptions WHERE EtudiantId = @EtudiantId AND AnneeAcademique = @Annee)
                    INSERT dbo.Inscriptions(EtudiantId, AnneeAcademique, DateInscription) VALUES(@EtudiantId, @Annee, CONVERT(date, GETDATE()));
                """,
                new Dictionary<string, object?>
                {
                    ["@EtudiantId"] = etudiantId,
                    ["@Annee"] = annee
                });
        }
        catch
        {
            // Mise à jour du jeu de données en mémoire si SQL Server n'est pas actif
            var tableEtu = CatalogueService.ObtenirDonneesSecours("Etudiants");
            int nextEtuId = tableEtu.Rows.Count + 1;
            tableEtu.Rows.Add(nextEtuId, matricule, $"{nom} {prenoms}".Trim(), nomFiliere, niveau, "Actif");

            var tableIns = CatalogueService.ObtenirDonneesSecours("Inscriptions");
            int nextInsId = tableIns.Rows.Count + 1;
            tableIns.Rows.Add(nextInsId, (long)nextInsId, $"{nom} {prenoms}".Trim(), nomFiliere, niveau, DateTime.Today.ToString("dd/MM/yyyy"), annee);
        }

        if (MessageBox.Show(
                "L'inscription de l'étudiant a été enregistrée avec succès.\n\nSouhaitez-vous imprimer ou prévisualiser le reçu d'inscription ?",
                "Nouvelle Inscription",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information) == DialogResult.Yes)
        {
            ImprimerRecuCourant();
        }

        FermerOuRetourner(DialogResult.OK);
    }

    private void ImprimerRecuCourant()
    {
        UiStyle.ImprimerRecuInscription(
            this,
            _matricule.Text.Trim(),
            $"{_nom.Text.Trim()} {_prenoms.Text.Trim()}".Trim(),
            _filiere.Text,
            Convert.ToString(_niveau.SelectedItem) ?? "L2",
            Convert.ToString(_annee.SelectedItem) ?? "2026 - 2027",
            _dateNaissance.Text.Trim(),
            Convert.ToString(_sexe.SelectedItem) ?? "—");
    }

    private void FermerOuRetourner(DialogResult result)
    {
        if (_retour is not null)
        {
            _retour();
            return;
        }

        DialogResult = result;
        Close();
    }

    private sealed class PanneauPhotoEtudiant : Panel
    {
        private Image? _image;

        public PanneauPhotoEtudiant()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        public void ChargerImage(string path)
        {
            _image?.Dispose();
            _image = Image.FromFile(path);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = UiStyle.CreerCheminArrondi(rect, 10);
            using (var bg = new LinearGradientBrush(rect, Color.FromArgb(242, 245, 251), Color.FromArgb(228, 234, 245), 90f))
            {
                g.FillPath(bg, path);
            }

            if (_image is not null)
            {
                g.SetClip(path);
                g.DrawImage(_image, rect);
                g.ResetClip();
            }
            else
            {
                // Silhouette bleutée comme sur 04_formulaire_inscription.png
                using var silhouette = new SolidBrush(Color.FromArgb(144, 165, 202));
                int cx = Width / 2;
                int cy = Height / 2;
                g.FillEllipse(silhouette, cx - 22, cy - 36, 44, 44);
                g.FillPie(silhouette, cx - 44, cy + 12, 88, 66, 180, 180);
            }

            using var border = new Pen(UiStyle.BordureGrise, 1.2f);
            g.DrawPath(border, path);
        }
    }
}
