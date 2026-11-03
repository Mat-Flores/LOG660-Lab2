# Lancer et tester le backend

## Configuration

Il faut le SDK .NET 9, l'accès réseau au serveur Oracle et le schéma du labo 1 déjà chargé.
Les identifiants Oracle servent à connecter le serveur à la base. Pour se connecter
à l'application, il faut le courriel et le mot de passe d'un client du labo 1.

Depuis la racine du dépôt, entrer dans `backend` :

```powershell
cd backend
dotnet restore Lab2.sln
dotnet build Lab2.sln
```

Si le terminal est déjà dans `backend`, ne pas refaire `cd backend`.
Pour utiliser sa propre connexion Oracle sans modifier le fichier partagé :

```powershell
$env:ConnectionStrings__Default = 'User Id=VOTRE_SCHEMA;Password=VOTRE_MOT_DE_PASSE;Data Source=HOTE:PORT/SERVICE'
dotnet run --project Lab2.Api --launch-profile http
```

La variable remplace `ConnectionStrings:Default` de `Lab2.Api/appsettings.json`
pour ce terminal. Adapter l'hôte, le port et le service aux paramètres Oracle de l'équipe.
Le pilote Oracle est installé avec les dépendances NuGet. Le démarrage ne crée pas de tables.

Garder ce terminal ouvert. Les appels suivants se font dans un second terminal PowerShell,
en suivant les sections dans l'ordre.

## Vérifier que le serveur répond

```powershell
$api = 'http://localhost:5080'
$listes = Invoke-RestMethod "$api/api/films/criteres"
$listes | ConvertTo-Json -Depth 4
```

