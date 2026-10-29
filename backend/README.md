# Lab2 - Backend

## 1 - Structure

```txt
Lab2/
├── backend/
│   ├── .gitignore                        # Fichiers exclus de git (bin/, obj/, .vs/, ...)
│   ├── Lab2.sln                          # Solution .NET : regroupe les deux projets
│   ├── README.md                         # Ce fichier
│   │
│   ├── Lab2.Infrastructure/              # Accès aux données (aucune dépendance vers Api)
│   │   ├── DependencyInjection.cs        # AddInfrastructure(): DbContext Oracle (compatibilité 21c) + repositories
│   │   ├── Entities/                     # 17 entités, 1 fichier par table, annotées
│   │   ├── Exceptions/
│   │   │   └── RegleAffairesException.cs # Règle d'affaires violée (quota, inventaire, ...)
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs           # Point d'entrée EF Core (DbSet = tables)
│   │   │   └── Configurations/           # 1 fichier par entité : relations, ON DELETE
│   │   └── Repositories/
│   │       ├── Interfaces/               # ICatalogueRepository, IClientRepository, ILocationRepository
│   │       ├── ClientRepository.cs       # Client + utilisateur, adresse, forfait, carte (cas 2)
│   │       ├── CatalogueRepository.cs    # Recherche de films, fiche, personnes (cas 3)
│   │       ├── LocationRepository.cs     # Copies disponibles, quota, insertion (cas 4)
│   │       └── RechercheFilmCriteres.cs  # Critères de recherche du cas 3
│   │
│   └── Lab2.Api/                         # Exposition HTTP (dépend de Infrastructure)
│       ├── Controllers/                  # Auth, Clients, Films, Personnes, Locations
│       ├── Services/                     # Façade : ClientService, CatalogueService, LocationService
│       ├── Dtos/                         # 1 fichier par classe
│       ├── Mapping/
│       │   ├── ClientProfile.cs          # Profil AutoMapper (Entité -> DTO) des clients
│       │   └── CatalogueProfile.cs       # Profil AutoMapper des films et personnes
│       ├── Properties/
│       │   └── launchSettings.json       # Port 5080, environnement Development
│       ├── Program.cs                    # Composition : DI, AutoMapper, services, controllers
│       └── appsettings.json              # Chaîne de connexion Oracle
│
└── frontend/                             # Application cliente (indépendante)
```

Flux :
`Frontend <-> Controller <-> DTO <-> (Profile) <-> Service <-> Entité <-> Repository <-> DbContext <-> Oracle`

## 2 - Rôle de chaque dossier

### Racine

| Élément | Rôle |
| --- | --- |
| `backend/` | Tout le code serveur .NET. |
| `frontend/` | Application cliente. Indépendante du .NET : elle ne communique avec le backend que par HTTP/JSON et ne voit que les DTOs. |

### `backend/`

| Élément | Rôle |
| --- | --- |
| `.gitignore` | Liste ce que git ignore : fichiers compilés (`bin/`, `obj/`), dossiers d'IDE (`.vs/`, `.idea/`). |
| `Lab2.sln` | Fichier de solution : regroupe `Lab2.Infrastructure` et `Lab2.Api` pour les compiler ensemble et les ouvrir dans l'IDE. Il ne contient aucun code. |

### `Lab2.Infrastructure/` — tout ce qui touche à Oracle

Si la base de données change, seul ce projet est modifié. Il ne connaît ni l'Api, ni les DTOs, ni HTTP.

| Élément | Rôle |
| --- | --- |
| `DependencyInjection.cs` | Méthode `AddInfrastructure(configuration)` appelée par `Program.cs` : lit la chaîne de connexion, enregistre le `DbContext` Oracle et les repositories. Le serveur du cours est en Oracle 21c : `UseOracleSQLCompatibility(DatabaseVersion21)` empêche EF de générer du SQL 23ai (`TRUE`/`FALSE`), refusé avec `ORA-00904`. |
| `Entities/` | Classes C# qui représentent les tables (`Client`, `Film`, `Location`, ...). Les annotations (`[Table]`, `[Column]`, `[Key]`, `[ForeignKey]`, ...) décrivent le mapping vers les tables et colonnes Oracle. Les entités ne sortent jamais de l'Api. Elles suivent la BD réelle : les champs absents des XML du labo 1 sont nullables (`Film` : réalisateur, langue, affiche ; `Personne` : date et lieu de naissance, photo, biographie). |
| `Exceptions/` | `RegleAffairesException` : règle d'affaires violée (film inexistant, quota du forfait, copie indisponible), avec le code HTTP à renvoyer (404 ou 409). |
| `Data/AppDbContext.cs` | Point d'entrée d'EF Core : un `DbSet` par table, conventions de types (`VARCHAR2`, `DATE`) et chargement des configurations. |
| `Data/Configurations/` | Une classe par entité qui complète les annotations : relations et comportements `ON DELETE` (cascade ou restrict). |
| `Repositories/` | Les requêtes EF Core (LINQ), cachées derrière des interfaces pour que les services ne touchent jamais au `DbContext`. Ils renvoient des entités, jamais des DTOs. `RechercheFilmCriteres` regroupe les critères du cas 3 ; le controller le lie directement à la query string. |

### `Lab2.Api/` — tout ce qui touche au monde extérieur

Dépend de `Lab2.Infrastructure`. C'est le projet de démarrage.

