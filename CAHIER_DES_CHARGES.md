# Cahier des Charges — Gestion Université IIT

## 1. Présentation générale du projet
- **Nom du projet** : Gestion Université IIT (`GestionUniversiteIIT`)
- **Type d'application** : Application de bureau Windows Forms (.NET 8 — `net8.0-windows`)
- **Base de données** : Microsoft SQL Server (`GestionUniversiteIIT`) via `Microsoft.Data.SqlClient`
- **Objectif** : Centraliser et automatiser la gestion académique et administrative de l'Institut International de Technologie (IIT) : authentification sécurisée, gestion des étudiants, inscriptions, enseignants, cours, salles, emploi du temps (avec détection de conflits), suivi des présences/absences et tableaux de bord statistiques.

---

## 2. Périmètre fonctionnel

### 2.1 Authentification & Sécurité (`FrmConnexion`, `AuthService`)
- Connexion par nom d'utilisateur et mot de passe.
- Création guidée du premier administrateur si la table `dbo.Utilisateurs` est vide.
- Réinitialisation de mot de passe depuis l'écran de connexion.
- Règles de sécurité :
  - Mots de passe d'au moins **10 caractères**.
  - Hachage **PBKDF2-SHA256** avec **210 000 itérations** et sel aléatoire de 16 octets (`SelMotDePasse`, `HachageMotDePasse`).
  - Comparaison à temps constant (`CryptographicOperations.FixedTimeEquals`).
  - Gestion des rôles (`Administrateur`, `Scolarité`).

### 2.2 Tableau de bord & Navigation (`FrmAccueil`)
- Barre latérale de navigation regroupant les 12 modules :
  `Accueil`, `Étudiants`, `Inscriptions`, `Enseignants`, `Cours`, `Planification`, `Salles`, `Absences`, `Historique`, `Rapports`, `Paramètres`, `Déconnexion`.
- **Indicateurs clés (KPI)** en temps réel : nombre d'étudiants actifs, d'inscriptions, de cours et d'enseignants actifs.
- **Accès rapides** vers les actions fréquentes (nouvelle inscription, saisie d'absences, consultation du planning, etc.).
- **Signal visuel permanent de connexion SQL Server** dans la barre supérieure (`SQL Connecté` en vert / `SQL Déconnecté` en rouge / `Vérification...` en orange) avec rafraîchissement automatique toutes les 15 secondes.

### 2.3 Gestion des Étudiants & Inscriptions (`FrmEtudiants`, `FrmInscriptions`, `FrmAjouterInscription`)
- Liste paginée avec recherche instantanée par matricule, nom, prénom, filière ou niveau.
- Création d'une nouvelle inscription avec :
  - Matricule, Nom (*), Prénoms (*), Date de naissance, Sexe.
  - Filière (*), Niveau (* : `L1`, `L2`, `L3`), Année académique (*).
  - Sélection d'une photo étudiant (`PhotoPath`).
  - Impression / export d'un reçu d'inscription.
- Modification et suppression des fiches avec contrôle d'intégrité SQL (unicité du matricule, unicité de l'inscription par étudiant et année académique).
- Filtrage des inscriptions par année académique et export CSV/impression.

### 2.4 Gestion des Enseignants, Cours et Salles (`FrmEnseignants`, `FrmCours`, `FrmSalles`, `FrmCatalogue`, `FrmEditionCatalogue`)
- **Enseignants** : matricule unique, nom, prénom, spécialité, email, téléphone, statut actif/inactif.
- **Cours** : code unique, libellé, filière rattachée, enseignant responsable, crédits (contrainte `BETWEEN 1 AND 60`).
- **Salles** : code unique, capacité (`> 0`), type (`Cours`, `TP`, `Laboratoire`, `Amphithéâtre`), état actif/inactif.
- CRUD générique piloté par `CatalogueService` (`DefinitionCatalogue`) avec recherche, pagination et badges de statut.

### 2.5 Planification des Cours (`FrmPlanning`, `PlanningService`)
- Vue matricielle hebdomadaire (`Heure | Lundi | Mardi | Mercredi | Jeudi | Vendredi`).
- Filtrage par semaine et par filière.
- Ajout / édition de créneaux avec **vérification automatique des conflits** (`PlanningService.VerifierConflits`) :
  1. Cohérence horaire (`HeureFin > HeureDebut`).
  2. Conflit de salle (même salle occupée au même jour sur un créneau qui se chevauche).
  3. Conflit de groupe (même filière et même niveau déjà en cours sur ce créneau).
  4. Conflit d'enseignant (même enseignant affecté simultanément à un autre cours).
