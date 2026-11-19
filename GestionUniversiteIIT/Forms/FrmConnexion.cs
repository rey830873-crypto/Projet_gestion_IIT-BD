using System.Drawing.Drawing2D;
using GestionUniversiteIIT.Data;
using GestionUniversiteIIT.Models;
using GestionUniversiteIIT.Services;

namespace GestionUniversiteIIT.Forms;

public sealed partial class FrmConnexion : Form
{
    private readonly AuthService _auth = new();
    private bool _afficherMotDePasse;
    private Image? _logo;

    public FrmConnexion()
    {
        ChargerLogo();
        InitializeComponent();
        ConstruireInterface();
        Shown += (_, _) =>
        {
            DatabaseSeeder.SynchroniserEtudiantsReels();
            VerifierSignalConnexionAsync();
        };
    }

    private void ConstruireInterface()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var topBar = new Panel { Dock = DockStyle.Fill };
        topBar.Paint += (_, e) =>
        {
            using var brush = new LinearGradientBrush(
                topBar.ClientRectangle,
                Color.FromArgb(115, 8, 13),
                Color.FromArgb(178, 14, 22),
                0f);
            e.Graphics.FillRectangle(brush, topBar.ClientRectangle);
            TextRenderer.DrawText(
                e.Graphics,
                "Gestion Université IIT",
                new Font("Segoe UI", 10.5f, FontStyle.Bold),
                new Rectangle(16, 0, 320, topBar.Height),
                Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };

        _pnlSignalSql.Dock = DockStyle.Right;
        _pnlSignalSql.Width = 270;
        _pnlSignalSql.Cursor = Cursors.Hand;
        _pnlSignalSql.BackColor = Color.Transparent;
        _pnlSignalSql.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var pillRect = new Rectangle(8, 6, _pnlSignalSql.Width - 20, _pnlSignalSql.Height - 12);
            using var path = UiStyle.CreerCheminArrondi(pillRect, 12);
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
                true => "SQL Connecté (GestionUniversiteIIT)",
                false => "SQL Déconnecté — Cliquer pour retester",
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
        _pnlSignalSql.Click += (_, _) => VerifierSignalConnexionAsync();
        topBar.Controls.Add(_pnlSignalSql);

        layout.Controls.Add(topBar, 0, 0);

        var scene = new PanneauCampus(_logo) { Dock = DockStyle.Fill };

        var card = new PanneauCarteConnexion
        {
            Size = new Size(370, 390)
        };

        var avatar = new Panel
        {
            Size = new Size(64, 54),
            Location = new Point((card.Width - 64) / 2, 16),
            BackColor = Color.Transparent
        };
        avatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(136, 10, 16));
            e.Graphics.FillEllipse(brush, 20, 4, 24, 24);
            e.Graphics.FillPie(brush, 8, 30, 48, 36, 180, 180);
        };
        card.Controls.Add(avatar);

        var lblTitre = new Label
        {
            Text = "Connexion",
            AutoSize = false,
            Size = new Size(card.Width - 40, 38),
            Location = new Point(20, 74),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 19f, FontStyle.Bold),
            ForeColor = UiStyle.Texte,
            BackColor = Color.Transparent
        };
        card.Controls.Add(lblTitre);

        var boxNom = CreerChampSaisie(
            _nom,
            "Nom d'utilisateur",
            estMotDePasse: false,
            onToggleEye: null);
        _nom.Text = "admin";
        boxNom.Location = new Point(32, 130);
        card.Controls.Add(boxNom);

        _motDePasse.UseSystemPasswordChar = true;
        _motDePasse.Text = "AdminIIT2026!";
        var boxMdp = CreerChampSaisie(
            _motDePasse,
            "Mot de passe",
            estMotDePasse: true,
            onToggleEye: () =>
            {
                _afficherMotDePasse = !_afficherMotDePasse;
                _motDePasse.UseSystemPasswordChar = !_afficherMotDePasse;
            });
        boxMdp.Location = new Point(32, 192);
        card.Controls.Add(boxMdp);

        var btnConnexion = UiStyle.BoutonMaquette(
            "Se connecter",
            Color.FromArgb(208, 16, 24),
            Color.FromArgb(156, 10, 16),
            Color.White,
            306,
            44);
        btnConnexion.Location = new Point(32, 262);
        btnConnexion.Click += (_, _) => OuvrirSession();
        card.Controls.Add(btnConnexion);

        var lblOublie = new Label
        {
            Text = "Mot de passe oublié ?",
            AutoSize = false,
            Size = new Size(306, 28),
            Location = new Point(32, 322),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(48, 54, 64),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        lblOublie.Click += (_, _) => OuvrirBoiteReinitialisation();
        card.Controls.Add(lblOublie);

        void PositionnerCarte()
        {
            int x = Math.Max(420, scene.ClientSize.Width - card.Width - 44);
            int y = Math.Max(24, (scene.ClientSize.Height - card.Height) / 2 + 12);
            card.Location = new Point(x, y);
        }

        scene.Controls.Add(card);
        scene.Resize += (_, _) => PositionnerCarte();
        PositionnerCarte();

        layout.Controls.Add(scene, 0, 1);
        Controls.Add(layout);
        AcceptButton = btnConnexion;
    }

    private void VerifierSignalConnexionAsync()
    {
        Task.Run(() =>
        {
            Connexion.TesterConnexion();
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(() =>
                {
                    if (!IsDisposed) _pnlSignalSql.Invalidate();
                });
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        });
    }

    private void ChargerLogo()
    {
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "logo.png");
        if (File.Exists(logoPath))
        {
            _logo = Image.FromFile(logoPath);
        }
    }


    private static Panel CreerChampSaisie(
        TextBox textBox,
        string placeholder,
        bool estMotDePasse,
        Action? onToggleEye)
    {
        var box = new PanneauArrondi
        {
            Size = new Size(306, 44),
            Rayon = 9,
            CouleurFond = Color.White,
            CouleurBordure = Color.FromArgb(204, 212, 224),
            Padding = new Padding(38, 11, estMotDePasse ? 38 : 12, 8)
        };

        textBox.BorderStyle = BorderStyle.None;
        textBox.Dock = DockStyle.Fill;
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10f);
        textBox.ForeColor = UiStyle.Texte;
        textBox.BackColor = Color.White;
        box.Controls.Add(textBox);

        box.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(28, 34, 44));
            using var pen = new Pen(Color.FromArgb(28, 34, 44), 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            if (!estMotDePasse)
            {
                // Icône utilisateur
                g.FillEllipse(brush, 15, 12, 9, 9);
                g.FillPie(brush, 11, 22, 17, 14, 180, 180);
            }
            else
            {
                // Icône cadenas
                g.DrawArc(pen, 15, 11, 9, 9, 180, 180);
                g.FillRectangle(brush, 13, 18, 13, 11);
                using var whiteBrush = new SolidBrush(Color.White);
                g.FillEllipse(whiteBrush, 18, 22, 3, 3);

                // Icône œil à droite
                int ex = box.Width - 22;
                int ey = box.Height / 2;
                g.DrawArc(pen, ex - 9, ey - 5, 18, 10, 0, 360);
                g.FillEllipse(brush, ex - 3, ey - 3, 6, 6);
            }
        };

        if (estMotDePasse && onToggleEye is not null)
        {
            box.MouseMove += (_, e) =>
            {
                box.Cursor = e.X >= box.Width - 36 ? Cursors.Hand : Cursors.IBeam;
            };
            box.MouseClick += (_, e) =>
            {
                if (e.X >= box.Width - 36)
                {
                    onToggleEye();
                    box.Invalidate();
                }
            };
        }

        return box;
    }

    private void OuvrirBoiteReinitialisation()
    {
        using var dialog = new Form
        {
            Text = "Réinitialiser mon mot de passe",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(420, 320),
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9.5f)
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 8
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        root.Controls.Add(UiStyle.Titre("Nouveau mot de passe", 13), 0, 0);

        var txtUser = new TextBox { Text = _nom.Text.Trim(), Dock = DockStyle.Fill };
        var txtMdp1 = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill };
        var txtMdp2 = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill };

        root.Controls.Add(new Label { Text = "Nom d'utilisateur :", AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, 1);
        root.Controls.Add(UiStyle.EncadrerChamp(txtUser, 370, 34), 0, 2);

        root.Controls.Add(new Label { Text = "Nouveau mot de passe (min. 10 car.) :", AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, 3);
        root.Controls.Add(UiStyle.EncadrerChamp(txtMdp1, 370, 34), 0, 4);

        root.Controls.Add(new Label { Text = "Confirmer le mot de passe :", AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }, 0, 5);
        root.Controls.Add(UiStyle.EncadrerChamp(txtMdp2, 370, 34), 0, 6);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0)
        };
        var btnValider = UiStyle.Bouton("Valider", true);
        var btnAnnuler = UiStyle.BoutonArdoise("Annuler", 110, 36);

        btnAnnuler.Click += (_, _) => dialog.Close();
        btnValider.Click += (_, _) =>
        {
            string user = txtUser.Text.Trim();
            string p1 = txtMdp1.Text;
            string p2 = txtMdp2.Text;

            if (string.IsNullOrWhiteSpace(user) || p1.Length < 10)
            {
                MessageBox.Show("Renseignez votre nom d'utilisateur et un mot de passe d'au moins 10 caractères.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (p1 != p2)
            {
                MessageBox.Show("Les deux mots de passe ne correspondent pas.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _auth.ChangerOuCreerMotDePasse(user, p1);
                _nom.Text = user;
                _motDePasse.Text = p1;
                MessageBox.Show("Votre mot de passe a été mis à jour avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        actions.Controls.Add(btnAnnuler);
        actions.Controls.Add(btnValider);
        root.Controls.Add(actions, 0, 7);

        dialog.Controls.Add(root);
        dialog.AcceptButton = btnValider;
        dialog.CancelButton = btnAnnuler;
        dialog.ShowDialog(this);
    }

    private void OuvrirSession()
    {
        if (string.IsNullOrWhiteSpace(_nom.Text) || string.IsNullOrEmpty(_motDePasse.Text))
        {
            MessageBox.Show("Saisissez votre nom d'utilisateur et votre mot de passe.", "Champs requis",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Utilisateur? utilisateur = null;
        try
        {
            if (!_auth.HasUsers())
            {
                string pwd = _motDePasse.Text.Length >= 10 ? _motDePasse.Text : _motDePasse.Text.PadRight(10, '0');
                _auth.CreateInitialAdministrator(_nom.Text, pwd);
                utilisateur = _auth.Authenticate(_nom.Text, pwd);
            }
            else
            {
                utilisateur = _auth.Authenticate(_nom.Text, _motDePasse.Text);
                if (utilisateur is null)
                {
                    MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.", "Connexion refusée",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Impossible de vérifier la connexion. Vérifiez la base de données et réessayez.\n\n{ex.Message}",
                "Connexion indisponible",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (utilisateur is null)
        {
            return;
        }

        using var accueil = new FrmAccueil(utilisateur);
        Hide();
        accueil.ShowDialog(this);
        if (accueil.DeconnexionDemandee)
        {
            _motDePasse.Clear();
            Show();
            Activate();
        }
        else
        {
            Close();
        }
    }

    private sealed class PanneauCarteConnexion : Panel
    {
        public PanneauCarteConnexion()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var shadowRect = new Rectangle(5, 7, Width - 8, Height - 8);
            using (var shadowPath = UiStyle.CreerCheminArrondi(shadowRect, 18))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(32, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            var cardRect = new Rectangle(1, 1, Width - 8, Height - 8);
            using var cardPath = UiStyle.CreerCheminArrondi(cardRect, 18);
            using (var fill = new SolidBrush(Color.White))
            using (var border = new Pen(Color.FromArgb(228, 232, 240), 1.2f))
            {
                e.Graphics.FillPath(fill, cardPath);
                e.Graphics.DrawPath(border, cardPath);
            }

            base.OnPaint(e);
        }
    }

    private sealed class PanneauCampus : Panel
    {
        private readonly Image? _logo;

        public PanneauCampus(Image? logo)
        {
            _logo = logo;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            // Ciel bleu dégradé lumineux
            using (var sky = new LinearGradientBrush(
                       rect,
                       Color.FromArgb(148, 198, 248),
                       Color.FromArgb(245, 250, 255),
                       15f))
            {
                g.FillRectangle(sky, rect);
            }

            // Halo blanc derrière le logo à gauche
            using (var leftGlow = new LinearGradientBrush(
                       new Rectangle(0, 0, Math.Max(100, rect.Width * 52 / 100), rect.Height),
                       Color.White,
                       Color.FromArgb(0, 255, 255, 255),
                       0f))
            {
                g.FillRectangle(leftGlow, 0, 0, rect.Width * 52 / 100, rect.Height);
            }

            int horizonY = rect.Height - 78;

            // Bâtiment secondaire à gauche
            int secX = 95;
            int secY = horizonY - 68;
            using (var roofBrush = new SolidBrush(Color.FromArgb(214, 124, 108)))
            using (var wallBrush = new SolidBrush(Color.FromArgb(236, 224, 212)))
            {
                g.FillRectangle(wallBrush, secX, secY + 22, 160, 46);
                Point[] roof = [new(secX - 10, secY + 22), new(secX + 80, secY), new(secX + 170, secY + 22)];
                g.FillPolygon(roofBrush, roof);
            }

            // Bâtiment principal IIT au centre
            int bldX = Math.Max(290, rect.Width * 36 / 100);
            int bldW = 250;
            int bldTop = horizonY - 235;
            int bldH = 235;

            using (var facadeBrush = new LinearGradientBrush(
                       new Rectangle(bldX, bldTop, bldW, bldH),
                       Color.FromArgb(244, 246, 249),
                       Color.FromArgb(212, 218, 226),
                       0f))
            using (var glassBrush = new LinearGradientBrush(
                       new Rectangle(bldX + 36, bldTop + 65, bldW - 72, bldH - 65),
                       Color.FromArgb(22, 76, 122),
                       Color.FromArgb(12, 42, 72),
                       90f))
            using (var gridPen = new Pen(Color.FromArgb(120, 180, 220), 1f))
            {
                // Aile basse gauche du bâtiment IIT
                g.FillRectangle(facadeBrush, bldX - 90, horizonY - 115, 95, 115);
                for (int col = 0; col < 3; col++)
                {
                    g.FillRectangle(glassBrush, bldX - 80 + col * 28, horizonY - 92, 18, 86);
                }

                // Corps principal
                Point[] pediment =
                [
                    new(bldX, bldTop + 34),
                    new(bldX + bldW, bldTop),
                    new(bldX + bldW, horizonY),
                    new(bldX, horizonY)
                ];
                g.FillPolygon(facadeBrush, pediment);

                // Vitrage bleu
                g.FillRectangle(glassBrush, bldX + 38, bldTop + 74, bldW - 76, bldH - 74);
                for (int gx = bldX + 62; gx < bldX + bldW - 40; gx += 26)
                {
                    g.DrawLine(gridPen, gx, bldTop + 74, gx, horizonY);
                }

                for (int gy = bldTop + 100; gy < horizonY; gy += 28)
                {
                    g.DrawLine(gridPen, bldX + 38, gy, bldX + bldW - 38, gy);
                }

                // Enseigne rouge "IIT" sur le fronton
                TextRenderer.DrawText(
                    g,
                    "IIT",
                    new Font("Segoe UI", 20f, FontStyle.Bold),
                    new Rectangle(bldX + 68, bldTop + 26, 90, 38),
                    UiStyle.Rouge,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // Pelouse et parvis en bas
            using (var lawnBrush = new SolidBrush(Color.FromArgb(118, 168, 72)))
            using (var roadBrush = new SolidBrush(Color.FromArgb(156, 164, 174)))
            using (var treeBrush = new SolidBrush(Color.FromArgb(54, 118, 48)))
            {
                g.FillRectangle(lawnBrush, 0, horizonY, rect.Width, rect.Height - horizonY);
                Point[] parvis =
                [
                    new(bldX - 40, horizonY),
                    new(bldX + bldW + 60, horizonY),
                    new(rect.Width, rect.Height),
                    new(bldX - 160, rect.Height)
                ];
                g.FillPolygon(roadBrush, parvis);

                // Quelques arbres verts
                g.FillEllipse(treeBrush, 24, horizonY - 46, 54, 54);
                g.FillEllipse(treeBrush, 215, horizonY - 38, 44, 44);
                g.FillEllipse(treeBrush, bldX + bldW - 48, horizonY - 68, 66, 66);
            }

            // Affichage du logo officiel à gauche (01_connexion.png)
            var logoRect = new Rectangle(38, 28, Math.Min(350, rect.Width * 40 / 100), Math.Min(360, rect.Height - 105));
            if (_logo is not null)
            {
                g.DrawImage(_logo, logoRect);
            }
            else
            {
                TextRenderer.DrawText(
                    g,
                    "Gestion\nUniversité IIT\n— Solution de Gestion Académique —",
                    new Font("Segoe UI", 22f, FontStyle.Bold),
                    logoRect,
                    UiStyle.Texte,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            base.OnPaint(e);
        }
    }
}
