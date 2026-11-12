# Documentation Technique du Code — Gestion Université IIT

Ce document explique l'ensemble de la structure et du code source du projet **`GestionUniversiteIIT`** situé dans `C:\Users\HP VICTUS\Documents\Projet_gestion_IIT\GestionUniversiteIIT`.

---

## 1. Point d'entrée & Configuration

### `Program.cs`
- Point d'entrée `[STAThread] Main()`.
- Initialise la configuration WinForms (`ApplicationConfiguration.Initialize()`) et lance le formulaire de connexion `new Forms.FrmConnexion()`.

### `App.config` & `GestionUniversiteIIT.csproj`
- **`App.config`** : contient la section `<connectionStrings>` avec l'entrée `Universite` (`Microsoft.Data.SqlClient`) pointant vers la base `GestionUniversiteIIT`.
- **`GestionUniversiteIIT.csproj`** : cible `net8.0-windows` avec WinForms, `Nullable` et `ImplicitUsings` activés. Référence `Microsoft.Data.SqlClient` (v6.0.2) et `System.Configuration.ConfigurationManager` (v8.0.1), et embarque `../logo.png` sous `Resources/logo.png`.

---

## 2. Couche Accès aux Données (`Data/`)

### `Data/Connexion.cs`
Classe statique gérant la chaîne de connexion SQL Server et le signal d'état global :
- **`ConnectionString`** : lit la chaîne `Universite` dans `App.config` ou utilise un override en mémoire lors d'une modification à chaud.
- **`Create()` / `Create(string)`** : construit un `SqlConnection` avec un `ConnectTimeout` minimum de 10 secondes afin d'éviter les expirations lors de la négociation SSL.
- **`ConstruireChaine(...)`** : assemble une chaîne de connexion via `SqlConnectionStringBuilder` (serveur, base, authentification Windows ou SQL, `Encrypt`, `TrustServerCertificate`, timeout).
- **`EstConnecte`**, **`DernierMessageStatut`**, **`StatusChanged`** : maintiennent et diffusent en temps réel l'état de connexion SQL Server vers les écrans (`FrmAccueil`, `FrmConnexion`).
- **`TesterConnexion(string?)`** : ouvre une connexion, mesure la latence en millisecondes (`Stopwatch`), récupère `ServerVersion` et déclenche `NotifierStatut(...)`.
- **`SauvegarderEtAppliquer(...)`** : applique la nouvelle chaîne en mémoire et la persiste à la fois dans le `.dll.config` d'exécution (`ConfigurationManager.OpenExeConfiguration`) et dans le `App.config` source.

### `Data/DbHelper.cs`
Classe statique centralisant les opérations ADO.NET :
- **`Query(sql, parameters)`** : exécute un `SELECT` via `SqlDataAdapter` et retourne un `DataTable`.
- **`Execute(sql, parameters)`** : exécute un `INSERT` / `UPDATE` / `DELETE` (`ExecuteNonQuery`).
- **`Scalar(sql, parameters)`** : retourne une valeur scalaire (`ExecuteScalar`), en convertissant `DBNull` en `null`.
- **`WithConnectionRetry<T>`** : enveloppe chaque appel SQL :
  - En cas de succès : réinitialise le délai d'attente et appelle `Connexion.NotifierStatut(true, ...)`.
  - En cas d'erreur réseau/SQL (`IsConnectionFailure` : codes `-2, 2, 17, 26, 40, 53, 64, 233, 258, 4060, 10053, 10054, 10060, 11001`) : active une pause de 8 secondes (`RetryDelay`) avant la prochaine tentative et appelle `Connexion.NotifierStatut(false, ...)`.
- **`ReinitialiserDelaiReessai()`** : annule immédiatement la pause de 8 secondes après un changement de paramètres de connexion.
- **`AddParameters(...)`** : ajoute les paramètres SQL de manière sécurisée (`DBNull.Value` si `null`).

---

