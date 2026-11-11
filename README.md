# Gestion Université IIT

Application de bureau Windows Forms en C# pour la gestion académique de l'IIT : étudiants, inscriptions, enseignants, cours, salles, planning, présences/absences et statistiques.

## Prérequis

- Windows 10/11 et Visual Studio Community 2022 ou plus récent, avec la charge de travail **Développement .NET Desktop**.
- SQL Server Express (instance `SQLEXPRESS`) ou une instance SQL Server accessible.

## Mise en route

1. Ouvrir `GestionUniversiteIIT.sln` dans Visual Studio.
2. Dans SQL Server Management Studio, exécuter `Database/01_CreateDatabase.sql`. Le script crée les tables et les filières/salles initiales sans supprimer de données existantes.
3. Vérifier la chaîne `Universite` dans `App.config`. Par défaut, elle utilise `.\SQLEXPRESS`, l'authentification Windows et la base `GestionUniversiteIIT`.
4. Restaurer les packages NuGet et démarrer le projet.
5. À la première ouverture, saisir le nom de l'administrateur et un mot de passe d'au moins 10 caractères. Les mots de passe sont stockés sous forme de hachage PBKDF2 avec sel aléatoire.
6. Créer les enseignants, cours et étudiants, puis les inscriptions et créneaux. L'écran « Saisir les présences » enregistre une ligne par étudiant et par cours/date.

Les formulaires de gestion proposent l'ajout, la modification, la suppression et la recherche locale. Les contraintes de clés étrangères et d'unicité sont appliquées par SQL Server ; les erreurs de saisie et de base sont affichées à l'utilisateur.

## Structure

- `Data/` : connexion SQL Server et accès ADO.NET.
- `Models/` : modèles métier.
- `Services/` : authentification, catalogues, absences et statistiques.
- `Forms/` : interfaces WinForms générées en code, sans fichiers Designer.
- `Database/` : script initial idempotent.

Le logo présent dans le dossier parent du projet est inclus comme ressource de sortie sous `Resources/logo.png`.
