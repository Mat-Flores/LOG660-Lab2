# Conception du backend

## Périmètre

Le backend couvre les cas 2, 3 et 4 : connexion, consultation interactive et location.
Il prolonge les deux projets .NET existants et utilise le schéma Oracle du labo 1.
Le client mince est développé séparément dans `frontend/`.

## Répartition des responsabilités

| Élément | Responsabilité |
| --- | --- |
| Contrôleurs | Lire la requête, récupérer l'identité connectée et choisir la réponse HTTP. |
| `AuthentificationService` | Vérifier les identifiants d'un utilisateur existant. |
| `CatalogueService` | Rechercher les films et consulter les fiches des films et des personnes. |
| `LocationService` | Vérifier les conditions d'une location et gérer sa transaction. |
| `ClientService` | Consulter le profil du client. |
| Repositories | Exécuter les requêtes et écritures sur le contexte EF Core. |
| `AppDbContext` et configurations | Décrire les tables, colonnes, clés et associations Oracle. |
| DTO et profils AutoMapper | Construire les données utiles au client sans sérialiser les entités. |

Les services forment les façades de l'application. Les contrôleurs ne manipulent pas le contexte de données.
Le projet Infrastructure ne dépend pas du projet Api.

## Connexion

Le repository recherche l'utilisateur par courriel avec une requête paramétrée.
Le service compare le mot de passe en respectant le format déjà stocké au labo 1.
Un échec retourne le même message pour un courriel inconnu ou un mauvais mot de passe.
Après une réussite, le contrôleur émet un cookie contenant l'identifiant de l'utilisateur.
Les opérations suivantes utilisent cette identité pour consulter son profil ou louer un film.

## Consultation

Les critères de recherche sont facultatifs et se combinent avec un ET logique.
Pour les critères textuels libres, chaque mot est recherché comme une partie du titre ou du nom,
indépendamment de sa position et des majuscules. Tous les mots d'une recherche d'acteur
doivent appartenir à une même personne.

La liste retourne seulement les renseignements de sélection. La fiche détaillée contient
le réalisateur, les interprétations, les scénaristes, les genres, les pays et les bandes-annonces.
Une fiche de personne donne accès à sa biographie et à ses renseignements personnels publics.

Les lectures utilisent `AsNoTracking`. Les collections de la fiche de film sont chargées
avec `AsSplitQuery` pour éviter leur multiplication dans une seule jointure.

## Déroulement d'une location

1. Le contrôleur récupère le client depuis la session et le film depuis la demande.
2. Le service ouvre une transaction et demande le verrouillage du client.
3. Il vérifie que le compte est un client avec un forfait valide.
4. Il verrouille le film et vérifie son existence.
5. Il compte les locations dont `dateRetour` est absente et vérifie le quota.
6. Il sélectionne une copie sans location en cours.
7. Il crée une entité `Location` et demande sa sauvegarde par EF Core.
8. Il valide la transaction et retourne le résultat avec l'identifiant généré par Oracle.

Une erreur avant la validation annule la transaction. Les appels de location prennent
les verrous dans le même ordre : client, puis film. Le premier protège le quota entre
plusieurs films; le second protège la sélection des copies entre plusieurs clients de l'API.
L'attente d'un verrou Oracle est limitée à cinq secondes. Un conflit attendu retourne `409`.

La disponibilité se déduit de `Location.dateRetour`; aucun nouvel état n'est stocké dans `Copie`.
La durée maximale du forfait sert à calculer la date prévue, sans modifier le schéma.
Les triggers du labo 1 restent une validation supplémentaire lors de l'insertion.

## Mappage objet-relationnel

Les 17 entités correspondent aux 17 tables existantes. Les noms sont forcés en majuscules
car Oracle stocke ainsi les identifiants SQL créés sans guillemets.

| Élément | Mappage |
| --- | --- |
| Chaînes | `VARCHAR2`, avec les longueurs du schéma. |
| Dates | `DATE`; type C# nullable lorsque la colonne accepte une valeur absente. |
| Biographie | `CLOB`. |
| Coût du forfait | `NUMBER(5,2)` et `decimal`. |
| Utilisateur, film et personne | Identifiants existants, sans génération automatique à l'insertion. |
| Location | Identifiant fourni par la colonne Oracle identity. |
| Tables associatives | Clés composées conservées. |

`Client` et `Employe` sont reliés à `Utilisateur` par une association un-à-un dont la clé
primaire est aussi une clé étrangère. Ce mappage reprend les tables de spécialisation existantes.
`Adresse` et `CarteCredit` utilisent également une clé primaire partagée avec leur propriétaire.

Les suppressions en cascade sont limitées aux relations de composition décrites dans le schéma.
Les relations de `Location` restent restrictives pour conserver l'historique.
L'application n'exécute aucune migration ni création de tables au démarrage.

## Éléments pour le rapport du laboratoire

Cette note fournit les choix de conception de la partie backend. Le rapport final doit
les intégrer au gabarit du cours, avec la présentation du client mince, la répartition réelle
du travail et les éléments propres à l'équipe. Ces informations ne sont pas déduites du code.
