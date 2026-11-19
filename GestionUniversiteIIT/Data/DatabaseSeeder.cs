using Microsoft.Data.SqlClient;

namespace GestionUniversiteIIT.Data;

public static class DatabaseSeeder
{
    private static readonly (string Matricule, string Nom, string Prenom)[] EtudiantsReels =
    [
        ("IIT-0606", "ADON", "LOÏC OTNIEL"),
        ("IIT-0454", "ADOU", "KOBENAN HABBIB EMMANUEL"),
        ("IIT-0430", "AGAMAH", "KOSSI MAMADI KARL-ELVIS"),
        ("IIT-0370", "AKRE", "AGOUASSE STEVE MAC AUREL"),
        ("IIT-0375", "ALLADAYE", "CHRISTIAN TOLIDJI URIEL"),
        ("IIT-0503", "ANVO", "AMON EMERAUDE MURIELLE"),
        ("IIT-0402", "ATOUKOUVI", "AFI ANNE CHRISTELLE"),
        ("IIT-0453", "BAZIE", "SYLVIE JOSIANE"),
        ("IIT-0412", "BIAGOU", "ELISE MARC-AUREL REGIS"),
        ("IIT-0359", "BLE", "GOUDE LAURENT CHARLES JUNIOR"),
        ("IIT-0400", "COULIBALY", "TENENA LOUIS MELVINE"),
        ("IIT-0455", "DE BLAISE", "N'DA AMETCHI PIERRE-JONATHAN"),
        ("IIT-0410", "DIABAGATE", "ILIAS HASSANE"),
        ("IIT-0429", "DJOCROU", "ROMEO CHRIS EMMANUEL"),
        ("IIT-0372", "EDERE", "HERVE REGINA"),
        ("IIT-0364", "EHOUSSOU", "BOMAN-N'GOUAN OINT PHANUEL"),
        ("IIT-0383", "ESSANE", "JEAN-NOEL"),
        ("IIT-0356", "EZOUA", "EMIAN CHRIST ESLI"),
        ("IIT-0365", "GODE", "AYEKPA EPIPHANIE MARIE-STELLA RACHEL"),
        ("IIT-0355", "KADJO", "KADJO JOEL TIMOTHEE"),
        ("IIT-0556", "KAMAGATE", "KHADY DJAMILA NAMIZATA"),
        ("IIT-0404", "KODJA", "MAHOUGNON ESTHER"),
        ("À ATTRIBUER", "KONATE", "ABDOUL KARIM"),
        ("IIT-0345", "KONE", "KHALIF-AARON"),
        ("IIT-0350", "KOUADIO", "KETSIA MARIE-AUDE AFFOUA"),
        ("IIT-0449", "KOUADIO", "N'DJABLIH KESSIE URIEL JEAN-MARTIN"),
        ("IIT-0363", "KOUAKOU", "GRACE ELIANE"),
        ("IIT-0413", "KOUAME", "RANIYA MARIE CARMEL"),
        ("IIT-0422", "KOUASSI", "ADONIS KOUAKOU JANSENIUS"),
        ("IIT-0394", "N'CHO", "N'CHACOU CHRIS-ROLAND PATRICE"),
        ("IIT-0444", "N'DOLI", "MOAYE MARIE EUNICE NAOMI"),
        ("IIT-0348", "N'DOUA", "KACOU PAUL ISRAEL"),
        ("IIT-0417", "N'GORAN", "LAEL JEAN CHARLES"),
        ("IIT-0339", "N'GUETTA", "CHRIST EMMANUEL"),
        ("IIT-0391", "OGOUBIYI", "ABDOUL MIFDOL FOLAHANMI ADIO FIACRE"),
        ("IIT-0418", "OUATTARA", "PEBARGNANA NOURDINE"),
        ("IIT-0398", "OULAYE", "DAPLE EVARISTE"),
        ("IIT-0378", "PALE", "SIE STEVEN LERICHE HERVE"),
        ("IIT-0357", "SAKI", "DJOROU CHRIS ADONIS"),
        ("IIT-0438", "SERI", "ZADJE ANGE MICHAEL EMERSON"),
        ("IIT-0373", "SEYNOU", "FATIM"),
        ("IIT-0371", "SIHOULI", "YOGNE JEAN-HOREB"),
        ("IIT-0415", "SILUE", "GNENEMA ABOUBACAR KADER"),
        ("IIT-0384", "TAH", "GUELASSE ARIEL YOHANN MONDESIR"),
        ("IIT-0409", "TAYE", "CHRIST EMMANUEL"),
        ("IIT-0591", "TIESSE", "LOU NANZIE ALLIANCE SEPHORA PRECIEUSE"),
        ("IIT-0366", "VRY", "LOBA JOSUE BENJAMIN"),
        ("IIT-0419", "YAPO", "BIENVENU CEDRICK BAKARY"),
        ("IIT-0414", "YAPO", "SOUKOULE CHEICK OUMAR JORDAN"),
        ("IIT-0340", "YOBOUET", "JEAN LUC DELOR KASSIE"),
        ("IIT-0458", "ZOUA", "JEAN YVES JUNIOR")
    ];