## 3. Modèles Métier (`Models/`)
Tous les modèles sont des `public sealed record` immuables reflétant les tables SQL :
- **`Utilisateur.cs`** : `Utilisateur(int UtilisateurId, string NomUtilisateur, string Role)`.
- **`Etudiant.cs`** : `Etudiant(...)` (identifiant, matricule, nom, prénom, filière, niveau, date de naissance, sexe, photo, actif).
- **`Enseignant.cs`** : `Enseignant(...)` (matricule, nom, prénom, spécialité, email, téléphone, actif).
- **`Cours.cs`** : `Cours(...)` (code, libellé, filière, enseignant, crédits).
- **`Salle.cs`** : `Salle(...)` (code, capacité, type, active).
- **`Inscription.cs`** : `Inscription(...)` (étudiant, année académique, date d'inscription).
- **`Planning.cs`** : `Planning(...)` (cours, salle, jour, heure début/fin, filière, niveau).
- **`Absence.cs`** : `Absence(...)` (cours, étudiant, date du cours, présent, justification).

---

## 4. Couche Services (`Services/`)

### `Services/AuthService.cs`
- **`HasUsers()`** : vérifie si `dbo.Utilisateurs` contient au moins un compte (avec repli sur `UtilisateursSecours` en mémoire).
- **`Authenticate(username, password)`** : récupère le sel (16 octets) et le hash (32 octets) depuis `dbo.Utilisateurs` (`Actif = 1`), dérive le mot de passe saisi via `Rfc2898DeriveBytes.Pbkdf2` (SHA-256, 210 000 itérations) et compare via `CryptographicOperations.FixedTimeEquals`.
- **`CreateInitialAdministrator(username, password)`** : crée le premier administrateur si aucun utilisateur n'existe.
- **`ChangerOuCreerMotDePasse(username, newPassword, role)`** : impose un minimum de 10 caractères, génère un nouveau sel aléatoire, met à jour ou insère l'utilisateur dans `dbo.Utilisateurs` et dans le cache de secours.

### `Services/CatalogueService.cs`
Moteur générique de CRUD pour les écrans de type catalogue (`Etudiants`, `Inscriptions`, `Enseignants`, `Cours`, `Planning`, `Salles`) :
- **`TypeChamp`**, **`ChampCatalogue`**, **`DefinitionCatalogue`** : décrivent les colonnes, requêtes SQL (`SELECT`, `INSERT`, `UPDATE`, `DELETE`), libellés et champs de saisie de chaque écran.
- **`GetDefinition(cle)`** : retourne la configuration SQL et UI complète de l'entité demandée.
- **`Lister` / `Lire` / `ChargerListe` / `Enregistrer` / `Supprimer`** : exécutent les opérations SQL correspondantes (avec vérification des conflits via `PlanningService.VerifierConflits` pour le planning) et gèrent des données de secours en mémoire en cas de déconnexion SQL.
- **`ListerFilieres` / `AjouterFiliere` / `SupprimerFiliere`** : gèrent la table `dbo.Filieres`.

### `Services/PlanningService.cs`
- **`Lister()`** : charge les créneaux avec jointures sur `Cours`, `Filieres` et `Salles`.
- **`VerifierConflits(...)`** : vérifie que `heureFin > heureDebut`, puis contrôle les chevauchements horaires sur le même jour pour :
  - la **salle** (`SalleId`),
  - le **groupe** (`FiliereId` + `Niveau`),
  - l'**enseignant** (`EnseignantId` du cours).
  En cas de conflit, lève une `InvalidOperationException` avec un message détaillé.

### `Services/AbsenceService.cs`
- **`Historique(debut, fin, etudiantFiltre)`** : retourne les absences (`Present = 0`) sur la période et le filtre étudiant.
- **`EtudiantsDuCours(coursId, date, filiere, niveau)`** : charge la liste des étudiants concernés et leur état de présence pour un cours et une date donnés.
- **`EnregistrerPresence(coursId, date, etudiants, libelleCours)`** : insère ou met à jour chaque ligne dans `dbo.Absences`.

### `Services/StatistiqueService.cs`
- **`Statistiques`** (`record`) : contient les compteurs globaux et calcule `TauxPresence` et `TauxAbsence`.
- **`Charger()`** : exécute une requête SQL agrégée sur `Etudiants`, `Inscriptions`, `Cours`, `Enseignants`, `Absences` et la répartition `L1` / `L2` / `L3`. (Si la base est hors-ligne ou ne contient que le jeu initial minimal `<= 10`, retourne des valeurs de référence.)

### Services de lecture dédiés
- **`EtudiantService.cs`**, **`InscriptionService.cs`**, **`EnseignantService.cs`**, **`CoursService.cs`**, **`SalleService.cs`** : fournissent des méthodes `Lister(...)` ciblées avec filtrage SQL (`LIKE`).

---

## 5. Couche Interface Graphique (`Forms/`)

### Architecture WinForms & Compatibilité Concepteur
Chaque formulaire est séparé en deux fichiers partiels :
- **`*.Designer.cs`** : ne contient que les déclarations de champs et les propriétés simples dans `InitializeComponent()`, afin de rester 100 % compatible avec le concepteur visuel de Visual Studio.
- **`*.cs`** : contient `ConstruireInterface()` (construction dynamique des `TableLayoutPanel`, boutons, grilles, graphiques) et toute la logique événementielle.

### Charte graphique (`Forms/UiStyle.cs`)
- Définit la palette IIT (`Rouge`, `RougeFonce`, `Bleu`, `Vert`, `Orange`, `Ardoise`, `Fond`, `Texte`, couleurs de badges).
- **`IconeMenu`** : icônes vectorielles dessinées en GDI+ (`DessinerIconeMenu`).
- Composants réutilisables : `Bouton`, `BoutonArdoise`, `BoutonBleuExport`, `BoutonMaquette`, `EntetePage`, `ChampRecherche`, `EncadrerChamp`, `CarteArrondie`, `ConfigurerGrille`, `ActiverActionsGrille`, `CreerPiedPagination`, `PaginerVue`, `ImprimerTable`.

### Écrans principaux
1. **`FrmConnexion`** : authentification, création du premier admin si la base est vide, réinitialisation du mot de passe, pastille de signal SQL Server dans la barre supérieure (vérification asynchrone sur `Shown`).
2. **`FrmAccueil`** : fenêtre principale (menu latéral + barre supérieure bordeaux avec signal SQL Server en direct rafraîchi toutes les 15 secondes + zone de contenu dynamique), tableau de bord d'accueil avec KPI et raccourcis.
3. **`FrmEtudiants`**, **`FrmEnseignants`**, **`FrmCours`**, **`FrmSalles`** : héritent de **`FrmCatalogue`** en lui passant la `DefinitionCatalogue` correspondante.
4. **`FrmInscriptions`** : liste des inscriptions filtrable par année académique et exportable.
5. **`FrmAjouterInscription`** / **`FrmAjouterEtudiant`** : formulaire complet d'inscription avec sélection de photo et impression de reçu.
6. **`FrmEditionCatalogue`** : boîte de dialogue modale générée dynamiquement selon les `ChampCatalogue` de l'entité éditée.
7. **`FrmPlanning`** : grille hebdomadaire matricielle par filière et semaine, création de créneaux avec contrôle de conflits et export.
8. **`FrmSaisieAbsences`** : feuille d'appel filtrée par cours, date, filière et niveau.
9. **`FrmHistoriqueAbsences`** : consultation filtrée par étudiant et plage de dates, avec export/impression.
10. **`FrmStatistiques`** : 4 cartes KPI, graphique camembert (`PanneauGraphiqueCamembert`) et histogramme par niveau (`PanneauGraphiqueBarres`).
11. **`FrmParametres`** : onglet *Base de données & Connexion* (paramètres SQL Server éditables, test avec latence, sauvegarde à chaud dans `App.config`, état en direct des 9 tables), onglet *Gestion des Filières*, et onglet *Comptes & Sécurité*.

---

## 6. Script SQL (`Database/01_CreateDatabase.sql`)
- Script idempotent (`IF DB_ID(...) IS NULL`, `IF OBJECT_ID(...) IS NULL`) créant la base `GestionUniversiteIIT`, les 9 tables avec leurs clés primaires, clés étrangères et contraintes d'unicité/vérification, ainsi que le jeu de données initial.