- Export de l'emploi du temps.

### 2.6 Gestion & Historique des Absences (`FrmSaisieAbsences`, `FrmHistoriqueAbsences`, `AbsenceService`)
- **Saisie des présences/absences** : sélection du cours, de la date, de la filière et du niveau ; cochage en lot des étudiants présents/absents et enregistrement en base (`MERGE` / `INSERT-UPDATE` sur `(CoursId, EtudiantId, DateCours)`).
- **Historique des absences** : filtrage par étudiant et par plage de dates, affichage des statuts par badges, impression et export CSV.

### 2.7 Statistiques & Rapports (`FrmStatistiques`, `StatistiqueService`)
- 4 cartes KPI (Étudiants, Inscriptions, Cours, Enseignants).
- Graphique camembert : **Taux de présence vs Taux d'absence**.
- Histogramme : **Répartition des inscriptions par niveau** (`L1`, `L2`, `L3`).

### 2.8 Paramètres & Administration (`FrmParametres`)
- **Base de données & Connexion** :
  - Paramétrage interactif : Serveur SQL, Base de données, Mode d'authentification (Windows ou SQL Server avec identifiant/mot de passe), options SSL (`Encrypt`, `TrustServerCertificate`).
  - Test de connexion en direct (statut, version SQL Server, latence en ms).
  - Sauvegarde et application à chaud dans `App.config` et la configuration d'exécution sans redémarrage.
  - Tableau en direct du nombre de lignes par table SQL Server.
- **Gestion des Filières** : ajout et suppression de filières (`dbo.Filieres`).
- **Comptes & Sécurité** : modification du mot de passe personnel et création de nouveaux comptes utilisateurs.

---

## 3. Architecture technique & Base de données

### 3.1 Architecture logicielle
- **Langage & Framework** : C# 12 / .NET 8 (`net8.0-windows`, WinForms, `Nullable` activé).
- **Accès aux données** : ADO.NET pur via `Microsoft.Data.SqlClient` (requêtes paramétrées contre les injections SQL).
- **Organisation en couches** :
  - `Data/` : connexion, construction dynamique de chaîne, exécution SQL et gestion du statut.
  - `Models/` : `record` immuables représentant les entités métier.
  - `Services/` : logique métier, validation, chiffrement et requêtes SQL.
  - `Forms/` : interfaces graphiques construites par code et compatibles avec le concepteur Visual Studio.
  - `Database/` : script SQL idempotent `01_CreateDatabase.sql`.

### 3.2 Modèle de données SQL Server (`GestionUniversiteIIT`)
| Table | Clé primaire | Contraintes principales |
|---|---|---|
| `dbo.Filieres` | `FiliereId` | `Nom` unique |
| `dbo.Etudiants` | `EtudiantId` | `Matricule` unique, FK `FiliereId -> Filieres` |
| `dbo.Enseignants` | `EnseignantId` | `Matricule` unique |
| `dbo.Cours` | `CoursId` | `Code` unique, `Credits` entre 1 et 60, FK `FiliereId`, `EnseignantId` |
| `dbo.Salles` | `SalleId` | `Code` unique, `Capacite > 0` |
| `dbo.Inscriptions` | `InscriptionId` | FK `EtudiantId`, couple `(EtudiantId, AnneeAcademique)` unique |
| `dbo.Plannings` | `PlanningId` | `HeureFin > HeureDebut`, FK `CoursId`, `SalleId`, `FiliereId` |
| `dbo.Absences` | `AbsenceId` | FK `CoursId`, `EtudiantId`, triplet `(CoursId, EtudiantId, DateCours)` unique |
| `dbo.Utilisateurs` | `UtilisateurId` | `NomUtilisateur` unique, sel 16 octets, hash 32 octets |

---

## 4. Contraintes & Qualité
- **Tolérance aux pannes** : si SQL Server est momentanément indisponible, `DbHelper` applique un délai de réessai (`8 secondes`) et les services disposent de données de secours en mémoire pour éviter le blocage de l'interface.
- **Compatibilité DPI & Concepteur Visual Studio** : les formulaires séparent l'initialisation standard (`InitializeComponent()` dans `.Designer.cs`) de la construction dynamique (`ConstruireInterface()` dans `.cs`).
- **Configuration** : `App.config` stocke la chaîne `Universite` modifiable à tout moment depuis l'interface d'administration.
