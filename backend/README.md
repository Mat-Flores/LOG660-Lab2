# Lab2 - Backend

Le serveur couvre les cas d'utilisation 2 (connexion), 3 (consultation) et 4 (location).
Il utilise les tables et les données Oracle du labo 1 avec Entity Framework Core,
l'équivalent ORM retenu pour les deux projets .NET existants.

## 1 - Structure

```text
backend/
├── Lab2.sln
├── README.md
├── CONCEPTION.md
├── DEMONSTRATION.md
├── Lab2.Infrastructure/
│   ├── DependencyInjection.cs
│   ├── Entities/                     # Classes métier et annotations ORM
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/           # Clés, associations et héritage TPT
│   ├── Models/RegleLocationException.cs
│   └── Repositories/
│       ├── Interfaces/
│       ├── ClientRepository.cs
│       ├── CatalogueRepository.cs
│       ├── LocationRepository.cs
│       └── RechercheFilmCriteres.cs
└── Lab2.Api/
    ├── Controllers/                  # Auth, Films, Personnes, Locations
    ├── Services/                     # Façades des cas 2, 3 et 4
    ├── Dtos/                         # Contrats HTTP d'entrée et de sortie
    ├── Mapping/                      # Conversion entités → DTO
    ├── Properties/launchSettings.json
    ├── Program.cs
    └── appsettings.json
```

## 2 - Rôle des composants

| Composant | Responsabilité |
| --- | --- |
| Contrôleurs | Recevoir les demandes, contrôler l'identité pour la location et retourner les réponses HTTP. |
| `ClientService` | Vérifier le courriel et le mot de passe d'un client, puis construire son profil. |
| `CatalogueService` | Fournir la recherche, les critères disponibles et les fiches des films et personnes. |
| `LocationService` | Orchestrer les contrôles métier et la transaction de location. |
| Repositories | Exécuter les lectures et écritures EF Core. |
| Entités et configurations | Mapper les classes, propriétés, identifiants, associations et spécialisations sur Oracle. |
| Profils AutoMapper | Construire les DTO sans exposer les entités et leurs propriétés sensibles. |
| `Program.cs` | Configurer les dépendances, l'authentification par jeton, CORS et les contrôleurs. |

Les services forment les façades : le client déclenche une opération sans connaître
les requêtes ni les règles nécessaires à son exécution.
`Lab2.Api` dépend de `Lab2.Infrastructure`. Infrastructure ne dépend pas d'Api.

## 3 - Installation et lancement

Prérequis : SDK .NET 9, accès réseau à Oracle et identifiants du schéma du labo 1.
Le pilote Oracle est fourni par NuGet.

Depuis la racine du dépôt, dans PowerShell :

```powershell
dotnet restore backend/Lab2.sln
dotnet build backend/Lab2.sln
# Facultatif : remplacer la configuration uniquement pour ce terminal.
$env:ConnectionStrings__Default = 'User Id=VOTRE_SCHEMA;Password=VOTRE_MOT_DE_PASSE;Data Source=HOTE:PORT/SERVICE'
dotnet run --project backend/Lab2.Api
```

L'API écoute sur `http://localhost:5080` avec le profil fourni.
La connexion peut aussi être configurée dans `Lab2.Api/appsettings.json`.
Le fournisseur génère du SQL compatible Oracle 21c pour éviter les expressions
booléennes SQL des versions plus récentes.

L'application ne lance aucune migration ni création de tables. Elle utilise le schéma
existant, y compris ses contraintes et ses triggers.

## 4 - Endpoints

| Méthode | Route | Entrée | Réponse |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | `ConnexionDto` | `ClientDto` et en-tête `X-Auth-Token` |
| GET | `/api/films/criteres` | — | Genres, pays et langues pour les listes de recherche |
| GET | `/api/films` | Critères de recherche dans l'URL | Liste de `FilmResumeDto` |
| GET | `/api/films/{id}` | Identifiant du film | `FilmDetailDto` |
| GET | `/api/personnes/{id}` | Identifiant de la personne | `PersonneDto` |
| POST | `/api/locations` | `NouvelleLocationDto` et jeton | `LocationDto`, statut `201` |

### Connexion

```json
{ "courriel": "client@exemple.com", "motDePasse": "motDePasseDuClient" }
```

La recherche porte sur les clients, avec les données personnelles héritées de
`Utilisateur`. Un employé seul ne peut pas se connecter à cette application.
Un courriel inconnu ou un mauvais mot de passe retourne le même message avec `401`.
Le mot de passe reste comparé au format existant du labo 1.

Le JSON retourne le profil, l'adresse, le forfait et la carte masquée lorsqu'ils existent.
Le mot de passe, le CVV et le numéro complet de carte ne sont jamais renvoyés.
Le jeton est protégé par ASP.NET Core, expire après 30 minutes et se trouve dans
l'en-tête de réponse `X-Auth-Token`. La réponse de connexion est marquée `no-store`.

### Recherche et consultation

Les critères suivants sont facultatifs et combinés par un **ET** :