| Élément | Rôle | Connaît | Ne connaît pas |
| --- | --- | --- | --- |
| `Controllers/` | Endpoints REST. Gardés minces : lisent la requête, appellent un service, renvoient un code HTTP (200, 201, 400, 401, 404, 409). Tous sauf `AuthController` sont `[Authorize]`. | Services, DTOs, `RechercheFilmCriteres` | DbContext, Repositories, Entités |
| `Services/` | **Façade** (patron de l'énoncé) : toute requête complexe (connexion, recherche, location) passe par un service, jamais directement par les entités. Ils orchestrent les repositories, appliquent les règles d'affaires et convertissent les entités en DTOs avec AutoMapper. | Repositories, Entités, DTOs | HTTP |
| `Dtos/` | Objets échangés avec le frontend. **Entrée** : `ConnexionDto`, `NouvelleLocationDto`. **Sortie** : `ClientDto` (avec `AdresseDto`, `ForfaitDto`, `CarteCreditDto` : numéro de carte masqué, ni CVV ni mot de passe), `FilmResumeDto`, `FilmDetailDto` (avec `PersonneResumeDto`, `ActeurDto`), `PersonneDto`, `CriteresRechercheDto`, `LocationDto`. | — | — |
| `Mapping/` | Profils AutoMapper qui décrivent la conversion Entité -> DTO : `ClientProfile` aplatit `Utilisateur` dans `ClientDto` ; `CatalogueProfile` aplatit les tables d'association et regroupe les personnages par acteur. | Entités, DTOs | — |

Les autres fichiers de l'Api :

| Élément | Rôle |
| --- | --- |
| `Program.cs` | Composition de l'application : enregistre l'Infrastructure, AutoMapper, les services, l'authentification par cookie et les controllers, puis démarre le serveur. |
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

Depuis la racine du dépôt :

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project Lab2.Api       # écoute sur http://localhost:5080
```

> La BD (`bdlog660.ens.ad.etsmtl.ca`) n'est joignable que depuis le réseau de l'ÉTS ou par VPN. Sinon : `ORA-12154`.

## 5 - Endpoints

La session est un cookie HttpOnly (`webflix.session`) posé par `login`. Toutes les routes sauf `login`/`logout` exigent ce cookie, sinon **401**. Les erreurs sont des `ProblemDetails` (RFC 7807) : le message à afficher est dans `detail`.

| Cas | Méthode et route | Corps / paramètres | Réponses |
| --- | --- | --- | --- |
| 2 | `POST /api/auth/login` | `{ "courriel": "...", "motDePasse": "..." }` | 200 `ClientDto` + cookie ; 401 courriel ou mot de passe invalide ; 400 champ manquant |
| 2 | `POST /api/auth/logout` | — | 204 |
| 2 | `GET /api/clients/me` | — | 200 `ClientDto` du client connecté (restaure la session au rechargement) |
| 3 | `GET /api/films/criteres` | — | 200 `{ genres, pays, langues }` pour les listes déroulantes |
| 3 | `GET /api/films` | `titre`, `anneeMin`, `anneeMax`, `pays`, `langue`, `genre`, `realisateur`, `acteur` (tous facultatifs, répétables sauf années et langue) | 200 `FilmResumeDto[]` trié par titre (liste vide = aucun résultat) ; 400 si `anneeMin > anneeMax` |
| 3 | `GET /api/films/{id}` | — | 200 `FilmDetailDto` (avec `copiesDisponibles`) ; 404 |
| 3 | `GET /api/personnes/{id}` | — | 200 `PersonneDto` (réalisateur ou acteur) ; 404 |
| 4 | `POST /api/locations` | `{ "idFilm": 74958 }` | 201 `LocationDto` ; 404 film inexistant ; 409 aucune copie disponible ou quota du forfait atteint |

Recherche : les critères texte (`titre`, `realisateur`, `acteur`) sont découpés en mots, sans tenir compte de la casse ni de l'ordre (`acteur=hanks tom` trouve « Tom Hanks »). Un critère répété doit être satisfait à chaque fois : `acteur=hanks tom&acteur=ryan meg` donne les films avec les deux acteurs. Les champs pouvant être vides en BD (voir `Entities/`) arrivent à `null` dans le JSON : le frontend doit les gérer.

Location : la copie est choisie par le serveur et le client est celui de la session. Le quota et la disponibilité sont vérifiés dans `LocationService` pour donner des messages clairs. Le trigger `TRG_LOCATION_REGLES` du labo 1 reste le garde-fou contre les locations simultanées : ses erreurs `ORA-20003`/`ORA-20004` sont traduites en 409.

### Frontend et cookie

Le cookie est `SameSite=Strict` et l'API n'active pas CORS : le frontend doit appeler l'API **depuis la même origine**. En développement, on passe par le proxy du serveur de dev, par exemple avec Vite :

```js
// vite.config.js
server: { proxy: { '/api': 'http://localhost:5080' } }
```

Le frontend appelle ensuite `fetch('/api/...')` : le navigateur renvoie le cookie automatiquement.

### Test rapide (PowerShell)

```powershell
# Cas 2 : Robert Flores, forfait A (10 locations, durée illimitée)
curl.exe -c jar.txt -H "Content-Type: application/json" -d '{\"courriel\":\"RobertCFlores21@gmail.com\",\"motDePasse\":\"eishie3meiH\"}' http://localhost:5080/api/auth/login

# Cas 3 : films avec Tom Hanks ET Meg Ryan -> Sleepless in Seattle (1993), You've Got Mail (1998)
curl.exe -b jar.txt "http://localhost:5080/api/films?acteur=hanks%20tom&acteur=ryan%20meg"
# Cas 3 : fiche de The Lion King (1994), puis celle de James Earl Jones (Mufasa)
curl.exe -b jar.txt http://localhost:5080/api/films/110357
curl.exe -b jar.txt http://localhost:5080/api/personnes/469

# Cas 4 : location d'un film ayant plusieurs copies (201, dateRetourMax null pour le forfait A)
curl.exe -b jar.txt -H "Content-Type: application/json" -d '{\"idFilm\":74958}' http://localhost:5080/api/locations
```