    private static bool _synchronise;

    public static void SynchroniserEtudiantsReels()
    {
        if (_synchronise)
        {
            return;
        }

        try
        {
            // Aligner les créneaux sur les horaires officiels IIT : 09h00-12h00 et 13h30-16h30
            DbHelper.Execute(
                """
                UPDATE dbo.Plannings SET HeureDebut = '09:00', HeureFin = '12:00' WHERE HeureDebut < '12:30' AND (HeureDebut <> '09:00' OR HeureFin <> '12:00');
                UPDATE dbo.Plannings SET HeureDebut = '13:30', HeureFin = '16:30' WHERE HeureDebut >= '12:30' AND (HeureDebut <> '13:30' OR HeureFin <> '16:30');
                """);

            object? dejaFait = DbHelper.Scalar("SELECT COUNT(*) FROM dbo.Etudiants WHERE Matricule = N'IIT-0606'");
            if (Convert.ToInt32(dejaFait ?? 0) == 0)
            {
                Inserer51EtudiantsReels();
            }

            AjouterEtudiantsSupplementaires(345);

            // Rééquilibrer L1 / L2 / L3 au sein de chaque filière pour qu'aucun couple (Filière, Niveau) ne soit vide
            DbHelper.Execute(
                """
                WITH CTE AS (
                    SELECT EtudiantId,
                           ROW_NUMBER() OVER (PARTITION BY FiliereId ORDER BY EtudiantId) - 1 AS Rn
                    FROM dbo.Etudiants
                )
                UPDATE e
                SET e.Niveau = CASE CTE.Rn % 3 WHEN 0 THEN N'L1' WHEN 1 THEN N'L2' ELSE N'L3' END
                FROM dbo.Etudiants e
                JOIN CTE ON CTE.EtudiantId = e.EtudiantId;
                """);

            // Alimenter quelques séances de présences/absences réelles si dbo.Absences est vide
            DbHelper.Execute(
                """
                IF NOT EXISTS (SELECT 1 FROM dbo.Absences)
                BEGIN
                    INSERT INTO dbo.Absences(CoursId, EtudiantId, DateCours, Present, Justification)
                    SELECT TOP 36
                        c.CoursId,
                        e.EtudiantId,
                        DATEADD(day, -(ABS(CHECKSUM(e.EtudiantId)) % 14), CONVERT(date, GETDATE())),
                        CASE WHEN (e.EtudiantId % 4) = 0 THEN 0 ELSE 1 END,
                        NULL
                    FROM dbo.Etudiants e
                    CROSS APPLY (SELECT TOP 1 CoursId FROM dbo.Cours c WHERE c.FiliereId = e.FiliereId ORDER BY c.CoursId) c
                    WHERE e.Matricule LIKE N'IIT-0%'
                    ORDER BY e.EtudiantId;
                END
                """);

            _synchronise = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseSeeder] {ex.Message}");
        }
    }

    private static void Inserer51EtudiantsReels()
    {
        using var conn = Connexion.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        using (var cmdDel = new SqlCommand(
            """
            DELETE a FROM dbo.Absences a JOIN dbo.Etudiants e ON e.EtudiantId = a.EtudiantId WHERE e.Matricule LIKE N'IIT00%';
            DELETE i FROM dbo.Inscriptions i JOIN dbo.Etudiants e ON e.EtudiantId = i.EtudiantId WHERE e.Matricule LIKE N'IIT00%';
            DELETE FROM dbo.Etudiants WHERE Matricule LIKE N'IIT00%';
            """, conn, tx))
        {
            cmdDel.ExecuteNonQuery();
        }

        var filieres = ChargerFilieres(conn, tx);
        if (filieres.Count == 0)
        {
            tx.Rollback();
            return;
        }

        string[] niveaux = ["L1", "L2", "L3"];
        for (int i = 0; i < EtudiantsReels.Length; i++)
        {
            var (matricule, nom, prenom) = EtudiantsReels[i];
            int filiereId = filieres[i % filieres.Count];
            string niveau = niveaux[i % niveaux.Length];

            using var cmdIns = new SqlCommand(
                """
                IF NOT EXISTS (SELECT 1 FROM dbo.Etudiants WHERE Matricule = @Matricule)
                    INSERT INTO dbo.Etudiants(Matricule, Nom, Prenom, FiliereId, Niveau, Actif)
                    VALUES (@Matricule, @Nom, @Prenom, @FiliereId, @Niveau, 1);
                """, conn, tx);
            cmdIns.Parameters.AddWithValue("@Matricule", matricule);
            cmdIns.Parameters.AddWithValue("@Nom", nom);
            cmdIns.Parameters.AddWithValue("@Prenom", prenom);
            cmdIns.Parameters.AddWithValue("@FiliereId", filiereId);
            cmdIns.Parameters.AddWithValue("@Niveau", niveau);
            cmdIns.ExecuteNonQuery();
        }

        AssurerInscriptions(conn, tx);
        tx.Commit();
    }

    private static void AjouterEtudiantsSupplementaires(int nombre)
    {
        object? dejaAjoutes = DbHelper.Scalar("SELECT COUNT(*) FROM dbo.Etudiants WHERE Matricule LIKE N'IIT-1%'");
        if (Convert.ToInt32(dejaAjoutes ?? 0) >= nombre)
        {
            return;
        }

        string[] noms =
        [
            "KOUASSI", "KONE", "TRAORE", "YAO", "KOUAME", "OUATTARA", "DIALLO", "BAMBA", "TOURE", "COULIBALY",
            "N'GUESSAN", "AKA", "BROU", "KOFFI", "DOUMBIA", "SANOGO", "SORO", "GUEI", "CISSE", "CAMARA",
            "FAYE", "YAPI", "ATSE", "ZADI", "SERI", "KRA", "TANO", "ASSI", "MENSAH", "DIOMANDE",
            "BAKAYOKO", "DIABATE", "LOUKOU", "BEUGRE", "SANGARE", "GNAGNE", "OUEDRAOGO", "N'DRI", "EHOUMAN", "KONAN"
        ];
        string[] prenomsM =
        [
            "Jean-Marc", "Ibrahim", "Emmanuel", "Youssouf", "Cédric", "Franck", "Ismaël", "Wilfried", "Karim", "Samuel",
            "Fabrice", "Lamine", "Brice", "Kevin", "Oumar", "Abdoul", "Rodrigue", "Hermann", "Moussa", "Seydou"
        ];
        string[] prenomsF =
        [
            "Aïssata", "Mariam", "Aline", "Prisca", "Estelle", "Grace", "Carine", "Ines", "Rachel", "Larissa",
            "Fatoumata", "Chantal", "Vanessa", "Rokia", "Melissa", "Mireille", "Sali", "Aminata", "Clarisse", "Nadia"
        ];
        string[] niveaux = ["L1", "L2", "L3"];

        using var conn = Connexion.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var filieres = ChargerFilieres(conn, tx);
        if (filieres.Count == 0)
        {
            tx.Rollback();
            return;
        }

        using var cmdIns = new SqlCommand(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.Etudiants WHERE Matricule = @Matricule)
                INSERT INTO dbo.Etudiants(Matricule, Nom, Prenom, FiliereId, Niveau, DateNaissance, Sexe, Actif)
                VALUES (@Matricule, @Nom, @Prenom, @FiliereId, @Niveau, @DateNaissance, @Sexe, 1);
            """, conn, tx);
        var pMat = cmdIns.Parameters.Add("@Matricule", System.Data.SqlDbType.NVarChar, 30);
        var pNom = cmdIns.Parameters.Add("@Nom", System.Data.SqlDbType.NVarChar, 100);
        var pPre = cmdIns.Parameters.Add("@Prenom", System.Data.SqlDbType.NVarChar, 100);
        var pFil = cmdIns.Parameters.Add("@FiliereId", System.Data.SqlDbType.Int);
        var pNiv = cmdIns.Parameters.Add("@Niveau", System.Data.SqlDbType.NVarChar, 20);
        var pDtn = cmdIns.Parameters.Add("@DateNaissance", System.Data.SqlDbType.Date);
        var pSex = cmdIns.Parameters.Add("@Sexe", System.Data.SqlDbType.NVarChar, 20);

        for (int i = 0; i < nombre; i++)
        {
            bool estFemme = i % 2 == 1;
            string nom = noms[i % noms.Length];
            string prenom = estFemme
                ? prenomsF[(i / 2 + i % 7) % prenomsF.Length]
                : prenomsM[(i / 2 + i % 5) % prenomsM.Length];
            string niveau = niveaux[i % niveaux.Length];
            int anneeNais = niveau switch { "L1" => 2006, "L2" => 2005, _ => 2004 };
            var dateNais = new DateTime(anneeNais, (i % 12) + 1, (i % 27) + 1);

            pMat.Value = $"IIT-{1001 + i:0000}";
            pNom.Value = nom;
            pPre.Value = prenom;
            pFil.Value = filieres[i % filieres.Count];
            pNiv.Value = niveau;
            pDtn.Value = dateNais;
            pSex.Value = estFemme ? "F" : "M";
            cmdIns.ExecuteNonQuery();
        }

        AssurerInscriptions(conn, tx);
        tx.Commit();
    }

    private static List<int> ChargerFilieres(SqlConnection conn, SqlTransaction tx)
    {
        var filieres = new List<int>();
        using var cmdFil = new SqlCommand("SELECT FiliereId FROM dbo.Filieres ORDER BY FiliereId", conn, tx);
        using var reader = cmdFil.ExecuteReader();
        while (reader.Read())
        {
            filieres.Add(reader.GetInt32(0));
        }
        return filieres;
    }

    private static void AssurerInscriptions(SqlConnection conn, SqlTransaction tx)
    {
        using var cmdInsc = new SqlCommand(
            """
            INSERT INTO dbo.Inscriptions(EtudiantId, AnneeAcademique, DateInscription)
            SELECT e.EtudiantId, N'2026 - 2027', CONVERT(date, '2026-10-08')
            FROM dbo.Etudiants e
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.Inscriptions i
                WHERE i.EtudiantId = e.EtudiantId AND i.AnneeAcademique = N'2026 - 2027'
            );
            """, conn, tx);
        cmdInsc.ExecuteNonQuery();
    }
}
