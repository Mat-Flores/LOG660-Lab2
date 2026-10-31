# Vérifier et présenter le backend

## 1 - Démarrer l'API

Depuis la racine du dépôt :

```powershell
dotnet restore backend/Lab2.sln
dotnet build backend/Lab2.sln
# Si nécessaire, remplacer la connexion pour ce terminal :
$env:ConnectionStrings__Default = 'User Id=VOTRE_SCHEMA;Password=VOTRE_MOT_DE_PASSE;Data Source=HOTE:PORT/SERVICE'
dotnet run --project backend/Lab2.Api
```

Garder ce terminal ouvert. La suite se fait dans un second terminal PowerShell.
Il faut accéder au réseau du serveur Oracle et utiliser un client et son mot de passe du labo 1.
Une erreur Oracle de connexion se résout d'abord dans la configuration réseau/connexion.
Aucune création de schéma ou migration n'est nécessaire.

## 2 - Se connecter et conserver le jeton

```powershell
$api = 'http://localhost:5080'
$identifiants = Get-Credential -Message 'Courriel et mot de passe du client du labo 1'
$corps = @{
    courriel = $identifiants.UserName
    motDePasse = $identifiants.GetNetworkCredential().Password
} | ConvertTo-Json
$reponse = Invoke-WebRequest "$api/api/auth/login" -Method Post -ContentType 'application/json' -Body $corps -UseBasicParsing
$profil = $reponse.Content | ConvertFrom-Json
$jeton = @($reponse.Headers['X-Auth-Token'])[0]
$entetes = @{ Authorization = "Bearer $jeton" }
$profil | ConvertTo-Json -Depth 5
```

Attendu : `200`, profil du client et forfait. La carte est masquée ; le mot de passe
et le CVV sont absents. Le jeton est dans un en-tête séparé, avec une validité de 30 minutes.

Refaire l'appel avec un mauvais mot de passe : `401` avec un message clair.
Un compte uniquement employé doit aussi être refusé. Un champ requis vide donne `400`.
PowerShell lève une exception pour les réponses `4xx` : le statut est consultable dans
`$_.Exception.Response.StatusCode` à l'intérieur d'un bloc `catch`.

## 3 - Rechercher et consulter

Les consultations ne nécessitent pas de jeton :

```powershell
$listes = Invoke-RestMethod "$api/api/films/criteres"
$listes | ConvertTo-Json -Depth 4
$films = Invoke-RestMethod "$api/api/films?titre=star"
$films | Format-Table idFilm, titre, anneeSortie
```

Choisir des mots présents dans la base. Une liste vide est un résultat valide si aucun
film ne correspond. Pour vérifier les mots inversés, comparer `titre=star%20wars`
et `titre=wars%20star` si ce titre existe dans les données.

Autres exemples à adapter aux valeurs des listes :

```text
/api/films?anneeMin=1970&anneeMax=1990
/api/films?acteur=ford&acteur=hamill
/api/films?genre=Aventure&genre=Science-fiction
/api/films?pays=USA&langue=English
```

Un même critère acteur ne doit pas assembler les mots des noms de personnes différentes.
Les critères répétés doivent tous être satisfaits.

Choisir un identifiant trouvé dans les résultats :

```powershell
$idFilm = [int](Read-Host 'Identifiant du film à consulter')
$fiche = Invoke-RestMethod "$api/api/films/$idFilm"
$fiche | ConvertTo-Json -Depth 8
# Si le film a un réalisateur :
if ($null -ne $fiche.realisateur) {
    Invoke-RestMethod "$api/api/personnes/$($fiche.realisateur.idPersonne)" | Format-List
}
```

Attendu : fiche avec les informations disponibles, les genres et pays, les personnes,
les personnages et les bandes-annonces. Un acteur à plusieurs rôles apparaît une fois.
Une valeur facultative absente reste `null`. Un identifiant inexistant retourne `404`.
Tester `anneeMin=abc`, `anneeMin=0` et `anneeMin=2000&anneeMax=1900` : `400`.

## 4 - Choisir un cas de location sur Oracle

Dans votre outil SQL, ces lectures permettent de repérer le quota et une copie libre.
Remplacer les identifiants par ceux choisis :

```sql
SELECT c.idUtilisateur, c.codeForfait, f.locationsMax,
       (SELECT COUNT(*) FROM location l
        WHERE l.idUtilisateur = c.idUtilisateur AND l.dateRetour IS NULL) AS locationsActives
FROM client c
JOIN forfait f ON f.code = c.codeForfait
WHERE c.idUtilisateur = 42;

SELECT c.idFilm, c.codeCopie
FROM copie c
WHERE NOT EXISTS (
    SELECT 1 FROM location l
    WHERE l.codeCopie = c.codeCopie AND l.dateRetour IS NULL
)
FETCH FIRST 20 ROWS ONLY;
```

Les écritures de la prochaine étape ajoutent de vraies locations dans le schéma connecté.
Utiliser un compte et une copie prévus pour la démonstration.