| Paramètre | Comportement |
| --- | --- |
| `titre` | Tous les mots partiels doivent apparaître dans le titre, dans n'importe quel ordre. |
| `anneeMin`, `anneeMax` | Intervalle inclusif ; années entre 1 et 9999. |
| `pays`, `genre`, `langue` | Valeur complète, sans distinction de majuscules. |
| `realisateur`, `acteur` | Tous les mots partiels doivent appartenir au nom d'une même personne. |

`titre`, `pays`, `genre`, `realisateur` et `acteur` peuvent être répétés.
Deux paramètres `acteur` exigent que les deux recherches d'acteurs soient satisfaites.

```text
/api/films?titre=wars%20star
/api/films?acteur=ford&acteur=hamill
/api/films?genre=Aventure&genre=Science-fiction
/api/films?anneeMin=1970&anneeMax=1990&langue=English
```

Les espaces seuls sont ignorés. Des critères contradictoires retournent une liste vide.
Un intervalle inversé ou un format d'année invalide retourne `400`.
Les longueurs maximales suivent les colonnes du schéma.

La fiche d'un film regroupe pays, genres, réalisateur, scénaristes, acteurs, personnages
et liens des bandes-annonces. Un acteur jouant plusieurs personnages apparaît une fois,
avec sa liste `personnages`. Les biographies se consultent par `/api/personnes/{id}`.
Les données facultatives absentes restent `null`.
Le catalogue est accessible sans jeton.

### Location

```http
POST /api/locations
Authorization: Bearer JETON_RECU_A_LA_CONNEXION
Content-Type: application/json
```

```json
{ "idUtilisateur": 42, "idFilm": 10 }
```

L'identifiant du corps doit correspondre au client du jeton.
Une demande valide réserve une copie disponible et insère une ligne dans `LOCATION`.
La date de retour reste absente tant que la copie n'a pas été retournée.

```json
{ "idLocation": 123, "idFilm": 10, "codeCopie": "COPIE-001", "dateLocation": "2026-10-31T00:00:00" }
```

Cet exemple illustre le format ; les identifiants et codes dépendent des données Oracle.
Les forfaits D, I et A permettent respectivement 1, 5 et 10 locations en cours.
Leurs durées maximales existantes sont de 10 jours, 30 jours et sans limite.
Le retour et la gestion des retards ne font pas partie des endpoints des cas 2, 3 et 4.

Une transaction couvre les contrôles et l'insertion. Sur Oracle, l'API verrouille le client,
puis le film, pour sérialiser les demandes simultanées de cette API. L'attente est limitée
à cinq secondes. Les triggers du labo 1 restent actifs.

| Statut | Signification |
| --- | --- |
| `200` | Connexion ou consultation réussie. |
| `201` | Location enregistrée. |
| `400` | Champs requis, identifiants ou critères invalides. |
| `401` | Identifiants de connexion invalides, jeton absent, invalide ou expiré pour une location. |
| `403` | Tentative de location pour un autre client, ou compte qui n'est plus un client. |
| `404` | Film ou personne introuvable. |
| `409` | Quota atteint, copie indisponible, forfait invalide ou conflit de location. |

Les refus métier utilisent `ProblemDetails` et les erreurs de saisie `ValidationProblemDetails`.

## 5 - Mappage du labo 1

Les 17 tables et leurs colonnes sont conservées. Les noms Oracle sont en majuscules,
les chaînes en `VARCHAR2`, les dates en `DATE`, la biographie en `CLOB` et le coût en
`NUMBER(5,2)`. Les clés composées des associations sont conservées.
`LOCATION.IDLOCATION` est généré par Oracle ; les identifiants existants ne sont pas régénérés.

`Utilisateur` est la classe abstraite commune. `Client` et `Employe` en héritent.
Le mappage TPT utilise les tables `UTILISATEUR`, `CLIENT` et `EMPLOYE`, reliées par leur
clé primaire partagée. Il ne demande aucune colonne de discriminateur.
Aucune fonctionnalité de gestion des employés ou de création de compte n'est ajoutée.

Le mappage ORM vers Oracle se trouve dans les entités et `Data/Configurations`.
Les profils AutoMapper servent à la conversion des réponses HTTP.
Les justifications détaillées figurent dans [CONCEPTION.md](CONCEPTION.md).

## 6 - Intégration et vérification

Le frontend est conservé tel quel. Lors de son intégration avec les locations protégées,
il devra lire `X-Auth-Token` après la connexion, puis transmettre
`Authorization: Bearer ...`. Le JSON `ClientDto`, les routes et le corps de location sont conservés.
CORS autorise les appels avec cet en-tête et expose `X-Auth-Token`, sans cookie ni credentials.

Le parcours complet de vérification sur Oracle, les résultats attendus et les points à
présenter sont dans [DEMONSTRATION.md](DEMONSTRATION.md).
Les tests automatisés sont conservés localement hors des fichiers suivis par Git.
La validation des verrous, triggers et écritures réelles doit être effectuée sur Oracle.
