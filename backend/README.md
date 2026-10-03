# MonApp — fullstack .NET (EF Core + Oracle) + frontend

## Structure

```
Lab2/
├── Lab2.sln
├── backend/
│   ├── Lab2.Infrastructure/              # Accès aux données (aucune dépendance vers Api)
│   │   ├── Entities/                     # Classes liées aux tables (Utilisateur, Adresse)
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs           # Point d'entrée EF Core (DbSet = tables)
│   │   │   ├── Configurations/           # Mappage fluent : noms Oracle, colonnes, index, relations
│   │   │   └── Migrations/               # Générées par dotnet ef
│   │   ├── Repositories/
│   │   │   ├── Interfaces/               # IUtilisateurRepository, IAdresseRepository
│   │   │   ├── UtilisateurRepository.cs
│   │   │   └── AdresseRepository.cs
│   │   └── DependencyInjection.cs        # AddInfrastructure(): DbContext + repositories
│   │
│   └── Lab2.Api/                         # Exposition HTTP (dépend de Infrastructure)
│       ├── Controllers/                  # Endpoints REST (UtilisateursController, HealthController)
│       ├── Services/                     # Logique métier / orchestration des repositories
│       ├── Dtos/                         # Objets échangés avec le frontend
│       ├── Mapping/                      # Profils AutoMapper (Entité <-> DTO)
│       ├── Middleware/                   # Gestion d'erreurs globale, logs (optionnel)
│       ├── Program.cs                    # Composition : Infrastructure, mappage, CORS, Swagger
│       └── appsettings.json              # Chaîne de connexion (sans mot de passe)
│
└── frontend/                             # Application cliente (indépendante)
```

Flux :
`Frontend <-> Controller <-> DTO <-> (Profile) <-> Service <-> Entité <-> Repository <-> DbContext <-> Oracle`

### Règles de dépendance

| Couche | Connaît | Ne connaît pas |
|---|---|---|
| Controllers | Services, DTOs | DbContext, Repositories, Entités |
| Services | Repositories, Entités, DTOs (via AutoMapper) | HTTP |
| Repositories | DbContext, Entités | DTOs, HTTP |

Les entités ne sortent jamais de l'API : les controllers ne manipulent que des DTOs.

## Connexion à Oracle

1. Dans `Lab2.Api/appsettings.json`, adapter `User Id` et `Data Source` (`hôte:port/nom_du_service`, le même que dans SQL Developer).
2. Mettre le mot de passe hors du dépôt (user-secrets, sur le projet de démarrage `Lab2.Api`) :

```bash
cd backend/Lab2.Api
dotnet user-secrets set "ConnectionStrings:Default" "User Id=EQUIPE201;Password=<mot_de_passe>;Data Source=bdlog660.ens.ad.etsmtl.ca:1521/ORCLPDB1"
```

## Premier lancement

Les migrations vivent dans l'Infrastructure, mais la config (chaîne de connexion) est lue depuis l'Api, d'où les deux options `--project` / `--startup-project` :

```bash
dotnet tool install --global dotnet-ef        # une seule fois
dotnet restore

dotnet ef migrations add Initial \
  --project backend/Lab2.Infrastructure \
  --startup-project backend/Lab2.Api \
  --output-dir Data/Migrations

dotnet ef database update \
  --project backend/Lab2.Infrastructure \
  --startup-project backend/Lab2.Api       # crée UTILISATEUR et ADRESSE

dotnet run --project backend/Lab2.Api      # http://localhost:5080/api/health
```

## Création du squelette

```bash
dotnet new sln -n Lab2
dotnet new classlib -n Lab2.Infrastructure -o backend/Lab2.Infrastructure
dotnet new webapi   -n Lab2.Api            -o backend/Lab2.Api
dotnet sln add backend/Lab2.Infrastructure backend/Lab2.Api
dotnet add backend/Lab2.Api reference backend/Lab2.Infrastructure
```

Packages :
- `Lab2.Infrastructure` : `Oracle.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`
- `Lab2.Api` : `AutoMapper`, `Microsoft.EntityFrameworkCore.Design`

## Composition (Program.cs)

```csharp
builder.Services.AddInfrastructure(builder.Configuration); // DbContext + repositories
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddControllers();
builder.Services.AddCors(...);
```

## Notes

- **Provider** : `Oracle.EntityFrameworkCore` 10.x (officiel Oracle), qui exige Oracle Database **19c ou plus**.
- **Tables déjà existantes** : si `UTILISATEUR` / `ADRESSE` existent déjà, ne pas lancer la migration ;
  générer plutôt les entités depuis la BD :
  `dotnet ef dbcontext scaffold "<connexion>" Oracle.EntityFrameworkCore --project backend/Lab2.Infrastructure --startup-project backend/Lab2.Api --output-dir Entities --context-dir Data`
- **Noms en majuscules** : EF Core met les identifiants entre guillemets ; sans majuscules explicites, il faudrait
  écrire `SELECT * FROM "Utilisateur"` dans SQL Developer.
- **AutoMapper 15+** est sous licence commerciale avec un tier Community gratuit (mode « honor system » :
  ça fonctionne sans clé, un avertissement apparaît dans les logs). Clé via
  `dotnet user-secrets set "AutoMapper:LicenseKey" "<clé>"`.