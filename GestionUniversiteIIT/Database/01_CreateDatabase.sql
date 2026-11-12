IF DB_ID(N'GestionUniversiteIIT') IS NULL
    CREATE DATABASE GestionUniversiteIIT;
GO

USE GestionUniversiteIIT;
GO

IF OBJECT_ID(N'dbo.Filieres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Filieres
    (
        FiliereId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Filieres PRIMARY KEY,
        Nom nvarchar(100) NOT NULL CONSTRAINT UQ_Filieres_Nom UNIQUE
    );
END;
GO

IF OBJECT_ID(N'dbo.Etudiants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Etudiants
    (
        EtudiantId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Etudiants PRIMARY KEY,
        Matricule nvarchar(30) NOT NULL CONSTRAINT UQ_Etudiants_Matricule UNIQUE,
        Nom nvarchar(100) NOT NULL,
        Prenom nvarchar(100) NOT NULL,
        FiliereId int NOT NULL,
        Niveau nvarchar(20) NOT NULL,
        DateNaissance date NULL,
        Sexe nvarchar(20) NULL,
        PhotoPath nvarchar(260) NULL,
        Actif bit NOT NULL CONSTRAINT DF_Etudiants_Actif DEFAULT 1,
        CONSTRAINT FK_Etudiants_Filieres FOREIGN KEY (FiliereId) REFERENCES dbo.Filieres(FiliereId)
    );
END;
GO

IF COL_LENGTH(N'dbo.Etudiants', N'PhotoPath') IS NULL
BEGIN
    ALTER TABLE dbo.Etudiants ADD PhotoPath nvarchar(260) NULL;
END;
GO

IF OBJECT_ID(N'dbo.Enseignants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enseignants
    (
        EnseignantId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enseignants PRIMARY KEY,
        Matricule nvarchar(30) NOT NULL CONSTRAINT UQ_Enseignants_Matricule UNIQUE,
        Nom nvarchar(100) NOT NULL,
        Prenom nvarchar(100) NOT NULL,
        Specialite nvarchar(100) NOT NULL,
        Email nvarchar(254) NULL,
        Telephone nvarchar(30) NULL,
        Actif bit NOT NULL CONSTRAINT DF_Enseignants_Actif DEFAULT 1
    );
END;
GO

-- Preserve a pre-existing legacy course table whose columns do not match this app.
-- Keep its data under a different name, then create the current app schema below.
IF OBJECT_ID(N'dbo.Cours', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Cours', N'CoursId') IS NULL
BEGIN
    IF OBJECT_ID(N'dbo.Cours_Legacy', N'U') IS NOT NULL
        THROW 50001, 'dbo.Cours_Legacy already exists; inspect both course tables before migrating.', 1;

    EXEC sys.sp_rename N'dbo.Cours', N'Cours_Legacy';
END;
GO

IF OBJECT_ID(N'dbo.Cours', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cours
    (
        CoursId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cours PRIMARY KEY,
        Code nvarchar(20) NOT NULL CONSTRAINT UQ_Cours_Code UNIQUE,
        Libelle nvarchar(150) NOT NULL,
        FiliereId int NOT NULL,
        EnseignantId int NULL,
        Credits int NOT NULL CONSTRAINT CK_Cours_Credits CHECK (Credits BETWEEN 1 AND 60),
        CONSTRAINT FK_Cours_Filieres FOREIGN KEY (FiliereId) REFERENCES dbo.Filieres(FiliereId),
        CONSTRAINT FK_Cours_Enseignants FOREIGN KEY (EnseignantId) REFERENCES dbo.Enseignants(EnseignantId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Salles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Salles
    (
        SalleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Salles PRIMARY KEY,
        Code nvarchar(30) NOT NULL CONSTRAINT UQ_Salles_Code UNIQUE,
        Capacite int NOT NULL CONSTRAINT CK_Salles_Capacite CHECK (Capacite > 0),
        [Type] nvarchar(50) NOT NULL,
        Active bit NOT NULL CONSTRAINT DF_Salles_Active DEFAULT 1
    );
END;
GO

IF OBJECT_ID(N'dbo.Inscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inscriptions
    (
        InscriptionId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inscriptions PRIMARY KEY,
        EtudiantId int NOT NULL,
        AnneeAcademique nvarchar(20) NOT NULL,
        DateInscription date NOT NULL CONSTRAINT DF_Inscriptions_Date DEFAULT CONVERT(date, GETDATE()),
        CONSTRAINT FK_Inscriptions_Etudiants FOREIGN KEY (EtudiantId) REFERENCES dbo.Etudiants(EtudiantId),
        CONSTRAINT UQ_Inscriptions_Etudiant_Annee UNIQUE (EtudiantId, AnneeAcademique)
    );
END;
GO

IF OBJECT_ID(N'dbo.Plannings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Plannings
    (
        PlanningId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Plannings PRIMARY KEY,
        CoursId int NOT NULL,
        SalleId int NOT NULL,
        JourSemaine nvarchar(20) NOT NULL,
        HeureDebut time(0) NOT NULL,
        HeureFin time(0) NOT NULL,
        FiliereId int NOT NULL,
        Niveau nvarchar(20) NOT NULL,
        CONSTRAINT CK_Plannings_Heures CHECK (HeureFin > HeureDebut),
        CONSTRAINT FK_Plannings_Cours FOREIGN KEY (CoursId) REFERENCES dbo.Cours(CoursId),
        CONSTRAINT FK_Plannings_Salles FOREIGN KEY (SalleId) REFERENCES dbo.Salles(SalleId),
        CONSTRAINT FK_Plannings_Filieres FOREIGN KEY (FiliereId) REFERENCES dbo.Filieres(FiliereId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Absences', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Absences
    (
        AbsenceId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Absences PRIMARY KEY,
        CoursId int NOT NULL,
        EtudiantId int NOT NULL,
        DateCours date NOT NULL,
        Present bit NOT NULL CONSTRAINT DF_Absences_Present DEFAULT 1,
        Justification nvarchar(500) NULL,
        CONSTRAINT FK_Absences_Cours FOREIGN KEY (CoursId) REFERENCES dbo.Cours(CoursId),
        CONSTRAINT FK_Absences_Etudiants FOREIGN KEY (EtudiantId) REFERENCES dbo.Etudiants(EtudiantId),
        CONSTRAINT UQ_Absences_Cours_Etudiant_Date UNIQUE (CoursId, EtudiantId, DateCours)
    );
END;
GO

IF OBJECT_ID(N'dbo.Utilisateurs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Utilisateurs
    (
        UtilisateurId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Utilisateurs PRIMARY KEY,
        NomUtilisateur nvarchar(100) NOT NULL CONSTRAINT UQ_Utilisateurs_Nom UNIQUE,
        SelMotDePasse varbinary(16) NOT NULL,
        HachageMotDePasse varbinary(32) NOT NULL,
        Role nvarchar(30) NOT NULL,
        Actif bit NOT NULL CONSTRAINT DF_Utilisateurs_Actif DEFAULT 1
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Filieres)
BEGIN
    INSERT dbo.Filieres(Nom) VALUES (N'Génie Logiciel'), (N'Marketing'), (N'Informatique'), (N'Réseaux');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Salles)
BEGIN
    INSERT dbo.Salles(Code,Capacite,[Type],Active) VALUES
        (N'A1',50,N'Cours',1),
        (N'A2',50,N'Cours',1),
        (N'B1',30,N'TP',1),
        (N'Lab Info',40,N'Laboratoire',0);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Enseignants)
BEGIN
    INSERT dbo.Enseignants(Matricule,Nom,Prenom,Specialite,Email,Telephone,Actif) VALUES
        (N'ENS001',N'DIA',N'Mamadou',N'Informatique',N'm.dia@iit.ci',N'07 01 02 03 04',1),
        (N'ENS002',N'KOUAME',N'Alice',N'Marketing',N'a.kouame@iit.ci',N'07 11 22 33 44',1),
        (N'ENS003',N'YAO',N'Jean',N'Réseaux',N'j.yao@iit.ci',N'07 55 66 77 88',1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cours)
BEGIN
    DECLARE @FilGL int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Génie Logiciel');
    DECLARE @FilMkt int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Marketing');
    DECLARE @FilInfo int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Informatique');
    DECLARE @Ens1 int = (SELECT TOP 1 EnseignantId FROM dbo.Enseignants WHERE Matricule = N'ENS001');
    DECLARE @Ens2 int = (SELECT TOP 1 EnseignantId FROM dbo.Enseignants WHERE Matricule = N'ENS002');
    DECLARE @Ens3 int = (SELECT TOP 1 EnseignantId FROM dbo.Enseignants WHERE Matricule = N'ENS003');

    INSERT dbo.Cours(Code,Libelle,FiliereId,EnseignantId,Credits) VALUES
        (N'GL101',N'Programmation C#',@FilGL,@Ens1,5),
        (N'GL102',N'Base de données (SQL)',@FilGL,@Ens1,4),
        (N'MT101',N'Marketing de base',@FilMkt,@Ens2,3),
        (N'INF101',N'Algorithmique',@FilInfo,@Ens1,5),
        (N'RES101',N'Réseaux',@FilGL,@Ens3,4),
        (N'GL103',N'Poo C#',@FilGL,@Ens1,4);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Etudiants)
BEGIN
    DECLARE @FGL int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Génie Logiciel');
    DECLARE @FMkt int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Marketing');
    DECLARE @FInf int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Informatique');

    INSERT dbo.Etudiants(Matricule,Nom,Prenom,FiliereId,Niveau,DateNaissance,Sexe,Actif) VALUES
        (N'IIT0001',N'KONE',N'Ibrahim',@FGL,N'L2','2004-03-14',N'M',1),
        (N'IIT0002',N'ADJA',N'Koffi',@FMkt,N'L2','2003-08-22',N'M',1),
        (N'IIT0003',N'TAYE',N'Emmanuel',@FInf,N'L1','2005-01-10',N'M',1),
        (N'IIT0004',N'ZOUA',N'Christ',@FGL,N'L2','2004-11-05',N'M',1),
        (N'IIT0005',N'OUATTARA',N'Michel',@FGL,N'L2','2004-06-19',N'M',1),
        (N'IIT0006',N'KOFFI',N'Ange',@FGL,N'L2','2004-09-27',N'F',1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Inscriptions)
BEGIN
    INSERT dbo.Inscriptions(EtudiantId,AnneeAcademique,DateInscription)
    SELECT EtudiantId, N'2026 - 2027',
           CASE WHEN Matricule IN (N'IIT0001', N'IIT0002') THEN CONVERT(date, '2026-10-05') ELSE CONVERT(date, '2026-10-06') END
    FROM dbo.Etudiants
    WHERE Matricule IN (N'IIT0001', N'IIT0002', N'IIT0003', N'IIT0004');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Plannings)
BEGIN
    DECLARE @PFilGL int = (SELECT TOP 1 FiliereId FROM dbo.Filieres WHERE Nom = N'Génie Logiciel');
    DECLARE @S_A1 int = (SELECT TOP 1 SalleId FROM dbo.Salles WHERE Code = N'A1');
    DECLARE @S_A2 int = (SELECT TOP 1 SalleId FROM dbo.Salles WHERE Code = N'A2');
    DECLARE @S_B1 int = (SELECT TOP 1 SalleId FROM dbo.Salles WHERE Code = N'B1');
    DECLARE @C_GL101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'GL101');
    DECLARE @C_GL102 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'GL102');
    DECLARE @C_INF101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'INF101');
    DECLARE @C_RES101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'RES101');
    DECLARE @C_GL103 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'GL103');

    IF @PFilGL IS NOT NULL AND @S_A1 IS NOT NULL AND @C_GL101 IS NOT NULL
    BEGIN
        INSERT dbo.Plannings(CoursId,SalleId,JourSemaine,HeureDebut,HeureFin,FiliereId,Niveau) VALUES
            (@C_GL101, @S_A1, N'Lundi',  '08:00', '10:00', @PFilGL, N'L2'),
            (@C_GL102, @S_A2, N'Mardi',  '08:00', '10:00', @PFilGL, N'L2'),
            (@C_INF101,@S_A1, N'Mardi',  '10:00', '12:00', @PFilGL, N'L2'),
            (@C_RES101,@S_B1, N'Jeudi',  '08:00', '10:00', @PFilGL, N'L2'),
            (@C_GL103, @S_A1, N'Jeudi',  '10:00', '12:00', @PFilGL, N'L2');
    END;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Absences)
BEGIN
    DECLARE @AC_GL101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'GL101');
    DECLARE @AC_GL102 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'GL102');
    DECLARE @AC_INF101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'INF101');
    DECLARE @AC_MT101 int = (SELECT TOP 1 CoursId FROM dbo.Cours WHERE Code = N'MT101');
    DECLARE @E1 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0001');
    DECLARE @E2 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0002');
    DECLARE @E3 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0003');
    DECLARE @E4 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0004');
    DECLARE @E5 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0005');
    DECLARE @E6 int = (SELECT TOP 1 EtudiantId FROM dbo.Etudiants WHERE Matricule = N'IIT0006');

    IF @AC_GL101 IS NOT NULL AND @E1 IS NOT NULL
    BEGIN
        INSERT dbo.Absences(CoursId,EtudiantId,DateCours,Present,Justification) VALUES
            (@AC_GL101, @E1, '2026-11-06', 1, NULL),
            (@AC_GL101, @E4, '2026-11-06', 0, NULL),
            (@AC_GL101, @E5, '2026-11-06', 1, NULL),
            (@AC_GL101, @E6, '2026-11-06', 0, NULL),
            (@AC_GL101, @E4, '2026-11-05', 0, NULL),
            (@AC_GL102, @E1, '2026-11-03', 0, NULL),
            (@AC_INF101,@E3, '2026-10-28', 0, NULL),
            (@AC_MT101, @E2, '2026-10-21', 0, NULL);
    END;
END;
GO