## 5 - Louer et vérifier l'écriture

```powershell
$idFilm = [int](Read-Host 'Identifiant du film avec copie disponible')
$demande = @{ idUtilisateur = $profil.idUtilisateur; idFilm = $idFilm } | ConvertTo-Json
$reponseLocation = Invoke-WebRequest "$api/api/locations" -Method Post -Headers $entetes -ContentType 'application/json' -Body $demande -UseBasicParsing
$location = $reponseLocation.Content | ConvertFrom-Json
$location | Format-List
```

Attendu : `201` avec `idLocation`, `idFilm`, `codeCopie` et `dateLocation`.
Dans Oracle, avec l'identifiant reçu :

```sql
SELECT idLocation, idUtilisateur, codeCopie, dateLocation, dateRetour
FROM location
WHERE idLocation = 123;
```

Attendu : une ligne pour le client connecté, avec `dateRetour` absente.

Vérifier ensuite les refus :

| Situation | Attendu |
| --- | --- |
| Même requête sans l'en-tête Authorization | `401`, aucune insertion. |
| Jeton modifié ou expiré | `401`, aucune insertion. |
| Identifiant d'un autre client dans le JSON | `403`, aucune insertion. |
| Identifiant utilisateur ou film absent, nul ou négatif | `400`. |
| Film inexistant | `404`. |
| Film dont toutes les copies sont déjà louées | `409`, message de disponibilité. |
| Client D/I/A ayant déjà 1/5/10 locations actives | `409`, message de quota. |
| Client sans forfait exploitable | `409`, message de forfait. |

Les codes et messages d'erreur sont visibles dans Postman ou dans la réponse HTTP.
Avec Postman, copier `X-Auth-Token` de la réponse de connexion dans l'authentification
`Bearer Token` de la requête de location.

Pour libérer une location de démonstration, votre outil SQL peut enregistrer son retour
avec l'identifiant exact reçu et celui du client :

```sql
UPDATE location
SET dateRetour = SYSDATE
WHERE idLocation = 123 AND idUtilisateur = 42 AND dateRetour IS NULL;
COMMIT;
```

Cela conserve l'historique. Vérifier qu'une copie retournée peut ensuite être relouée.

## 6 - Vérifier les demandes simultanées sur Oracle

Avec deux clients disposant d'une place dans leur forfait, choisir un film ayant exactement
une copie libre. Envoyer les deux requêtes depuis deux onglets Postman ou terminaux.
Une seule doit créer une location ; l'autre doit recevoir `409`.
Vérifier qu'une seule location active existe pour la copie.

Avec un client D sans location, lancer deux demandes vers des films différents.
Une seule doit réussir ; l'autre doit être refusée par le quota.
Le nombre de locations actives de ce client doit rester égal à un.
Ces vérifications doivent utiliser cette API et le même schéma Oracle.
Elles complètent les tests SQLite, qui n'exécutent pas les verrous ni les triggers Oracle.

## 7 - Points à expliquer à l'oral

1. Le contrôleur reçoit la demande ; le service sert de façade ; le repository utilise EF Core.
2. `Client` hérite des données communes de `Utilisateur`. Le TPT joint les deux tables existantes.
3. Les associations et leurs clés composées correspondent au SQL du labo 1.
4. Les critères de recherche deviennent des filtres SQL combinés, sans charger tout le catalogue pour filtrer.
5. Le jeton identifie le client côté serveur ; le JSON seul ne constitue pas une preuve de connexion.
6. La transaction regroupe le contrôle du quota, le choix de copie et l'insertion.
7. Les verrous client puis film coordonnent les locations simultanées de cette API.
8. Les DTO limitent les données retournées et masquent la carte.

Le détail des choix se trouve dans [CONCEPTION.md](CONCEPTION.md).
Le frontend reste à intégrer avec l'en-tête du jeton pour le parcours de location.

## 8 - Tests automatisés locaux

Le dossier local `Lab2.Api.Tests/` est ignoré par Git. Depuis la racine :

```powershell
dotnet test backend/Lab2.Api.Tests/Lab2.Api.Tests.csproj -c Release
```

Le test Oracle en lecture est ignoré par défaut. Pour l'exécuter séparément sur votre réseau :

```powershell
$env:LOG660_ORACLE = '1'
# Facultatif si la configuration appsettings.json ne convient pas :
$env:LOG660_ORACLE_CONNECTION = 'User Id=VOTRE_SCHEMA;Password=VOTRE_MOT_DE_PASSE;Data Source=HOTE:PORT/SERVICE'
dotnet test backend/Lab2.Api.Tests/Lab2.Api.Tests.csproj --filter FullyQualifiedName~OracleLectureTests
Remove-Item Env:LOG660_ORACLE
```

Ce test compare les colonnes et exécute des lectures ORM dans une transaction en lecture seule.
Il ne remplace pas les essais d'écriture et de concurrence ci-dessus.
Conserver les sources des tests pour la démonstration et la remise séparée prévue par l'équipe.
