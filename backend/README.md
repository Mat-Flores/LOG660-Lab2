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

## 5 - Test

Le seul endpoint exposé pour l'instant est `GET /api/clients/{id}` :

```powershell
curl.exe -i http://localhost:5080/api/clients/464812     # 200 + JSON si l'id existe, 404 sinon
```
