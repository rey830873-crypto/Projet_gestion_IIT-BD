USE GestionUniversiteIIT;
GO

-- Remplace les étudiants de test initiaux (IIT0001..IIT0040) par les 51 étudiants réels (matricules et noms uniquement)
IF NOT EXISTS (SELECT 1 FROM dbo.Etudiants WHERE Matricule = N'IIT-0606')
BEGIN
    DELETE a
    FROM dbo.Absences a
    JOIN dbo.Etudiants e ON e.EtudiantId = a.EtudiantId
    WHERE e.Matricule LIKE N'IIT00%';

    DELETE i
    FROM dbo.Inscriptions i
    JOIN dbo.Etudiants e ON e.EtudiantId = i.EtudiantId
    WHERE e.Matricule LIKE N'IIT00%';

    DELETE FROM dbo.Etudiants WHERE Matricule LIKE N'IIT00%';

    DECLARE @FiliereIds TABLE (Idx int IDENTITY(0,1), FiliereId int);
    INSERT INTO @FiliereIds(FiliereId)
    SELECT FiliereId FROM dbo.Filieres ORDER BY FiliereId;
    DECLARE @NbFilieres int = (SELECT COUNT(*) FROM @FiliereIds);

    DECLARE @Source TABLE (RowNum int IDENTITY(0,1), Matricule nvarchar(30), Nom nvarchar(100), Prenom nvarchar(100));
    INSERT INTO @Source(Matricule, Nom, Prenom) VALUES
        (N'IIT-0606', N'ADON', N'LOÏC OTNIEL'),
        (N'IIT-0454', N'ADOU', N'KOBENAN HABBIB EMMANUEL'),
        (N'IIT-0430', N'AGAMAH', N'KOSSI MAMADI KARL-ELVIS'),
        (N'IIT-0370', N'AKRE', N'AGOUASSE STEVE MAC AUREL'),
        (N'IIT-0375', N'ALLADAYE', N'CHRISTIAN TOLIDJI URIEL'),
        (N'IIT-0503', N'ANVO', N'AMON EMERAUDE MURIELLE'),
        (N'IIT-0402', N'ATOUKOUVI', N'AFI ANNE CHRISTELLE'),
        (N'IIT-0453', N'BAZIE', N'SYLVIE JOSIANE'),
        (N'IIT-0412', N'BIAGOU', N'ELISE MARC-AUREL REGIS'),
        (N'IIT-0359', N'BLE', N'GOUDE LAURENT CHARLES JUNIOR'),
        (N'IIT-0400', N'COULIBALY', N'TENENA LOUIS MELVINE'),
        (N'IIT-0455', N'DE BLAISE', N'N''DA AMETCHI PIERRE-JONATHAN'),
        (N'IIT-0410', N'DIABAGATE', N'ILIAS HASSANE'),
        (N'IIT-0429', N'DJOCROU', N'ROMEO CHRIS EMMANUEL'),
        (N'IIT-0372', N'EDERE', N'HERVE REGINA'),
        (N'IIT-0364', N'EHOUSSOU', N'BOMAN-N''GOUAN OINT PHANUEL'),
        (N'IIT-0383', N'ESSANE', N'JEAN-NOEL'),
        (N'IIT-0356', N'EZOUA', N'EMIAN CHRIST ESLI'),
        (N'IIT-0365', N'GODE', N'AYEKPA EPIPHANIE MARIE-STELLA RACHEL'),
        (N'IIT-0355', N'KADJO', N'KADJO JOEL TIMOTHEE'),
        (N'IIT-0556', N'KAMAGATE', N'KHADY DJAMILA NAMIZATA'),
        (N'IIT-0404', N'KODJA', N'MAHOUGNON ESTHER'),
        (N'À ATTRIBUER', N'KONATE', N'ABDOUL KARIM'),
        (N'IIT-0345', N'KONE', N'KHALIF-AARON'),
        (N'IIT-0350', N'KOUADIO', N'KETSIA MARIE-AUDE AFFOUA'),
        (N'IIT-0449', N'KOUADIO', N'N''DJABLIH KESSIE URIEL JEAN-MARTIN'),
        (N'IIT-0363', N'KOUAKOU', N'GRACE ELIANE'),
        (N'IIT-0413', N'KOUAME', N'RANIYA MARIE CARMEL'),
        (N'IIT-0422', N'KOUASSI', N'ADONIS KOUAKOU JANSENIUS'),
        (N'IIT-0394', N'N''CHO', N'N''CHACOU CHRIS-ROLAND PATRICE'),
        (N'IIT-0444', N'N''DOLI', N'MOAYE MARIE EUNICE NAOMI'),
        (N'IIT-0348', N'N''DOUA', N'KACOU PAUL ISRAEL'),
        (N'IIT-0417', N'N''GORAN', N'LAEL JEAN CHARLES'),
        (N'IIT-0339', N'N''GUETTA', N'CHRIST EMMANUEL'),
        (N'IIT-0391', N'OGOUBIYI', N'ABDOUL MIFDOL FOLAHANMI ADIO FIACRE'),
        (N'IIT-0418', N'OUATTARA', N'PEBARGNANA NOURDINE'),
        (N'IIT-0398', N'OULAYE', N'DAPLE EVARISTE'),
        (N'IIT-0378', N'PALE', N'SIE STEVEN LERICHE HERVE'),
        (N'IIT-0357', N'SAKI', N'DJOROU CHRIS ADONIS'),
        (N'IIT-0438', N'SERI', N'ZADJE ANGE MICHAEL EMERSON'),
        (N'IIT-0373', N'SEYNOU', N'FATIM'),
        (N'IIT-0371', N'SIHOULI', N'YOGNE JEAN-HOREB'),
        (N'IIT-0415', N'SILUE', N'GNENEMA ABOUBACAR KADER'),
        (N'IIT-0384', N'TAH', N'GUELASSE ARIEL YOHANN MONDESIR'),
        (N'IIT-0409', N'TAYE', N'CHRIST EMMANUEL'),
        (N'IIT-0591', N'TIESSE', N'LOU NANZIE ALLIANCE SEPHORA PRECIEUSE'),
        (N'IIT-0366', N'VRY', N'LOBA JOSUE BENJAMIN'),
        (N'IIT-0419', N'YAPO', N'BIENVENU CEDRICK BAKARY'),
        (N'IIT-0414', N'YAPO', N'SOUKOULE CHEICK OUMAR JORDAN'),
        (N'IIT-0340', N'YOBOUET', N'JEAN LUC DELOR KASSIE'),
        (N'IIT-0458', N'ZOUA', N'JEAN YVES JUNIOR');

    INSERT INTO dbo.Etudiants(Matricule, Nom, Prenom, FiliereId, Niveau, Actif)
    SELECT
        s.Matricule,
        s.Nom,
        s.Prenom,
        f.FiliereId,
        CASE s.RowNum % 3 WHEN 0 THEN N'L1' WHEN 1 THEN N'L2' ELSE N'L3' END,
        1
    FROM @Source s
    JOIN @FiliereIds f ON f.Idx = (s.RowNum % @NbFilieres)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Etudiants e WHERE e.Matricule = s.Matricule);

    INSERT INTO dbo.Inscriptions(EtudiantId, AnneeAcademique, DateInscription)
    SELECT e.EtudiantId, N'2026 - 2027', CONVERT(date, '2026-10-08')
    FROM dbo.Etudiants e
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Inscriptions i
        WHERE i.EtudiantId = e.EtudiantId AND i.AnneeAcademique = N'2026 - 2027'
    );
END;
GO