On doit obtenir les genres, pays et langues présents dans la base.
Cette adresse s'ouvre aussi dans le navigateur :
[http://localhost:5080/api/films/criteres](http://localhost:5080/api/films/criteres).
La racine `http://localhost:5080/` retourne `404` : l'API n'a pas de page d'accueil.

## Recherche et fiches

```powershell
$films = Invoke-RestMethod "$api/api/films?titre=star"
$films | Format-Table idFilm, titre, anneeSortie

Invoke-RestMethod "$api/api/films?titre=star&genre=Sci-Fi&langue=English&anneeMin=1970&anneeMax=1985" |
    Format-Table idFilm, titre, anneeSortie
```

Adapter les filtres aux valeurs renvoyées par `/api/films/criteres`.
Quelques cas à comparer :

| Recherche | Résultat attendu |
| --- | --- |
| `titre=star%20wars` et `titre=wars%20star` | Mêmes films. |
| `acteur=ford&acteur=hamill` | Films contenant les deux acteurs. |
| `genre=Adventure&genre=Sci-Fi` | Films ayant les deux genres. |
| `anneeMin=abc` ou `anneeMin=2000&anneeMax=1900` | `400`. |
| Titre absent de la base | Liste vide. |

Pour consulter un film trouvé dans les résultats :

```powershell
$idFilm = [int](Read-Host 'Identifiant du film')
$fiche = Invoke-RestMethod "$api/api/films/$idFilm"
$fiche | ConvertTo-Json -Depth 8

if ($null -ne $fiche.realisateur) {
    Invoke-RestMethod "$api/api/personnes/$($fiche.realisateur.idPersonne)" | Format-List
}
```

Vérifier les pays, genres, réalisateur, scénaristes, acteurs et personnages.
Un acteur avec plusieurs rôles doit apparaître une fois, avec sa liste de personnages.
Les informations facultatives absentes restent `null`. Une fiche inexistante retourne `404`.

## Connexion

Exécuter le bloc complet. Les variables sont remises à zéro pour ne pas réutiliser
une ancienne connexion si la nouvelle échoue.

```powershell
$profil = $null
$jeton = $null
$entetes = @{}
$reponse = $null

try {
    $identifiants = Get-Credential -Message 'Courriel et mot de passe du client'
    $corps = @{
        courriel = $identifiants.UserName
        motDePasse = $identifiants.GetNetworkCredential().Password
    } | ConvertTo-Json

    $reponse = Invoke-WebRequest "$api/api/auth/login" `
        -Method Post -ContentType 'application/json' -Body $corps `
        -UseBasicParsing -ErrorAction Stop

    $profil = $reponse.Content | ConvertFrom-Json
    $jeton = @($reponse.Headers['X-Auth-Token'])[0]
    $entetes = @{ Authorization = "Bearer $jeton" }
    "Connexion HTTP : $($reponse.StatusCode)"
    "Jeton reçu : $(-not [string]::IsNullOrWhiteSpace($jeton))"
    $profil | ConvertTo-Json -Depth 5
}
catch {
    if ($null -ne $_.Exception.Response) {
        "HTTP : $([int]$_.Exception.Response.StatusCode)"
    }
    $_.Exception.Message
    $_.ErrorDetails.Message
}
```

Attendu : `200`, jeton reçu `True`, profil avec forfait et carte masquée.
Le mot de passe et le CVV ne doivent pas être présents dans la réponse.
Le jeton se trouve dans `$reponse.Headers['X-Auth-Token']`, pas dans le JSON du profil.
Il expire après 30 minutes : refaire la connexion si nécessaire.

Un mauvais mot de passe, un courriel inconnu ou un compte uniquement employé donne `401`.
Un champ requis vide donne `400`. Après un test avec de mauvais identifiants,
refaire une connexion valide avant de passer à la location.

## Location

Choisir un client ayant une place dans son forfait et un film avec une copie libre.
Cette requête permet de trouver des copies disponibles dans Oracle :

```sql
SELECT c.idFilm, c.codeCopie
FROM copie c
WHERE NOT EXISTS (
    SELECT 1 FROM location l
    WHERE l.codeCopie = c.codeCopie AND l.dateRetour IS NULL
)
FETCH FIRST 20 ROWS ONLY;
```

Une location réussie ajoute une ligne dans la base connectée.
Dans le terminal utilisé pour la connexion :

```powershell
$reponseLocation = $null
$location = $null

try {
    if ($null -eq $profil -or [string]::IsNullOrWhiteSpace($jeton)) {
        throw 'Faire une connexion valide avant de louer.'
    }
    $idFilm = [int](Read-Host 'Identifiant du film avec copie disponible')
    $demande = @{ idUtilisateur = $profil.idUtilisateur; idFilm = $idFilm } | ConvertTo-Json

    $reponseLocation = Invoke-WebRequest "$api/api/locations" `
        -Method Post -Headers $entetes -ContentType 'application/json' `
        -Body $demande -UseBasicParsing -ErrorAction Stop

    "Location HTTP : $($reponseLocation.StatusCode)"
    $location = $reponseLocation.Content | ConvertFrom-Json
    $location | Format-List
}
catch {
    if ($null -ne $_.Exception.Response) {
        "HTTP : $([int]$_.Exception.Response.StatusCode)"
    }
    $_.Exception.Message
    $_.ErrorDetails.Message
}
```

Attendu : `201` avec `idLocation`, `idFilm`, `codeCopie` et `dateLocation`.
Pour vérifier dans Oracle, remplacer `123` par l'identifiant reçu :

```sql
SELECT idLocation, idUtilisateur, codeCopie, dateLocation, dateRetour
FROM location
WHERE idLocation = 123;
```

La ligne doit appartenir au client connecté, avec `dateRetour` à `NULL`.
Pour vérifier son quota, remplacer `42` par `$profil.idUtilisateur` :

```sql
SELECT c.idUtilisateur, f.locationsMax, COUNT(l.idLocation) AS locationsActives
FROM client c
JOIN forfait f ON f.code = c.codeForfait
LEFT JOIN location l ON l.idUtilisateur = c.idUtilisateur AND l.dateRetour IS NULL
WHERE c.idUtilisateur = 42
GROUP BY c.idUtilisateur, f.locationsMax;
```

| Cas à tester | Statut attendu |
| --- | --- |
| Jeton absent, invalide ou expiré | `401`. Refaire la connexion et reconstruire `$entetes` si le jeton a expiré. |
| `idUtilisateur` différent de celui du client connecté | `403`. |
| Identifiant absent, nul ou négatif | `400`. |
| Film inexistant | `404`. |
| Toutes les copies sont louées | `409`. |
| Quota atteint : D = 1, I = 5, A = 10 | `409`. |
| Forfait manquant | `409`. |

Les refus ne doivent ajouter aucune location. Dans PowerShell, les réponses `4xx`
passent dans le `catch` ; le statut et le message sont affichés.
Dans Postman, copier le jeton reçu dans `X-Auth-Token` vers l'onglet Authorization,
type Bearer Token, de `POST /api/locations`. Le corps JSON contient `idUtilisateur` et `idFilm`.

Pour libérer une location de test en conservant l'historique, remplacer les deux
identifiants par ceux de la location et du client :

```sql
UPDATE location
SET dateRetour = SYSDATE
WHERE idLocation = 123 AND idUtilisateur = 42 AND dateRetour IS NULL;
COMMIT;
```

La copie retournée doit pouvoir être relouée.

## Demandes simultanées

Depuis deux terminaux ou onglets Postman, utiliser deux clients ayant du quota et
un film avec une seule copie libre. Envoyer les demandes en même temps : une seule
doit réussir, l'autre doit recevoir `409`. Vérifier qu'il n'existe qu'une location
active pour cette copie.

Refaire avec un client D sans location et deux films différents. Une seule demande
doit réussir ; le client doit garder une seule location active.
Ces essais se font sur Oracle pour vérifier les verrous et les triggers.

Le frontend doit encore transmettre le jeton pour tester la location depuis ses écrans.
Les choix de mappage et de fonctionnement sont dans [CONCEPTION.md](CONCEPTION.md).
