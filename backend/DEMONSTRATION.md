# Démonstration du backend

## 1. Démarrer l'application

Vérifier que le VPN ÉTS est connecté si nécessaire et que le SDK .NET 9 est installé.
Depuis la racine du projet, ouvrir un premier terminal :

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project Lab2.Api
```

Résultat attendu : le serveur écoute sur `http://localhost:5080`.
Le laisser ouvert. Les commandes suivantes se lancent dans un second terminal PowerShell.

Si Oracle ne répond pas, vérifier d'abord la résolution du serveur et son port :

```powershell
Resolve-DnsName bdlog660.ens.ad.etsmtl.ca
Test-NetConnection bdlog660.ens.ad.etsmtl.ca -Port 1521
```

Le nom doit se résoudre et `TcpTestSucceeded` doit valoir `True`.
Une erreur `ORA-12154` peut indiquer que la connexion Oracle n'a pas pu résoudre son adresse.
Ne pas recréer les tables pour résoudre un problème de réseau.

## 2. Vérifier qu'une connexion est nécessaire

```powershell
curl.exe -i http://localhost:5080/api/films
```

Résultat attendu : `401 Unauthorized`.

## 3. Se connecter

Utiliser le courriel et le mot de passe d'un client déjà présent dans les données du labo 1.
Le compte Oracle d'équipe sert à la connexion à la base; ce n'est pas le compte client de l'application.

```powershell
$base = 'http://localhost:5080'
$courriel = Read-Host 'Courriel du client'
$secret = Read-Host 'Mot de passe du client' -AsSecureString
$corps = @{
    courriel = $courriel
    motDePasse = [System.Net.NetworkCredential]::new('', $secret).Password
} | ConvertTo-Json

$utilisateur = Invoke-RestMethod -Method Post -Uri "$base/api/auth/connexion" `
    -ContentType 'application/json' -Body $corps -SessionVariable session
$utilisateur | Format-List
```

Résultat attendu : l'identité de l'utilisateur, sans mot de passe. La variable `$session`
conserve le cookie pour les requêtes suivantes. Un mauvais courriel ou mot de passe retourne `401`.

Consulter le profil et vérifier son forfait :

```powershell
$profil = Invoke-RestMethod -Uri "$base/api/clients/$($utilisateur.idUtilisateur)" -WebSession $session
$profil | ConvertTo-Json -Depth 5
```

Résultat attendu : les renseignements du client et son forfait. Le numéro de carte est masqué et le CVV est absent.

## 4. Rechercher et consulter

```powershell
$films = @(Invoke-RestMethod -Uri "$base/api/films?titre=star" -WebSession $session)
$films | Format-Table idFilm, titre, anneeSortie
```

Si les données ne contiennent pas ce titre, omettre le filtre ou choisir un mot d'un titre existant.
Pour un titre contenant « Star Wars », les requêtes `titre=star%20wars` et `titre=wars%20star`
doivent retourner les mêmes films.

Combiner des filtres, en prenant des valeurs réellement présentes dans la base :

```powershell
Invoke-RestMethod -Uri "$base/api/films?acteur=ford&anneeMin=1970&anneeMax=2000" -WebSession $session
```

Choisir un identifiant dans les résultats, puis ouvrir la fiche :

```powershell
$idFilm = [int](Read-Host 'Identifiant du film à consulter')
$film = Invoke-RestMethod -Uri "$base/api/films/$idFilm" -WebSession $session
$film | ConvertTo-Json -Depth 6

if ($null -ne $film.realisateur) {
    Invoke-RestMethod -Uri "$base/api/personnes/$($film.realisateur.idPersonne)" -WebSession $session
}
```

Résultat attendu : la fiche du film et ses associations, puis les renseignements du réalisateur s'il est connu.
Chaque acteur et scénariste possède aussi un `idPersonne` utilisable avec la même route.

## 5. Louer un film

Cette commande enregistre une location réelle pour le client connecté. Choisir un client
dont le quota permet encore une location et un film possédant une copie disponible.

```powershell
$demande = @{ idFilm = $idFilm } | ConvertTo-Json
$location = Invoke-RestMethod -Method Post -Uri "$base/api/locations" `
    -ContentType 'application/json' -Body $demande -WebSession $session
$location | Format-List

Invoke-RestMethod -Uri "$base/api/locations" -WebSession $session
```

Résultat attendu : `201 Created`, une copie choisie automatiquement et un identifiant de location.
La nouvelle location apparaît dans les locations en cours. L'API ne reçoit aucun identifiant de client dans le corps de la demande.

Pour le forfait D, la date prévue est 10 jours après la location; pour I, 30 jours; pour A, elle est absente.
La durée illimitée du forfait A ne supprime pas sa limite de 10 locations simultanées.

## 6. Montrer les refus attendus

Avec un client au forfait D ayant déjà une location en cours, une nouvelle demande doit retourner `409`.
La location refusée ne doit pas apparaître dans la liste.

```powershell
Invoke-RestMethod -Method Post -Uri "$base/api/locations" `
    -ContentType 'application/json' -Body $demande -WebSession $session
```

Autres cas à montrer selon les données disponibles :

| Cas | Résultat attendu |
| --- | --- |
| Intervalle `anneeMin=2000&anneeMax=1900` | `400` |
| Recherche d'un titre absent | `200` et liste vide |
| Fiche d'un identifiant inexistant | `404` |
| Location avec `idFilm=0` | `400` |
| Film dont toutes les copies sont louées | `409` |
| Profil d'un autre client | `403` |

Le retour d'une copie relève d'un autre cas d'utilisation. Il n'est pas ajouté aux routes du labo 2.

## 7. Fermer la session

```powershell
Invoke-RestMethod -Method Post -Uri "$base/api/auth/deconnexion" -WebSession $session
Invoke-RestMethod -Uri "$base/api/films" -WebSession $session
```

Résultat attendu : déconnexion avec `204`, puis `401` pour la recherche suivante.

## Points à expliquer pendant la présentation

1. Le contrôleur reçoit une requête HTTP et délègue à un service.
2. Le service joue le rôle de façade et applique les règles métier.
3. Le repository utilise EF Core pour lire ou modifier les données.
4. Le mappage relie les classes C# aux tables et associations du labo 1.
5. La session identifie le client; les DTO évitent de retourner les entités et leurs données sensibles.
6. La transaction et les verrous protègent l'attribution des copies et le quota.

Pour illustrer le code, suivre `LocationsController.Louer`, puis `LocationService.LouerAsync`
et enfin `LocationRepository`. Les règles principales sont regroupées dans le service.
