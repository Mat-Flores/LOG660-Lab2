# Lab2 - Backend

## 1 - Structure

```txt
Lab2/
├── .gitignore                            # Fichiers exclus de git (bin/, obj/, .vs/, ...)
├── backend/
│   ├── Lab2.sln                          # Solution .NET : regroupe les deux projets
│   │
│   ├── Lab2.Infrastructure/              # Accès aux données (aucune dépendance vers Api)
│   │   ├── DependencyInjection.cs        # AddInfrastructure(): DbContext Oracle + repositories
│   │   ├── Entities/                     # 17 entités, 1 fichier par table, annotées
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs           # Point d'entrée EF Core (DbSet = tables)
│   │   │   └── Configurations/           # 1 fichier par entité : relations, ON DELETE
│   │   └── Repositories/
│   │       ├── Interfaces/ 
│   │       │   └── IClientRepository.cs
│   │       └── ClientRepository.cs       # Exemple: Requêtes EF Core sur CLIENT
│   │
│   └── Lab2.Api/                         # Exposition HTTP (dépend de Infrastructure)
│       ├── Controllers/
│       │   └── ClientsController.cs      # GET api/clients/{id}
│       ├── Services/
│       │   ├── IClientService.cs         # Contrat : logique métier des clients
│       │   └── ClientService.cs          # Repository -> Entité -> DTO
│       ├── Dtos/                         # 7 DTOs, 1 fichier par classe
│       ├── Mapping/
│       │   └── ClientProfile.cs          # Profil AutoMapper (Entité -> DTO)
│       ├── Properties/
│       │   └── launchSettings.json       # Port 5080, environnement Development
│       ├── Program.cs                    # Composition : DI, AutoMapper, services, controllers
│       ├── appsettings.json              # Chaîne de connexion Oracle
│       └── Lab2.Api.http                 # Requêtes de test (VS / VS Code)
│
└── frontend/                             # Application cliente (indépendante)
```

Flux :
`Frontend <-> Controller <-> DTO <-> (Profile) <-> Service <-> Entité <-> Repository <-> DbContext <-> Oracle`

## 2 - Rôle de chaque dossier

### Racine

| Élément | Rôle |
| --- | --- |
| `.gitignore` | Liste ce que git ignore : fichiers compilés (`bin/`, `obj/`), dossiers d'IDE (`.vs/`, `.idea/`). |
| `backend/` | Tout le code serveur .NET. |
| `frontend/` | Application cliente. Indépendante du .NET : elle ne communique avec le backend que par HTTP/JSON et ne voit que les DTOs. |

### `backend/`

| Élément | Rôle |
| --- | --- |
| `Lab2.sln` | Fichier de solution : regroupe `Lab2.Infrastructure` et `Lab2.Api` pour les compiler ensemble et les ouvrir dans l'IDE. Il ne contient aucun code. |

### `Lab2.Infrastructure/` — tout ce qui touche à Oracle

Si la base de données change, seul ce projet est modifié. Il ne connaît ni l'Api, ni les DTOs, ni HTTP.

| Élément | Rôle |
| --- | --- |
| `DependencyInjection.cs` | Méthode `AddInfrastructure(configuration)` appelée par `Program.cs` : lit la chaîne de connexion, enregistre le `DbContext` Oracle et les repositories. |
| `Entities/` | Classes C# qui représentent les tables (`Client`, `Film`, `Location`, ...). Les annotations (`[Table]`, `[Column]`, `[Key]`, `[ForeignKey]`, ...) décrivent le mapping vers les tables et colonnes Oracle. Les entités ne sortent jamais de l'Api. |
| `Data/AppDbContext.cs` | Point d'entrée d'EF Core : un `DbSet` par table, conventions de types (`VARCHAR2`, `DATE`) et chargement des configurations. |
| `Data/Configurations/` | Une classe par entité qui complète les annotations : relations et comportements `ON DELETE` (cascade ou restrict). |
| `Repositories/` | Les requêtes EF Core (LINQ), cachées derrière des interfaces pour que les services ne touchent jamais au `DbContext`. Ils renvoient des entités, jamais des DTOs. |

### `Lab2.Api/` — tout ce qui touche au monde extérieur

Dépend de `Lab2.Infrastructure`. C'est le projet de démarrage.

| Élément | Rôle | Connaît | Ne connaît pas |
| --- | --- | --- | --- |
| `Controllers/` | Endpoints REST. Gardés minces : lisent la requête, appellent un service, renvoient un code HTTP (200, 404, ...). | Services, DTOs | DbContext, Repositories, Entités |
| `Services/` | Logique métier : orchestrent les repositories et convertissent les entités en DTOs avec AutoMapper. | Repositories, Entités, DTOs | HTTP |
| `Dtos/` | Objets échangés avec le frontend. **Sortie** : `ClientDto`, `ForfaitDto`, `AdresseDto`, `CarteCreditDto` (numéro de carte masqué, ni CVV ni mot de passe). | — | — |
| `Mapping/` | Profils AutoMapper qui décrivent la conversion Entité -> DTO (par exemple, `ClientProfile` aplatit `Utilisateur` dans `ClientDto`). | Entités, DTOs | — |

Les autres fichiers de l'Api :

| Élément | Rôle |
| --- | --- |
| `Program.cs` | Composition de l'application : enregistre l'Infrastructure, AutoMapper, les services et les controllers, puis démarre le serveur. |
| `appsettings.json` | Configuration : chaîne de connexion Oracle (`ConnectionStrings:Default`). |
| `Properties/launchSettings.json` | Profil de lancement local : port 5080 et environnement `Development`. |

## 3 - Installation

### Prérequis

