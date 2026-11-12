using System.Data;
using Microsoft.Data.SqlClient;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmParametres : Form
{
    private readonly string _utilisateur;
    private readonly AuthService _auth = new();
    private readonly CatalogueService _catalogue = new();

    public FrmParametres() : this("Concepteur") { }

    public FrmParametres(string utilisateur)
    {
        _utilisateur = utilisateur;
        InitializeComponent();
        ConstruireInterface();
        if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            ChargerDonnees();
        }
    }

    private void ConstruireInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 18, 26, 22),
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(UiStyle.EntetePage(IconeMenu.Parametres, "Paramètres & Administration"), 0, 0);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold)
        };

        tabs.TabPages.Add(CreerOngletConnexion());
        tabs.TabPages.Add(CreerOngletFilieres());
        tabs.TabPages.Add(CreerOngletSecurite());

        root.Controls.Add(tabs, 0, 1);
        Controls.Add(root);
    }


    private TabPage CreerOngletConnexion()
    {
        var page = new TabPage("Base de données & Connexion")
        {
            BackColor = Color.White,
            Padding = new Padding(16)
        };

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        // Colonne gauche : vrais paramètres de connexion SQL Server
        var cardParams = UiStyle.CarteArrondie();
        cardParams.Padding = new Padding(18);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 8; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreerLabelTitre("Serveur SQL :"), 0, 0);
        _txtServeur.Dock = DockStyle.Fill;
        _txtServeur.Font = new Font("Segoe UI", 10f);
        _txtServeur.PlaceholderText = "ex: ., 127.0.0.1, .\\SQLEXPRESS";
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtServeur, 340, 34), 1, 0);

        layout.Controls.Add(CreerLabelTitre("Base de données :"), 0, 1);
        _txtBase.Dock = DockStyle.Fill;
        _txtBase.Font = new Font("Segoe UI", 10f);
        _txtBase.PlaceholderText = "ex: GestionUniversiteIIT";
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtBase, 340, 34), 1, 1);

        layout.Controls.Add(CreerLabelTitre("Authentification :"), 0, 2);
        _cboAuth.Items.Clear();
        _cboAuth.Items.AddRange(["Authentification Windows (Intégrée)", "Authentification SQL Server"]);
        _cboAuth.SelectedIndex = 0;
        _cboAuth.Font = new Font("Segoe UI", 9.5f);
        _cboAuth.Width = 340;
        _cboAuth.Anchor = AnchorStyles.Left;
        _cboAuth.SelectedIndexChanged += (_, _) => BasculerModeAuth();
        layout.Controls.Add(_cboAuth, 1, 2);

        layout.Controls.Add(CreerLabelTitre("Identifiant SQL :"), 0, 3);
        _txtUserSql.Dock = DockStyle.Fill;
        _txtUserSql.Font = new Font("Segoe UI", 10f);
        _txtUserSql.PlaceholderText = "ex: sa (si SQL Auth)";
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtUserSql, 340, 34), 1, 3);

        layout.Controls.Add(CreerLabelTitre("Mot de passe SQL :"), 0, 4);
        _txtPassSql.Dock = DockStyle.Fill;
        _txtPassSql.Font = new Font("Segoe UI", 10f);
        _txtPassSql.PlaceholderText = "Mot de passe SQL Server";
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtPassSql, 340, 34), 1, 4);

        layout.Controls.Add(CreerLabelTitre("Options SSL :"), 0, 5);
        var pnlSsl = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        _chkEncrypt.Font = new Font("Segoe UI", 9f);
        _chkTrustCert.Font = new Font("Segoe UI", 9f);
        pnlSsl.Controls.Add(_chkEncrypt);
        pnlSsl.Controls.Add(_chkTrustCert);
        layout.Controls.Add(pnlSsl, 1, 5);

        layout.Controls.Add(CreerLabelTitre("Signal de connexion :"), 0, 6);
        var pnlSignal = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };
        _pnlPastilleSignal.Margin = new Padding(0, 3, 8, 0);
        _pnlPastilleSignal.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var couleur = Connexion.EstConnecte switch
            {
                true => Color.FromArgb(24, 158, 62),
                false => UiStyle.Rouge,
                _ => Color.FromArgb(217, 119, 6)
            };
            using var brush = new SolidBrush(couleur);
            e.Graphics.FillEllipse(brush, 1, 1, 11, 11);
        };
        _lblStatutConnexion.AutoSize = true;
        _lblStatutConnexion.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _lblStatutConnexion.ForeColor = UiStyle.Gris;
        _lblStatutConnexion.Text = "Vérification en cours...";
        pnlSignal.Controls.Add(_pnlPastilleSignal);
        pnlSignal.Controls.Add(_lblStatutConnexion);
        layout.Controls.Add(pnlSignal, 1, 6);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            Padding = new Padding(0, 4, 0, 0)
        };

        var btnTester = UiStyle.Bouton("Tester la connexion", true);
        btnTester.Size = new Size(175, 38);
        btnTester.Click += (_, _) => TesterConnexion();

        var btnEnregistrer = UiStyle.Bouton("Enregistrer & Appliquer", true);
        btnEnregistrer.Size = new Size(195, 38);
        btnEnregistrer.BackColor = Color.FromArgb(24, 134, 48);
        btnEnregistrer.ForeColor = Color.White;
        btnEnregistrer.Click += (_, _) => EnregistrerEtAppliquerConnexion();

        actions.Controls.Add(btnTester);
        actions.Controls.Add(btnEnregistrer);
        layout.Controls.Add(actions, 1, 7);

        cardParams.Controls.Add(layout);
        split.Controls.Add(cardParams, 0, 0);

        // Colonne droite : état interactif des tables SQL Server
        var cardEtat = UiStyle.CarteArrondie();
        cardEtat.Padding = new Padding(16);
        cardEtat.Margin = new Padding(14, 0, 0, 0);

        var etatLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        etatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        etatLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        etatLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var lblTitreEtat = new Label
        {
            Text = "État en direct des tables SQL Server",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            TextAlign = ContentAlignment.MiddleLeft
        };
        etatLayout.Controls.Add(lblTitreEtat, 0, 0);

        UiStyle.ConfigurerGrille(_gridEtatTables);
        _gridEtatTables.Dock = DockStyle.Fill;
        _gridEtatTables.AutoGenerateColumns = true;
        etatLayout.Controls.Add(_gridEtatTables, 0, 1);

        var btnActualiserTables = UiStyle.BoutonArdoise("Actualiser l'état des tables", 220, 36);
        btnActualiserTables.Anchor = AnchorStyles.Left;
        btnActualiserTables.Click += (_, _) => ActualiserEtatTables();
        etatLayout.Controls.Add(btnActualiserTables, 0, 2);

        cardEtat.Controls.Add(etatLayout);
        split.Controls.Add(cardEtat, 1, 0);

        page.Controls.Add(split);
        return page;
    }

    private TabPage CreerOngletFilieres()
    {
        var page = new TabPage("Gestion des Filières")
        {
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        _txtNouvelleFiliere.PlaceholderText = "Nom de la nouvelle filière...";
        _txtNouvelleFiliere.Width = 260;
        var boxFiliere = UiStyle.EncadrerChamp(_txtNouvelleFiliere, 280, 36);

        var btnAjouter = UiStyle.Bouton("+ Ajouter filière", true);
        btnAjouter.Click += (_, _) => AjouterFiliere();

        var btnSupprimer = UiStyle.BoutonArdoise("Supprimer sélection", 170, 38);
        btnSupprimer.Click += (_, _) => SupprimerFiliere();

        bar.Controls.Add(boxFiliere);
        bar.Controls.Add(btnAjouter);
        bar.Controls.Add(btnSupprimer);
        layout.Controls.Add(bar, 0, 0);

        UiStyle.ConfigurerGrille(_gridFilieres);
        _gridFilieres.Dock = DockStyle.Fill;
        _gridFilieres.AutoGenerateColumns = true;

        var card = UiStyle.CarteArrondie();
        card.Controls.Add(_gridFilieres);
        layout.Controls.Add(card, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreerOngletSecurite()
    {
        var page = new TabPage("Comptes & Sécurité")
        {
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        var rootSec = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        rootSec.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        rootSec.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // 1. Modifier son propre mot de passe
        var cardPerso = UiStyle.CarteArrondie();
        cardPerso.Padding = new Padding(18);
        var tablePerso = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6
        };
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tablePerso.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        tablePerso.Controls.Add(UiStyle.Titre($"Mon compte ({_utilisateur})", 13), 0, 0);

        tablePerso.Controls.Add(CreerLabelTitre("Nouveau mot de passe (min. 10 car.) :"), 0, 1);
        _txtNouveauMdp.UseSystemPasswordChar = true;
        tablePerso.Controls.Add(UiStyle.EncadrerChamp(_txtNouveauMdp, 280, 36), 0, 2);

        tablePerso.Controls.Add(CreerLabelTitre("Confirmer le mot de passe :"), 0, 3);
        _txtConfirmerMdp.UseSystemPasswordChar = true;
        tablePerso.Controls.Add(UiStyle.EncadrerChamp(_txtConfirmerMdp, 280, 36), 0, 4);

        var btnChangerMdp = UiStyle.Bouton("Modifier mon mot de passe", true);
        btnChangerMdp.Click += (_, _) => ModifierMotDePassePerso();
        tablePerso.Controls.Add(btnChangerMdp, 0, 5);

        cardPerso.Controls.Add(tablePerso);
        rootSec.Controls.Add(cardPerso, 0, 0);

        // 2. Créer un nouvel utilisateur
        var cardNewUser = UiStyle.CarteArrondie();
        cardNewUser.Padding = new Padding(18);
        var tableNewUser = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8
        };
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tableNewUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        tableNewUser.Controls.Add(UiStyle.Titre("Créer un nouvel utilisateur", 13), 0, 0);

        tableNewUser.Controls.Add(CreerLabelTitre("Nom d'utilisateur :"), 0, 1);
        tableNewUser.Controls.Add(UiStyle.EncadrerChamp(_txtNouvelUtilisateur, 280, 36), 0, 2);

        tableNewUser.Controls.Add(CreerLabelTitre("Rôle :"), 0, 3);
        _cboRoleUtilisateur.Items.AddRange(["Administrateur", "Scolarité", "Enseignant"]);
        _cboRoleUtilisateur.SelectedIndex = 0;
        tableNewUser.Controls.Add(UiStyle.EncadrerChamp(_cboRoleUtilisateur, 280, 36), 0, 4);

        tableNewUser.Controls.Add(CreerLabelTitre("Mot de passe initial (min. 10 car.) :"), 0, 5);
        _txtMdpUtilisateur.UseSystemPasswordChar = true;
        tableNewUser.Controls.Add(UiStyle.EncadrerChamp(_txtMdpUtilisateur, 280, 36), 0, 6);

        var btnCreerUser = UiStyle.Bouton("+ Créer l'utilisateur", true);
        btnCreerUser.Click += (_, _) => CreerNouvelUtilisateur();
        tableNewUser.Controls.Add(btnCreerUser, 0, 7);

        cardNewUser.Controls.Add(tableNewUser);
        rootSec.Controls.Add(cardNewUser, 1, 0);

        page.Controls.Add(rootSec);
        return page;
    }

    private static Label CreerLabelTitre(string texte) => new()
    {
        Text = texte,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = UiStyle.Texte
    };

    private void ChargerDonnees()
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(Connexion.ConnectionString);
            _txtServeur.Text = builder.DataSource;
            _txtBase.Text = builder.InitialCatalog;
            _cboAuth.SelectedIndex = builder.IntegratedSecurity ? 0 : 1;
            _txtUserSql.Text = builder.UserID;
            _txtPassSql.Text = builder.Password;
            _chkEncrypt.Checked = builder.Encrypt != SqlConnectionEncryptOption.Optional;
            _chkTrustCert.Checked = builder.TrustServerCertificate;
        }
        catch
        {
            _txtServeur.Text = ".";
            _txtBase.Text = "GestionUniversiteIIT";
            _cboAuth.SelectedIndex = 0;
        }

        BasculerModeAuth();
        TesterConnexionSilencieux();
        ActualiserEtatTables();
        RechargerFilieres();
    }

    private void BasculerModeAuth()
    {
        bool sqlAuth = _cboAuth.SelectedIndex == 1;
        _txtUserSql.Enabled = sqlAuth;
        _txtPassSql.Enabled = sqlAuth;
    }

    private string ConstruireChaineDepuisFormulaire()
    {
        return Connexion.ConstruireChaine(
            _txtServeur.Text,
            _txtBase.Text,
            _cboAuth.SelectedIndex == 0,
            _txtUserSql.Text,
            _txtPassSql.Text,
            _chkEncrypt.Checked,
            _chkTrustCert.Checked);
    }

    private void MettreAJourSignalConnexion(bool? connecte, string texte)
    {
        _lblStatutConnexion.Text = texte;
        _lblStatutConnexion.ForeColor = connecte switch
        {
            true => Color.FromArgb(24, 134, 48),
            false => UiStyle.Rouge,
            _ => UiStyle.Gris
        };
        _pnlPastilleSignal.Invalidate();
    }

    private void TesterConnexionSilencieux()
    {
        var res = Connexion.TesterConnexion();
        MettreAJourSignalConnexion(res.Succes, res.Succes
            ? $"Connecté à {_txtBase.Text.Trim()} — SQL {res.VersionServeur} ({res.LatenceMs} ms)"
            : res.Message);
    }

    private void RechargerFilieres()
    {
        try
        {
            _gridFilieres.DataSource = _catalogue.ListerFilieres();
            if (_gridFilieres.Columns.Contains("Id"))
            {
                _gridFilieres.Columns["Id"]!.FillWeight = 20;
                _gridFilieres.Columns["Id"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur chargement filières : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TesterConnexion()
    {
        if (string.IsNullOrWhiteSpace(_txtServeur.Text) || string.IsNullOrWhiteSpace(_txtBase.Text))
        {
            MessageBox.Show("Renseignez le serveur SQL et le nom de la base de données.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string candidate = ConstruireChaineDepuisFormulaire();
        var res = Connexion.TesterConnexion(candidate);
        MettreAJourSignalConnexion(res.Succes, res.Succes
            ? $"Connecté à {_txtBase.Text.Trim()} — SQL {res.VersionServeur} ({res.LatenceMs} ms)"
            : res.Message);

        if (res.Succes)
        {
            MessageBox.Show($"Connexion SQL Server réussie !\nServeur : {_txtServeur.Text.Trim()}\nBase : {_txtBase.Text.Trim()}\nVersion : {res.VersionServeur}\nLatence : {res.LatenceMs} ms",
                "Connexion réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show($"Échec de connexion à SQL Server :\n{res.Message}",
                "Erreur de connexion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void EnregistrerEtAppliquerConnexion()
    {
        if (string.IsNullOrWhiteSpace(_txtServeur.Text) || string.IsNullOrWhiteSpace(_txtBase.Text))
        {
            MessageBox.Show("Le serveur SQL et la base de données sont obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string candidate = ConstruireChaineDepuisFormulaire();
        var test = Connexion.TesterConnexion(candidate);
        if (!test.Succes)
        {
            var choix = MessageBox.Show(
                $"Le test de connexion a échoué :\n{test.Message}\n\nVoulez-vous tout de même enregistrer ces paramètres ?",
                "Connexion non vérifiée",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (choix != DialogResult.Yes)
            {
                return;
            }
        }

        try
        {
            Connexion.SauvegarderEtAppliquer(
                _txtServeur.Text,
                _txtBase.Text,
                _cboAuth.SelectedIndex == 0,
                _txtUserSql.Text,
                _txtPassSql.Text,
                _chkEncrypt.Checked,
                _chkTrustCert.Checked);

            DbHelper.ReinitialiserDelaiReessai();
            TesterConnexionSilencieux();
            ActualiserEtatTables();
            RechargerFilieres();

            MessageBox.Show("Paramètres SQL Server enregistrés et appliqués immédiatement.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible d'enregistrer la configuration : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActualiserEtatTables()
    {
        var table = new DataTable();
        table.Columns.Add("Table SQL", typeof(string));
        table.Columns.Add("Lignes", typeof(string));

        string[] tables = ["Filieres", "Etudiants", "Enseignants", "Cours", "Salles", "Inscriptions", "Plannings", "Absences", "Utilisateurs"];
        try
        {
            using var con = Connexion.Create(ConstruireChaineDepuisFormulaire());
            con.Open();
            foreach (var nom in tables)
            {
                using var cmd = new SqlCommand($"SELECT COUNT(*) FROM dbo.[{nom}]", con) { CommandTimeout = 5 };
                object? val = cmd.ExecuteScalar();
                table.Rows.Add(nom, Convert.ToInt32(val ?? 0).ToString("N0"));
            }
        }
        catch
        {
            foreach (var nom in tables)
            {
                table.Rows.Add(nom, "Indisponible");
            }
        }

        _gridEtatTables.DataSource = table;
        if (_gridEtatTables.Columns.Contains("Lignes"))
        {
            _gridEtatTables.Columns["Lignes"]!.FillWeight = 35;
            _gridEtatTables.Columns["Lignes"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }

    private void AjouterFiliere()
    {
        string nom = _txtNouvelleFiliere.Text.Trim();
        if (string.IsNullOrWhiteSpace(nom))
        {
            MessageBox.Show("Saisissez le nom de la filière.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _catalogue.AjouterFiliere(nom);
            _txtNouvelleFiliere.Clear();
            RechargerFilieres();
            MessageBox.Show($"Filière '{nom}' ajoutée avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SupprimerFiliere()
    {
        if (_gridFilieres.CurrentRow is null || !_gridFilieres.Columns.Contains("Id"))
        {
            MessageBox.Show("Sélectionnez une filière à supprimer.", "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int id = Convert.ToInt32(_gridFilieres.CurrentRow.Cells["Id"].Value);
        if (MessageBox.Show("Confirmer la suppression de cette filière ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                _catalogue.SupprimerFiliere(id);
                RechargerFilieres();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Suppression impossible : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ModifierMotDePassePerso()
    {
        string mdp1 = _txtNouveauMdp.Text;
        string mdp2 = _txtConfirmerMdp.Text;

        if (mdp1.Length < 10)
        {
            MessageBox.Show("Le mot de passe doit comporter au moins 10 caractères.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (mdp1 != mdp2)
        {
            MessageBox.Show("Les deux mots de passe ne correspondent pas.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _auth.ChangerOuCreerMotDePasse(_utilisateur, mdp1);
            _txtNouveauMdp.Clear();
            _txtConfirmerMdp.Clear();
            MessageBox.Show("Votre mot de passe a été modifié avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CreerNouvelUtilisateur()
    {
        string nom = _txtNouvelUtilisateur.Text.Trim();
        string role = Convert.ToString(_cboRoleUtilisateur.SelectedItem) ?? "Administrateur";
        string mdp = _txtMdpUtilisateur.Text;

        if (string.IsNullOrWhiteSpace(nom) || mdp.Length < 10)
        {
            MessageBox.Show("Saisissez un nom d'utilisateur et un mot de passe d'au moins 10 caractères.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _auth.ChangerOuCreerMotDePasse(nom, mdp, role);
            _txtNouvelUtilisateur.Clear();
            _txtMdpUtilisateur.Clear();
            MessageBox.Show($"Utilisateur '{nom}' ({role}) créé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
