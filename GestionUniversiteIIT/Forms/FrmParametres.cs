using System.Data;
using Microsoft.Data.SqlClient;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed class FrmParametres : Form
{
    private readonly string _utilisateur;
    private readonly AuthService _auth = new();
    private readonly CatalogueService _catalogue = new();

    private readonly TextBox _txtServeur = new() { ReadOnly = true };
    private readonly TextBox _txtBase = new() { ReadOnly = true };
    private readonly Label _lblStatutConnexion = new();

    private readonly DataGridView _gridFilieres = new();
    private readonly TextBox _txtNouvelleFiliere = new();

    private readonly TextBox _txtNouveauMdp = new();
    private readonly TextBox _txtConfirmerMdp = new();

    private readonly TextBox _txtNouvelUtilisateur = new();
    private readonly ComboBox _cboRoleUtilisateur = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtMdpUtilisateur = new();

    public FrmParametres(string utilisateur)
    {
        _utilisateur = utilisateur;
        Text = "Paramètres";
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;

        ConstruireInterface();
        ChargerDonnees();
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
            Padding = new Padding(20)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 5; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreerLabelTitre("Serveur SQL :"), 0, 0);
        _txtServeur.Dock = DockStyle.Fill;
        _txtServeur.Font = new Font("Segoe UI", 10f);
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtServeur, 340, 36), 1, 0);

        layout.Controls.Add(CreerLabelTitre("Base de données :"), 0, 1);
        _txtBase.Dock = DockStyle.Fill;
        _txtBase.Font = new Font("Segoe UI", 10f);
        layout.Controls.Add(UiStyle.EncadrerChamp(_txtBase, 340, 36), 1, 1);

        layout.Controls.Add(CreerLabelTitre("Statut de connexion :"), 0, 2);
        _lblStatutConnexion.AutoSize = true;
        _lblStatutConnexion.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _lblStatutConnexion.ForeColor = UiStyle.Gris;
        _lblStatutConnexion.Anchor = AnchorStyles.Left;
        _lblStatutConnexion.Text = "Non vérifié";
        layout.Controls.Add(_lblStatutConnexion, 1, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 8)
        };

        var btnTester = UiStyle.Bouton("Tester la connexion SQL", true);
        btnTester.Click += (_, _) => TesterConnexion();

        var btnReinitialiser = UiStyle.BoutonArdoise("Réinitialiser données démo", 220, 38);
        btnReinitialiser.Click += (_, _) => ReinitialiserDemo();

        actions.Controls.Add(btnTester);
        actions.Controls.Add(btnReinitialiser);
        layout.Controls.Add(actions, 1, 3);

        var lblAide = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Pour basculer d'une instance SQL locale à un serveur distant, modifiez la chaîne de connexion dans 'App.config' et relancez l'application.",
            ForeColor = UiStyle.Gris,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Italic)
        };
        layout.Controls.Add(lblAide, 1, 4);

        page.Controls.Add(layout);
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
        }
        catch
        {
            _txtServeur.Text = ".\\SQLEXPRESS";
            _txtBase.Text = "GestionUniversiteIIT";
        }

        RechargerFilieres();
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
        try
        {
            using var con = Connexion.Create();
            con.Open();
            _lblStatutConnexion.Text = "Connecté à SQL Server (Succès)";
            _lblStatutConnexion.ForeColor = Color.FromArgb(24, 134, 48);
            MessageBox.Show("La connexion à SQL Server est opérationnelle !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _lblStatutConnexion.Text = "Mode Démo / Hors-ligne (SQL indisponible)";
            _lblStatutConnexion.ForeColor = UiStyle.Rouge;
            MessageBox.Show($"Impossible de se connecter à SQL Server : {ex.Message}\n\nL'application tourne en mode Démo automatique avec les données des maquettes.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ReinitialiserDemo()
    {
        if (MessageBox.Show("Voulez-vous réinitialiser toutes les données de secours en mémoire aux valeurs exactes des maquettes ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            CatalogueService.ReinitialiserDonneesSecours();
            RechargerFilieres();
            MessageBox.Show("Les données de démonstration ont été réinitialisées.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
