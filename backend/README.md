# LOG660 - Labo 2 : backend

API ASP.NET Core (.NET 9) pour la connexion, la consultation du catalogue et la location de films.
Les données proviennent de la base Oracle du labo 1.

## Organisation

- `Lab2.Api/Controllers` : routes HTTP et réponses.
- `Lab2.Api/Services` : façades et règles métier.
- `Lab2.Api/Dtos` : données reçues ou retournées par l'API.
- `Lab2.Api/Mapping` : conversion des entités en DTO avec AutoMapper.
- `Lab2.Infrastructure/Entities` : classes correspondant aux 17 tables.
- `Lab2.Infrastructure/Data` : contexte EF Core et configurations des relations.
- `Lab2.Infrastructure/Repositories` : lectures, écritures et transactions avec Oracle.

Flux d'une requête :

`Contrôleur -> Service -> Repository -> AppDbContext -> Oracle`

## Lancement

Prérequis : SDK .NET 9 et accès à la base du cours depuis le réseau ÉTS ou le VPN.
La chaîne de connexion est lue dans `Lab2.Api/appsettings.json`, à la clé `ConnectionStrings:Default`.
Elle peut être remplacée par la variable d'environnement `ConnectionStrings__Default`.

Depuis la racine du dépôt :

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project Lab2.Api
```

L'API écoute sur `http://localhost:5080`.
L'application utilise le schéma existant : elle ne crée ni ne modifie les tables au démarrage.

## Routes

| Méthode | Route | Utilisation |
| --- | --- | --- |
| POST | `/api/auth/connexion` | Vérifier le courriel et le mot de passe, puis ouvrir une session. |
| POST | `/api/auth/deconnexion` | Fermer la session. |
| GET | `/api/clients/{id}` | Consulter son propre profil client. |
| GET | `/api/films` | Rechercher des films. |
| GET | `/api/films/{id}` | Consulter la fiche d'un film. |
| GET | `/api/personnes/{id}` | Consulter les renseignements et la biographie d'une personne. |
| POST | `/api/locations` | Louer une copie disponible du film demandé. |
| GET | `/api/locations` | Consulter ses locations en cours. |

Toutes les routes, sauf la connexion, demandent le cookie de session.
La session expire après 30 minutes d'inactivité. Le mot de passe n'est pas retourné dans les réponses.

## Recherche

Les paramètres facultatifs sont `titre`, `acteur`, `realisateur`, `genre`, `pays`,
`langue`, `anneeMin` et `anneeMax`. Un film doit respecter tous les filtres fournis.
Les bornes des années sont inclusives.

Le titre, le nom de l'acteur et celui du réalisateur acceptent des mots partiels dans
n'importe quel ordre, sans tenir compte des majuscules. Ainsi, `titre=wars star` peut
retrouver « Star Wars ». Pour une personne, tous les mots doivent appartenir au même nom.

Le genre, le pays et la langue correspondent à une valeur complète, sans tenir compte des majuscules.
Sans filtre, la recherche retourne tous les films, triés par titre.
Une recherche sans résultat retourne une liste vide.

## Location

Le corps de la requête contient seulement le film choisi :

```json
{ "idFilm": 123 }
```

L'identifiant du client vient de la session, pas du corps de la requête.
L'application vérifie que le compte est un client, qu'il possède un forfait valide,
qu'il n'a pas atteint son quota et qu'une copie du film est disponible.

| Forfait | Locations simultanées | Durée maximale |
| --- | ---: | ---: |
| D | 1 | 10 jours |
| I | 5 | 30 jours |
| A | 10 | Illimitée |

Une location est en cours lorsque `dateRetour` est absente.
Une ancienne location retournée ne bloque ni la copie ni le quota.
La date de retour prévue est calculée à partir du forfait courant; elle n'est pas stockée dans une nouvelle colonne.
Pour le forfait A, elle vaut `null`.

La vérification et l'insertion se déroulent dans une transaction.
Sur Oracle, le client puis le film sont verrouillés avec `SELECT ... FOR UPDATE WAIT 5`.
Les appels de location de l'API respectent cet ordre pour protéger le quota et la disponibilité.
Les valeurs SQL sont passées en paramètres. L'insertion de `Location` utilise EF Core.
Les triggers du labo 1 restent actifs.

La réponse `201` contient l'identifiant de location généré par Oracle, le film,
le code de copie, la date de location et la date de retour prévue.
La liste des locations ne retourne que celles du client connecté.

## Réponses d'erreur

| Code | Situation |
| --- | --- |
| 400 | Champs invalides, identifiant de film non positif ou intervalle d'années incohérent. |
| 401 | Identifiants incorrects ou session absente. |
| 403 | Consultation du profil d'un autre utilisateur ou location avec un compte non client. |
| 404 | Film, personne ou profil client introuvable. |
| 409 | Quota atteint, aucune copie disponible, forfait absent ou conflit de location. |

## Mappage Oracle

Le mappage utilise les noms de tables et de colonnes en majuscules, les chaînes `VARCHAR2`,
les dates `DATE` et un `CLOB` pour la biographie. Le SQL généré reste compatible avec Oracle 19c et les versions suivantes.
Les colonnes facultatives du schéma existant sont représentées par des types C# nullables.
Les réponses peuvent donc contenir `null` lorsqu'un renseignement est absent dans la base.

Les identifiants existants sont conservés. Seul `Location.IdLocation` est généré à l'insertion par la colonne identity.
Les tables associatives gardent leurs clés composées.

La fiche détaillée d'un film charge ses collections avec les
[requêtes séparées EF Core](https://learn.microsoft.com/ef/core/querying/single-split-queries).
Les [transactions EF Core](https://learn.microsoft.com/ef/core/saving/transactions)
encadrent les écritures et les [verrous Oracle](https://docs.oracle.com/en/database/oracle/oracle-database/21/adfns/sql-processing-for-application-developers.html)
sont libérés à la validation ou à l'annulation.

## Démonstration et conception

- [Démonstration](DEMONSTRATION.md) : commandes PowerShell et résultats attendus.
- [Conception](CONCEPTION.md) : responsabilités des classes, mappage et déroulement d'une location.
