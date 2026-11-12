using System.Drawing.Drawing2D;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Models;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmAccueil : Form
{
    private readonly Utilisateur _utilisateur;
    private readonly List<BoutonMenuLateral> _boutonsMenu = [];
    private readonly System.Windows.Forms.Timer _timerSignalSql = new() { Interval = 15_000 };
    private readonly Action<bool, string> _onStatusChanged;
    private Form? _pageCourante;
    private Image? _logo;

    public bool DeconnexionDemandee { get; private set; }

    public FrmAccueil() : this(new Utilisateur(0, "Concepteur", "Administrateur")) { }

    public FrmAccueil(Utilisateur utilisateur)
    {
        _utilisateur = utilisateur;
        ChargerLogo();
        InitializeComponent();
        ConstruireInterface();

        _onStatusChanged = (_, _) =>
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => { if (!IsDisposed) _pnlSignalSql.Invalidate(); });
                }
                else
                {
                    _pnlSignalSql.Invalidate();
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        };
        Connexion.StatusChanged += _onStatusChanged;
        _timerSignalSql.Tick += (_, _) => VerifierSignalSqlAsync();
        Shown += (_, _) =>
        {
            _timerSignalSql.Start();
            VerifierSignalSqlAsync();
        };
        FormClosed += (_, _) =>
        {
            _timerSignalSql.Stop();
            _timerSignalSql.Dispose();
            Connexion.StatusChanged -= _onStatusChanged;
        };

        AfficherAccueil();
    }

    private static string ExtraireNomBase()
    {
        try
        {
            var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Connexion.ConnectionString);
            return string.IsNullOrWhiteSpace(b.InitialCatalog) ? "SQL" : b.InitialCatalog;
        }
        catch
        {
            return "SQL";
        }
    }

    private void VerifierSignalSqlAsync(bool ouvrirParametresSiEchec = false)
    {
        Task.Run(() =>
        {
            var res = Connexion.TesterConnexion();
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(() =>
                {
                    if (IsDisposed) return;
                    _pnlSignalSql.Invalidate();
                    if (!res.Succes && ouvrirParametresSiEchec)
                    {
                        NaviguerVers(IconeMenu.Parametres, new FrmParametres(_utilisateur.NomUtilisateur));
                    }
                });
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        });
    }

    private void ConstruireInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var menu = new PanneauMenuLateral(_logo)
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 96, 10, 10)
        };

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent
        };
        menu.Controls.Add(nav);

        AjouterBouton(nav, IconeMenu.Accueil, "Accueil", AfficherAccueil);
        AjouterBouton(nav, IconeMenu.Etudiants, "Étudiants", () => NaviguerVers(IconeMenu.Etudiants, new FrmEtudiants(OuvrirNouvelleInscription)));
        AjouterBouton(nav, IconeMenu.Inscriptions, "Inscriptions", OuvrirListeInscriptions);
        AjouterBouton(nav, IconeMenu.Enseignants, "Enseignants", () => NaviguerVers(IconeMenu.Enseignants, new FrmEnseignants()));
        AjouterBouton(nav, IconeMenu.Cours, "Cours", () => NaviguerVers(IconeMenu.Cours, new FrmCours()));
        AjouterBouton(nav, IconeMenu.Planification, "Planification", () => NaviguerVers(IconeMenu.Planification, new FrmPlanning()));
        AjouterBouton(nav, IconeMenu.Salles, "Salles", () => NaviguerVers(IconeMenu.Salles, new FrmSalles()));
        AjouterBouton(nav, IconeMenu.Absences, "Absences", OuvrirSaisieAbsences);
        AjouterBouton(nav, IconeMenu.Historique, "Historique", OuvrirHistoriqueAbsences);
        AjouterBouton(nav, IconeMenu.Rapports, "Rapports", () => NaviguerVers(IconeMenu.Rapports, new FrmStatistiques()));
        AjouterBouton(nav, IconeMenu.Parametres, "Paramètres", () => NaviguerVers(IconeMenu.Parametres, new FrmParametres(_utilisateur.NomUtilisateur)));
        AjouterBouton(nav, IconeMenu.Deconnexion, "Déconnexion", Deconnecter);

        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel { Dock = DockStyle.Fill };
        header.Paint += (_, e) =>
        {
            using var brush = new LinearGradientBrush(
                header.ClientRectangle,
                Color.FromArgb(108, 8, 13),
                Color.FromArgb(186, 14, 22),
                0f);
            e.Graphics.FillRectangle(brush, header.ClientRectangle);

            TextRenderer.DrawText(
                e.Graphics,
                "Gestion Université IIT",
                new Font("Segoe UI", 11f, FontStyle.Bold),
                new Rectangle(18, 0, 320, header.Height),
                Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };

        _pnlSignalSql.Dock = DockStyle.Right;
        _pnlSignalSql.Width = 290;
        _pnlSignalSql.Cursor = Cursors.Hand;
        _pnlSignalSql.BackColor = Color.Transparent;
        _pnlSignalSql.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var pillRect = new Rectangle(8, 7, _pnlSignalSql.Width - 22, _pnlSignalSql.Height - 14);
            using var path = UiStyle.CreerCheminArrondi(pillRect, 13);
            using var bg = new SolidBrush(Color.FromArgb(72, 0, 0, 0));
            e.Graphics.FillPath(bg, path);

            Color dotColor = Connexion.EstConnecte switch
            {
                true => Color.FromArgb(74, 222, 128),
                false => Color.FromArgb(248, 113, 113),
                _ => Color.FromArgb(251, 191, 36)
            };
            using var dotBrush = new SolidBrush(dotColor);
            e.Graphics.FillEllipse(dotBrush, pillRect.X + 10, pillRect.Y + (pillRect.Height - 10) / 2, 10, 10);

            string texteSignal = Connexion.EstConnecte switch
            {
                true => $"SQL Connecté ({ExtraireNomBase()})",
                false => "SQL Déconnecté — Cliquer pour vérifier",
                _ => "SQL : Vérification..."
            };

            TextRenderer.DrawText(
                e.Graphics,
                texteSignal,
                new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new Rectangle(pillRect.X + 26, pillRect.Y, pillRect.Width - 34, pillRect.Height),
                Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        };
        _pnlSignalSql.Click += (_, _) => VerifierSignalSqlAsync(ouvrirParametresSiEchec: true);
        header.Controls.Add(_pnlSignalSql);

        _contenu.Dock = DockStyle.Fill;
        _contenu.BackColor = Color.White;

        workspace.Controls.Add(header, 0, 0);
        workspace.Controls.Add(_contenu, 0, 1);

        root.Controls.Add(menu, 0, 0);
        root.Controls.Add(workspace, 1, 0);
        Controls.Add(root);
    }

    private void ChargerLogo()
    {
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "logo.png");
        if (File.Exists(logoPath))
        {
            _logo = Image.FromFile(logoPath);
        }
    }


    public void OuvrirNouvelleInscription()
    {
        SelectionnerMenu(IconeMenu.Inscriptions);
        Afficher(new FrmAjouterInscription(OuvrirListeInscriptions));
    }

    public void OuvrirListeInscriptions()
    {
        NaviguerVers(IconeMenu.Inscriptions, new FrmInscriptions(OuvrirNouvelleInscription));
    }

    public void OuvrirSaisieAbsences()
    {
        NaviguerVers(IconeMenu.Absences, new FrmSaisieAbsences(OuvrirHistoriqueAbsences));
    }

    public void OuvrirHistoriqueAbsences()
    {
        NaviguerVers(IconeMenu.Historique, new FrmHistoriqueAbsences(OuvrirSaisieAbsences));
    }

    private void NaviguerVers(IconeMenu icone, Form page)
    {
        SelectionnerMenu(icone);
        Afficher(page);
    }

    private void SelectionnerMenu(IconeMenu icone)
    {
        foreach (var bouton in _boutonsMenu)
        {
            bouton.EstActif = bouton.Icone == icone;
        }
    }

    private void AjouterBouton(FlowLayoutPanel nav, IconeMenu icone, string texte, Action action)
    {
        var bouton = new BoutonMenuLateral(icone, texte)
        {
            Width = 196,
            Height = 42,
            Margin = icone == IconeMenu.Deconnexion
                ? new Padding(0, 12, 0, 2)
                : new Padding(0, 2, 0, 2)
        };
        bouton.Click += (_, _) =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Impossible d'ouvrir cet écran : {exception.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        _boutonsMenu.Add(bouton);
        nav.Controls.Add(bouton);
    }

    private void AfficherAccueil()
    {
        SelectionnerMenu(IconeMenu.Accueil);

        var home = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(26, 18, 26, 22),
            ColumnCount = 1,
            RowCount = 3
        };
        home.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        home.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        home.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 1. En-tête "Bienvenue !" avec badge académique IIT
        var welcomePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var badge = new Panel
        {
            Size = new Size(56, 56),
            Location = new Point(6, 8),
            BackColor = Color.Transparent
        };
        badge.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var darkBlue = new SolidBrush(Color.FromArgb(30, 52, 82));
            using var red = new SolidBrush(UiStyle.Rouge);
            using var light = new SolidBrush(Color.FromArgb(236, 242, 250));
            e.Graphics.FillEllipse(light, 2, 2, 50, 50);
            e.Graphics.FillPie(darkBlue, 4, 8, 46, 44, 0, 180);
            e.Graphics.FillEllipse(red, 17, 10, 20, 24);
            TextRenderer.DrawText(
                e.Graphics,
                "IIT",
                new Font("Segoe UI", 7.5f, FontStyle.Bold),
                new Rectangle(17, 13, 20, 18),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        welcomePanel.Controls.Add(badge);

        welcomePanel.Controls.Add(new Label
        {
            Text = "Bienvenue !",
            AutoSize = true,
            Location = new Point(74, 8),
            Font = new Font("Segoe UI", 19.5f, FontStyle.Bold),
            ForeColor = UiStyle.Texte
        });
        welcomePanel.Controls.Add(new Label
        {
            Text = "Solution de Gestion Académique – IIT",
            AutoSize = true,
            Location = new Point(76, 43),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.FromArgb(92, 104, 122)
        });
        home.Controls.Add(welcomePanel, 0, 0);

        // 2. Quatre cartes KPI colorées (Étudiants, Cours, Enseignants, Inscriptions)
        var stats = new StatistiqueService().Charger();
        var kpiRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 8)
        };
        for (int i = 0; i < 4; i++)
        {
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Étudiants",
                stats.Etudiants,
                Color.FromArgb(30, 124, 218),
                Color.FromArgb(18, 88, 178),
                IconeMenu.Etudiants,
                valeurAuDessus: false,
                onClick: () => NaviguerVers(IconeMenu.Etudiants, new FrmEtudiants(OuvrirNouvelleInscription))),
            0, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Cours",
                stats.Cours,
                Color.FromArgb(214, 34, 40),
                Color.FromArgb(166, 16, 22),
                IconeMenu.Cours,
                valeurAuDessus: false,
                onClick: () => NaviguerVers(IconeMenu.Cours, new FrmCours())),
            1, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Enseignants",
                stats.Enseignants,
                Color.FromArgb(72, 162, 56),
                Color.FromArgb(44, 126, 32),
                IconeMenu.Enseignants,
                valeurAuDessus: false,
                onClick: () => NaviguerVers(IconeMenu.Enseignants, new FrmEnseignants())),
            2, 0);

        kpiRow.Controls.Add(
            UiStyle.CarteIndicateur(
                "Inscriptions",
                stats.Inscriptions,
                Color.FromArgb(242, 132, 20),
                Color.FromArgb(214, 98, 8),
                IconeMenu.Inscriptions,
                valeurAuDessus: false,
                onClick: OuvrirListeInscriptions),
            3, 0);

        home.Controls.Add(kpiRow, 0, 1);

        // 3. Section basse : "Inscriptions par niveau" (gauche) + "Actions rapides" (droite)
        var bottomGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 6, 0, 0)
        };
        bottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
        bottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

        var barChart = new PanneauGraphiqueBarres(stats.InscriptionsL1, stats.InscriptionsL2, stats.InscriptionsL3)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 4, 10, 4)
        };
        bottomGrid.Controls.Add(barChart, 0, 0);

        var quickCard = new PanneauArrondi
        {
            Dock = DockStyle.Fill,
            Rayon = 12,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(222, 226, 235),
            Padding = new Padding(20, 16, 20, 16),
            Margin = new Padding(10, 4, 6, 4)
        };

        var quickLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        quickLayout.Controls.Add(new Label
        {
            Text = "Actions rapides",
            AutoSize = true,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            Margin = new Padding(2, 2, 2, 8)
        }, 0, 0);

        var btnNouvelleInscription = new BoutonActionRapide("Nouvelle inscription", estPrimaire: true, IconeMenu.Inscriptions)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 5, 2, 5)
        };
        btnNouvelleInscription.Click += (_, _) => OuvrirNouvelleInscription();

        var btnPlanifierCours = new BoutonActionRapide("Planifier un cours", estPrimaire: false, IconeMenu.Planification)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 5, 2, 5)
        };
        btnPlanifierCours.Click += (_, _) => NaviguerVers(IconeMenu.Planification, new FrmPlanning());

        var btnSaisirAbsence = new BoutonActionRapide("Saisir une absence", estPrimaire: false, IconeMenu.Etudiants)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 5, 2, 5)
        };
        btnSaisirAbsence.Click += (_, _) => OuvrirSaisieAbsences();

        quickLayout.Controls.Add(btnNouvelleInscription, 0, 1);
        quickLayout.Controls.Add(btnPlanifierCours, 0, 2);
        quickLayout.Controls.Add(btnSaisirAbsence, 0, 3);
        quickCard.Controls.Add(quickLayout);

        bottomGrid.Controls.Add(quickCard, 1, 0);
        home.Controls.Add(bottomGrid, 0, 2);

        AfficherPanneau(home);
    }

    private void Afficher(Form page)
    {
        if (_pageCourante is not null)
        {
            _pageCourante.Dispose();
            _pageCourante = null;
        }

        _contenu.Controls.Clear();
        page.TopLevel = false;
        page.FormBorderStyle = FormBorderStyle.None;
        page.Dock = DockStyle.Fill;
        _contenu.Controls.Add(page);
        page.Show();
        _pageCourante = page;
    }

    private void AfficherPanneau(Control panneau)
    {
        _pageCourante?.Dispose();
        _pageCourante = null;
        _contenu.Controls.Clear();
        panneau.Dock = DockStyle.Fill;
        _contenu.Controls.Add(panneau);
    }

    private void Deconnecter()
    {
        if (MessageBox.Show("Voulez-vous vraiment vous déconnecter ?", "Déconnexion",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            DeconnexionDemandee = true;
            Close();
        }
    }

    private sealed class PanneauMenuLateral : Panel
    {
        private readonly Image? _logo;

        public PanneauMenuLateral(Image? logo)
        {
            _logo = logo;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            using (var bg = new LinearGradientBrush(
                       ClientRectangle,
                       Color.FromArgb(78, 11, 17),
                       Color.FromArgb(42, 5, 9),
                       90f))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            using (var shadow = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
            {
                g.FillRectangle(shadow, ClientRectangle.Right - 2, 0, 2, ClientRectangle.Height);
            }

            var divider = new Rectangle(12, 86, Width - 24, 1);
            using (var dividerBrush = new SolidBrush(Color.FromArgb(137, 73, 79)))
            {
                g.FillRectangle(dividerBrush, divider);
            }

            var circleRect = new Rectangle(16, 12, 68, 68);
            using (var whiteBrush = new SolidBrush(Color.FromArgb(252, 252, 252)))
            {
                g.FillEllipse(whiteBrush, circleRect);
            }

            if (_logo is not null)
            {
                g.DrawImage(_logo, new Rectangle(21, 16, 58, 58));
            }

            using (var glow = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
            {
                g.FillEllipse(glow, new Rectangle(14, 10, 72, 72));
            }

            TextRenderer.DrawText(
                g,
                "Gestion",
                new Font("Segoe UI", 11f, FontStyle.Bold | FontStyle.Italic),
                new Rectangle(92, 22, 120, 24),
                Color.FromArgb(255, 246, 247),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            var pillRect = new Rectangle(28, 49, 170, 26);
            using (var pillPath = UiStyle.CreerCheminArrondi(pillRect, 12))
            using (var pillBrush = new LinearGradientBrush(pillRect, Color.FromArgb(218, 35, 43), Color.FromArgb(160, 12, 18), 0f))
            {
                g.FillPath(pillBrush, pillPath);
            }

            TextRenderer.DrawText(
                g,
                "Université IIT",
                new Font("Segoe UI", 10f, FontStyle.Bold | FontStyle.Italic),
                pillRect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            base.OnPaint(e);
        }
    }

    private sealed class BoutonMenuLateral : Control
    {
        private bool _estActif;
        private bool _survol;

        public IconeMenu Icone { get; }

        public bool EstActif
        {
            get => _estActif;
            set
            {
                if (_estActif != value)
                {
                    _estActif = value;
                    Invalidate();
                }
            }
        }

        public BoutonMenuLateral(IconeMenu icone, string texte)
        {
            Icone = icone;
            Text = texte;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            ForeColor = Color.White;
            MouseEnter += (_, _) => { _survol = true; Invalidate(); };
            MouseLeave += (_, _) => { _survol = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(2, 1, Width - 4, Height - 2);
            if (_estActif)
            {
                using var path = UiStyle.CreerCheminArrondi(rect, 11);
                using var brush = new SolidBrush(Color.FromArgb(255, 246, 246, 248));
                g.FillPath(brush, path);

                using var indicatorPath = UiStyle.CreerCheminArrondi(
                    new Rectangle(2, 8, 4, Height - 16),
                    2);
                using var indicatorBrush = new SolidBrush(Color.FromArgb(196, 25, 36));
                g.FillPath(indicatorBrush, indicatorPath);
            }
            else
            {
                using var path = UiStyle.CreerCheminArrondi(rect, 11);
                var fond = _survol ? Color.FromArgb(136, 29, 36) : Color.FromArgb(91, 8, 14);
                using var brush = new SolidBrush(fond);
                g.FillPath(brush, path);
            }

            var foreground = _estActif
                ? Color.FromArgb(102, 18, 27)
                : Color.FromArgb(255, 247, 247, 248);
            UiStyle.DessinerIconeMenu(g, new Rectangle(16, (Height - 20) / 2, 20, 20), Icone, foreground);

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(48, 0, Width - 56, Height),
                foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    private sealed class BoutonActionRapide : Control
    {
        private readonly bool _estPrimaire;
        private readonly IconeMenu _icone;
        private bool _survol;

        public BoutonActionRapide(string texte, bool estPrimaire, IconeMenu icone)
        {
            Text = texte;
            _estPrimaire = estPrimaire;
            _icone = icone;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Height = 42;
            Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            MouseEnter += (_, _) => { _survol = true; Invalidate(); };
            MouseLeave += (_, _) => { _survol = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = UiStyle.CreerCheminArrondi(rect, 9);

            Color cHaut = _estPrimaire
                ? (_survol ? Color.FromArgb(212, 20, 28) : Color.FromArgb(190, 14, 22))
                : (_survol ? Color.FromArgb(228, 234, 244) : Color.FromArgb(238, 242, 248));
            Color cBas = _estPrimaire
                ? (_survol ? Color.FromArgb(168, 12, 18) : Color.FromArgb(148, 10, 15))
                : (_survol ? Color.FromArgb(214, 222, 234) : Color.FromArgb(224, 230, 240));

            using (var brush = new LinearGradientBrush(rect, cHaut, cBas, 90f))
            {
                g.FillPath(brush, path);
            }

            if (_estPrimaire)
            {
                using var penPlus = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                int cx = 28;
                int cy = Height / 2;
                g.DrawLine(penPlus, cx - 6, cy, cx + 6, cy);
                g.DrawLine(penPlus, cx, cy - 6, cx, cy + 6);
            }
            else
            {
                using var border = new Pen(Color.FromArgb(206, 214, 226), 1f);
                g.DrawPath(border, path);
                UiStyle.DessinerIconeMenu(g, new Rectangle(18, (Height - 20) / 2, 20, 20), _icone, UiStyle.Texte);
            }

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(48, 0, Width - 56, Height),
                _estPrimaire ? Color.White : UiStyle.Texte,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}
