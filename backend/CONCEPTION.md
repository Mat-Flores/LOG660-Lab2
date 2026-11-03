# Notes sur le backend

## Organisation

On garde les deux projets existants :

- `Lab2.Api` : contrôleurs, services, DTO et profils AutoMapper.
- `Lab2.Infrastructure` : entités, mappage EF Core et repositories Oracle.

Les services servent de façade pour les cas 2, 3 et 4. Les contrôleurs appellent un
service ; les requêtes passent par les repositories. Les entités restent côté serveur,
et les DTO définissent les données échangées avec le frontend.

## Mappage du labo 1

On utilise les 17 tables du labo 1 sans migration ni modification des scripts SQL.
Les annotations des entités et les fichiers `Data/Configurations` décrivent le mappage.

| Élément | Mappage |
| --- | --- |
| Tables et colonnes | Noms en majuscules, comme dans Oracle. |
| Chaînes | `VARCHAR2`, avec les longueurs du schéma. |
| Dates | `DATE`, nullable si la colonne l'est. |
| Biographie | `CLOB`. |
| Coût du forfait | `decimal`, précision 5 et échelle 2. |
| Identifiant d'une location | Généré par l'identité Oracle. |
| Autres identifiants | Valeurs existantes, sans génération automatique par EF. |
| Tables d'association | Clés composées du labo 1 conservées. |

`Client` et `Employe` héritent de `Utilisateur`. Le mappage TPT utilise une table
par type : les informations communes sont dans `UTILISATEUR`, et les informations
spécifiques dans `CLIENT` ou `EMPLOYE`. Leurs clés primaires sont aussi des clés
étrangères vers `UTILISATEUR`. Cela correspond aux tables existantes, sans colonne
de discriminateur à ajouter.

`Adresse` et `CarteCredit` sont des associations un-à-un avec une clé partagée.
Les acteurs, réalisateurs et scénaristes sont des rôles de `Personne`, représentés
par les associations avec les films. `INTERPRETATION` conserve le nom du personnage
dans sa clé composée pour permettre plusieurs rôles à un même acteur.

Les propriétés nullables permettent de lire les valeurs absentes du labo 1,
y compris les données anciennes conservées avec `ENABLE NOVALIDATE`.
Les cascades et les suppressions restrictives suivent les clés étrangères du SQL.

## Connexion

`ClientService` recherche un client par courriel et compare son mot de passe au
format stocké au labo 1. Le même message est retourné si le courriel est inconnu
ou si le mot de passe est incorrect. Un compte uniquement employé est refusé.

Le JSON de réponse reste un `ClientDto` : profil, adresse, forfait et carte masquée.
Le mot de passe, le CVV et le numéro complet de carte ne sont pas retournés.

Le jeton protégé par ASP.NET Core est renvoyé dans l'en-tête `X-Auth-Token`.
Il contient l'identifiant du client et expire après 30 minutes. Pour louer,
il faut l'envoyer dans `Authorization: Bearer ...`. Le serveur vérifie aussi
que `idUtilisateur` dans le JSON correspond au client connecté.

Le catalogue est public. Le frontend doit encore récupérer et transmettre le jeton
pour les locations. CORS expose `X-Auth-Token` et autorise l'en-tête `Authorization`.
On n'utilise pas de cookie.

## Recherche et fiches

Les critères se combinent avec un ET. Les mots partiels d'un titre ou d'un nom
peuvent être dans n'importe quel ordre, sans distinction de majuscules.
Tous les mots d'un critère acteur doivent correspondre à une même personne.
Avec `acteur=ford&acteur=hamill`, le film doit contenir les deux acteurs recherchés.

Les bornes d'année sont inclusives. Pays, genres et langue utilisent des valeurs
complètes. Les listes proposées viennent des tables de référence et des langues
distinctes des films. Les résultats sont triés par titre puis année.

La fiche regroupe les personnages joués par un même acteur. Les identifiants des
personnes permettent d'ouvrir leurs biographies avec `/api/personnes/{id}`.
Les lectures utilisent `AsNoTracking`. Pour la fiche d'un film, `AsSplitQuery`
évite de multiplier les lignes en joignant toutes les collections à la fois.

## Location

`LocationService` regroupe les étapes dans une transaction :

1. Verrouiller le client et vérifier son forfait.
2. Verrouiller le film et vérifier son existence.
3. Compter les locations actives et vérifier le quota.
4. Choisir une copie disponible.
5. Insérer la location avec EF Core, puis faire le commit.

Les verrous sont toujours pris dans l'ordre client, puis film. Le premier protège
le quota entre les demandes de l'API ; le second protège le choix des copies.
Le SQL paramétré `FOR UPDATE WAIT 5` limite l'attente à cinq secondes.
Une erreur avant le commit annule la transaction.

Une location est active si `DATERETOUR IS NULL`. Les limites viennent du forfait :
D = 1, I = 5, A = 10. Les triggers du labo 1 restent actifs ; les refus de quota
ou de disponibilité sont renvoyés en `409`.

Les verrous coordonnent les demandes passant par cette API. Les écritures lancées
directement en SQL ne suivent pas forcément le même ordre de verrouillage.
Les essais de concurrence et des triggers se font sur Oracle : SQLite ne les reproduit pas.

Pour configurer et tester le projet : [DEMONSTRATION.md](DEMONSTRATION.md).