| Outil | Version | Rôle |
| --- | --- | --- |
| SDK .NET | 9.0.x | Compile et lance le backend (le projet cible `net9.0`). |

Aucun client Oracle à installer : le pilote est fourni par les paquets NuGet, que `dotnet restore` télécharge automatiquement (connexion Internet requise au premier lancement).

### Installer le SDK .NET 9

Sous Windows (PowerShell) :

```powershell
winget install Microsoft.DotNet.SDK.9
```

Sans `winget`, ou sous un autre système : télécharger le **SDK** 9.0 (et non seulement un « Runtime ») sur <https://dotnet.microsoft.com/download/dotnet/9.0>.

Le SDK inclut déjà le runtime : rien d'autre à installer.

### Vérifier l'installation

Ouvrir un **nouveau** terminal (pour que le `PATH` soit à jour), puis :

```powershell
dotnet --list-sdks
```

Une ligne commençant par `9.0.` doit apparaître.

> Un SDK plus récent seul (par exemple 10.x) ne suffit pas pour **lancer** le projet : l'application cible .NET 9 et a besoin du runtime 9. Il faut installer le SDK 9 en plus ; les deux versions cohabitent sans problème.

### IDE (facultatif)

Visual Studio 2022 (17.12 ou plus récent), VS Code avec l'extension C# Dev Kit, ou Rider. Le projet se lance aussi sans IDE, en ligne de commande (voir ci-dessous).

## 4 - Lancement

Toutes les commandes `dotnet` se lancent depuis `Lab2/backend/`.

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project Lab2.Api       # écoute sur http://localhost:5080
```

## 5 - Connexion et test

La connexion utilise les utilisateurs et les mots de passe existants du labo 1. Le service
`AuthentificationService` vérifie les identifiants, puis le contrôleur crée une session par cookie.
Le mot de passe n'est jamais renvoyé dans la réponse. La session expire après 30 minutes
d'inactivité et peut être renouvelée lorsque l'utilisateur continue à utiliser l'API.

| Route | Résultat |
| --- | --- |
| `POST /api/auth/connexion` | `200` avec l'identité de l'utilisateur et un cookie de session; `401` si les identifiants sont incorrects; `400` si les champs sont absents ou trop longs. |
| `GET /api/clients/{id}` | `200` pour son propre profil; `401` sans connexion; `403` pour le profil d'un autre utilisateur; `404` si le profil client n'existe pas. |
| `POST /api/auth/deconnexion` | `204` et suppression du cookie; `401` sans connexion. |

Depuis PowerShell, remplacer les identifiants par ceux d'un utilisateur existant :

```powershell
$identifiants = @{ courriel = "client@exemple.com"; motDePasse = "motDePasseExistant" } | ConvertTo-Json

$utilisateur = Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/auth/connexion `
    -ContentType "application/json" -Body $identifiants -SessionVariable session

Invoke-RestMethod -Uri "http://localhost:5080/api/clients/$($utilisateur.idUtilisateur)" -WebSession $session

Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/auth/deconnexion -WebSession $session
```

Les tests automatisés utilisent des données fictives, sans connexion à Oracle :

```powershell
dotnet test Lab2.sln
```

## 6 - Consultation du catalogue

Ces routes nécessitent une connexion. Les contrôleurs passent par `CatalogueService`,
qui regroupe la recherche et la consultation des films et des personnes.

| Route | Résultat |
| --- | --- |
| `GET /api/films` | Liste des films : identifiant, titre, année et affiche, triée par titre. |
| `GET /api/films/{id}` | Fiche du film avec réalisateur, acteurs et personnages, scénaristes, genres, pays et bandes-annonces. |
| `GET /api/personnes/{id}` | Nom, naissance, photo et biographie d'une personne. |

La recherche accepte les paramètres facultatifs `titre`, `acteur`, `realisateur`, `genre`,
`pays`, `langue`, `anneeMin` et `anneeMax`. Les filtres se combinent : un film doit tous
les respecter. Les bornes des années sont inclusives.

Pour le titre, l'acteur et le réalisateur, chaque mot peut être partiel et apparaître dans
n'importe quel ordre. La recherche ignore les majuscules. Par exemple, `titre=wars star`
retrouve « Star Wars ». Pour un acteur ou un réalisateur, les mots doivent appartenir
au nom d'une même personne. Le genre, le pays et la langue correspondent à une valeur
complète, sans tenir compte des majuscules.

Après la connexion PowerShell de la section précédente, avant la déconnexion :

```powershell
Invoke-RestMethod -Uri "http://localhost:5080/api/films?titre=wars%20star&anneeMin=1970&anneeMax=2000" -WebSession $session

$films = Invoke-RestMethod -Uri "http://localhost:5080/api/films?genre=action" -WebSession $session
if ($films.Count -gt 0) {
    $film = Invoke-RestMethod -Uri "http://localhost:5080/api/films/$($films[0].idFilm)" -WebSession $session
    if ($null -ne $film.realisateur) {
        Invoke-RestMethod -Uri "http://localhost:5080/api/personnes/$($film.realisateur.idPersonne)" -WebSession $session
    }
}
```

Une recherche sans résultat retourne `200` avec une liste vide. Une fiche introuvable
retourne `404`, un filtre invalide retourne `400` et l'absence de connexion retourne `401`.

La fiche utilise les [requêtes séparées EF Core](https://learn.microsoft.com/ef/core/querying/single-split-queries)
pour charger ses collections sans multiplier les lignes entre les acteurs, genres et pays.
Les tests du catalogue exercent les routes, services, mappages et requêtes EF Core avec
une base SQLite en mémoire. Ils ne se connectent pas à la base Oracle du labo 1.
