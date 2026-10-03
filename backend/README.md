# MonApp — fullstack .NET 9 (EF Core + Oracle) + frontend

## Structure

```
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
│       ├── Program.cs                    # Composition : DI, AutoMapper, CORS, controllers
│       ├── appsettings.json              # Chaîne de connexion, CORS, logs
│       └── Lab2.Api.http                 # Requêtes de test (VS / VS Code)
│
└── frontend/                             # Application cliente (indépendante)
```

Flux :
`Frontend <-> Controller <-> DTO <-> (Profile) <-> Service <-> Entité <-> Repository <-> DbContext <-> Oracle`

## Rôle de chaque dossier

### Racine

| Élément | Rôle |
|---|---|
| `.gitignore` | Liste ce que git ignore : fichiers compilés (`bin/`, `obj/`), dossiers d'IDE (`.vs/`, `.idea/`). |
| `backend/` | Tout le code serveur .NET. |
| `frontend/` | Application cliente. Indépendante du .NET : elle ne communique avec le backend que par HTTP/JSON et ne voit que les DTOs. |

### `backend/`

| Élément | Rôle |
|---|---|
| `Lab2.sln` | Fichier de solution : regroupe `Lab2.Infrastructure` et `Lab2.Api` pour les compiler ensemble et les ouvrir dans l'IDE. Il ne contient aucun code. |

### `Lab2.Infrastructure/` — tout ce qui touche à Oracle

Si la base de données change, seul ce projet est modifié. Il ne connaît ni l'Api, ni les DTOs, ni HTTP.

| Élément | Rôle |
|---|---|
| `DependencyInjection.cs` | Méthode `AddInfrastructure(configuration)` appelée par `Program.cs` : lit la chaîne de connexion, enregistre le `DbContext` Oracle et les repositories. |
| `Entities/` | Classes C# qui représentent les tables (`Client`, `Film`, `Location`, ...). Les annotations (`[Table]`, `[Column]`, `[Key]`, `[ForeignKey]`, ...) décrivent le mapping vers les tables et colonnes Oracle. Les entités ne sortent jamais de l'Api. |
| `Data/AppDbContext.cs` | Point d'entrée d'EF Core : un `DbSet` par table, conventions de types (`VARCHAR2`, `DATE`) et chargement des configurations. |
| `Data/Configurations/` | Une classe par entité qui complète les annotations : relations et comportements `ON DELETE` (cascade ou restrict). |
| `Repositories/` | Les requêtes EF Core (LINQ), cachées derrière des interfaces pour que les services ne touchent jamais au `DbContext`. Ils renvoient des entités, jamais des DTOs. |

### `Lab2.Api/` — tout ce qui touche au monde extérieur

Dépend de `Lab2.Infrastructure`. C'est le projet de démarrage.

| Élément | Rôle | Connaît | Ne connaît pas |
|---|---|---|---|
| `Controllers/` | Endpoints REST. Gardés minces : lisent la requête, appellent un service, renvoient un code HTTP (200, 404, ...). | Services, DTOs | DbContext, Repositories, Entités |
| `Services/` | Logique métier : orchestrent les repositories et convertissent les entités en DTOs avec AutoMapper. | Repositories, Entités, DTOs | HTTP |
| `Dtos/` | Objets échangés avec le frontend. **Sortie** : `ClientDto`, `ForfaitDto`, `AdresseDto`, `CarteCreditDto` (numéro de carte masqué, ni CVV ni mot de passe). **Entrée** : `CreerClientDto`, `CreerCarteCreditDto`, `ConnexionDto`, validés par annotations. | — | — |
| `Mapping/` | Profils AutoMapper qui décrivent la conversion Entité -> DTO (par exemple, `ClientProfile` aplatit `Utilisateur` dans `ClientDto`). | Entités, DTOs | — |

Les autres fichiers de l'Api :

| Élément | Rôle |
|---|---|
| `Program.cs` | Composition de l'application : enregistre l'Infrastructure, AutoMapper, les services, les controllers et le CORS, puis démarre le serveur. |
| `appsettings.json` | Configuration : chaîne de connexion Oracle, origines CORS autorisées (`Cors:AllowedOrigins`), niveaux de logs. |
| `Properties/launchSettings.json` | Profil de lancement local : port 5080 et environnement `Development`. |
| `Lab2.Api.http` | Requêtes HTTP prêtes à envoyer pour tester les endpoints, depuis Visual Studio ou VS Code (REST Client). |

## Lancement

Toutes les commandes `dotnet` se lancent depuis `Lab2/backend/`.

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project Lab2.Api       # http://localhost:5080
```