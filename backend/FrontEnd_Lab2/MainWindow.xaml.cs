
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Webflix
{
    public partial class MainWindow : Window
    {
        // Adresse du backend de tes coéquipiers.
        private static readonly HttpClient Api = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5080/api/")
        };

        private readonly System.Text.Json.JsonSerializerOptions _json =
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        private ClientDto? _client;
        private FilmResumeDto? _selectedFilm;
        private FilmDetailDto? _filmDetail;

        private List<FilmResumeDto> _films = new List<FilmResumeDto>();
        private bool _criteriaLoaded;

        public MainWindow()
        {
            InitializeComponent();

            FilmList.ItemsSource = _films;

            ShowView(LoginView);
        }

        // --------------------------------------------------
        // NAVIGATION ENTRE LES ÉCRANS
        // --------------------------------------------------

        private void ShowView(UIElement view)
        {
            LoginView.Visibility = Visibility.Collapsed;
            SearchView.Visibility = Visibility.Collapsed;
            ConsultView.Visibility = Visibility.Collapsed;
            RentalView.Visibility = Visibility.Collapsed;
            ActorView.Visibility = Visibility.Collapsed;

            view.Visibility = Visibility.Visible;
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            ShowView(LoginView);
        }

        private async void ShowSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null)
            {
                ShowView(LoginView);
                LoginMessage.Text =
                    "Veuillez vous connecter avant d'accéder au catalogue.";
                return;
            }

            ShowView(SearchView);

            await LoadCriteriaAsync();
            await SearchFilmsAsync();
        }

        // --------------------------------------------------
        // CU1 : CONNEXION
        // POST /api/auth/login
        // --------------------------------------------------

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginMessage.Text = "";

            if (string.IsNullOrWhiteSpace(UsernameBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                LoginMessage.Text =
                    "Veuillez entrer votre courriel et votre mot de passe.";
                return;
            }

            try
            {
                var demande = new
                {
                    courriel = UsernameBox.Text.Trim(),
                    motDePasse = PasswordBox.Password
                };

                using HttpResponseMessage response =
                    await Api.PostAsJsonAsync("auth/login", demande);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LoginMessage.Text = await ApiError(response);
                    return;
                }

                response.EnsureSuccessStatusCode();

                _client = await response.Content
                    .ReadFromJsonAsync<ClientDto>(_json);

                if (_client == null)
                {
                    LoginMessage.Text =
                        "La réponse du serveur est invalide.";
                    return;
                }

                PasswordBox.Clear();

                ShowView(SearchView);

                await LoadCriteriaAsync();
                await SearchFilmsAsync();
            }
            catch (HttpRequestException ex)
            {
                LoginMessage.Text =
                    "Impossible de joindre le backend.\n" +
                    "Vérifiez que le serveur fonctionne sur " +
                    "http://localhost:5080.\n" + ex.Message;
            }
            catch (Exception ex)
            {
                LoginMessage.Text =
                    "Erreur pendant la connexion : " + ex.Message;
            }
        }

        // --------------------------------------------------
        // CHARGEMENT DES CRITÈRES DE RECHERCHE
        // GET /api/films/criteres
        // --------------------------------------------------

        private async Task LoadCriteriaAsync()
        {
            if (_criteriaLoaded)
                return;

            try
            {
                CriteresRechercheDto? criteres =
                    await Api.GetFromJsonAsync<CriteresRechercheDto>(
                        "films/criteres", _json);

                if (criteres == null)
                    return;

                FillCombo(CountryFilter, "Tous", criteres.Pays);
                FillCombo(GenreFilter, "Tous", criteres.Genres);
                FillCombo(LanguageFilter, "Toutes", criteres.Langues);

                _criteriaLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Impossible de charger les critères : " + ex.Message,
                    "Webflix",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static void FillCombo(
            ComboBox combo,
            string allLabel,
            IEnumerable<string> values)
        {
            combo.Items.Clear();

            combo.Items.Add(new ComboBoxItem
            {
                Content = allLabel
            });

            foreach (string value in values.Distinct().OrderBy(x => x))
            {
                combo.Items.Add(new ComboBoxItem
                {
                    Content = value
                });
            }

            combo.SelectedIndex = 0;
        }

        // --------------------------------------------------
        // CU2 : RECHERCHE DE FILMS
        // GET /api/films
        // --------------------------------------------------

        private async void Search_Click(
            object sender,
            RoutedEventArgs e)
        {
            await SearchFilmsAsync();
        }

        private async Task SearchFilmsAsync()
        {
            try
            {
                List<string> parameters = new List<string>();

                AddParameter(parameters, "titre", TitleFilter.Text);
                AddParameter(parameters, "acteur", ActorFilter.Text);
                AddParameter(parameters, "realisateur", DirectorFilter.Text);

                string pays = SelectedOption(CountryFilter, "Tous");
                string genre = SelectedOption(GenreFilter, "Tous");
                string langue = SelectedOption(LanguageFilter, "Toutes");

                if (pays != "Tous")
                    AddParameter(parameters, "pays", pays);

                if (genre != "Tous")
                    AddParameter(parameters, "genre", genre);

                if (langue != "Toutes")
                    AddParameter(parameters, "langue", langue);

                int? yearFrom = ParseYear(YearFromFilter.Text);
                int? yearTo = ParseYear(YearToFilter.Text);

                if (!string.IsNullOrWhiteSpace(YearFromFilter.Text)
                    && !yearFrom.HasValue)
                {
                    MessageBox.Show("L'année minimale est invalide.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(YearToFilter.Text)
                    && !yearTo.HasValue)
                {
                    MessageBox.Show("L'année maximale est invalide.");
                    return;
                }

                if (yearFrom.HasValue &&
                    yearTo.HasValue &&
                    yearFrom.Value > yearTo.Value)
                {
                    MessageBox.Show(
                        "L'année minimale doit être inférieure " +
                        "ou égale à l'année maximale.");
                    return;
                }

                if (yearFrom.HasValue)
                {
                    AddParameter(
                        parameters,
                        "anneeMin",
                        yearFrom.Value.ToString(
                            CultureInfo.InvariantCulture));
                }

                if (yearTo.HasValue)
                {
                    AddParameter(
                        parameters,
                        "anneeMax",
                        yearTo.Value.ToString(
                            CultureInfo.InvariantCulture));
                }

                string url = "films";

                if (parameters.Count > 0)
                {
                    url += "?" + string.Join("&", parameters);
                }

                _films =
                    await Api.GetFromJsonAsync<List<FilmResumeDto>>(
                        url, _json) ?? new List<FilmResumeDto>();

                FilmList.ItemsSource = _films;
                FilmList.SelectedItem = null;

                _selectedFilm = null;
                _filmDetail = null;

                ResultCount.Text = _films.Count == 0
                    ? "Aucun film ne correspond aux critères."
                    : $"{_films.Count} film(s) trouvé(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur pendant la recherche : " + ex.Message +
                    "\nVérifiez que l'API et la connexion à la base de données fonctionnent.",
                    "Webflix",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static void AddParameter(
            List<string> parameters,
            string name,
            string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters.Add(
                    Uri.EscapeDataString(name) + "=" +
                    Uri.EscapeDataString(value.Trim()));
            }
        }

        private static string SelectedOption(
            ComboBox combo,
            string defaultValue)
        {
            if (combo.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? defaultValue;
            }

            return defaultValue;
        }

        private static int? ParseYear(string value)
        {
            return int.TryParse(value, out int year)
                ? year
                : null;
        }

        // Réinitialisation des filtres.

        private async void ClearFilters_Click(
            object sender,
            RoutedEventArgs e)
        {
            TitleFilter.Clear();
            ActorFilter.Clear();
            DirectorFilter.Clear();
            YearFromFilter.Clear();
            YearToFilter.Clear();

            CountryFilter.SelectedIndex = 0;
            GenreFilter.SelectedIndex = 0;
            LanguageFilter.SelectedIndex = 0;

            await SearchFilmsAsync();
        }

        private void FilmList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _selectedFilm = FilmList.SelectedItem as FilmResumeDto;
        }

        // --------------------------------------------------
        // CU3 : CONSULTATION DÉTAILLÉE D'UN FILM
        // GET /api/films/{id}
        // --------------------------------------------------

        private async void ConsultFilm_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_selectedFilm == null)
            {
                MessageBox.Show("Veuillez sélectionner un film.");
                return;
            }

            try
            {
                _filmDetail =
                    await Api.GetFromJsonAsync<FilmDetailDto>(
                        $"films/{_selectedFilm.IdFilm}", _json);

                if (_filmDetail == null)
                    return;

                FilmTitle.Text = _filmDetail.Titre;

                FilmYear.Text =
                    $"Année de sortie : {_filmDetail.AnneeSortie}";

                FilmDuration.Text =
                    $"Durée : {_filmDetail.DureeMinutes} minutes";

                FilmGenre.Text =
                    "Genres : " +
                    string.Join(", ", _filmDetail.Genres);

                FilmLanguage.Text =
                    "Pays : " +
                    string.Join(", ", _filmDetail.Pays) +
                    " | Langue originale : " +
                    (_filmDetail.LangueOriginale ?? "Non précisée");

                FilmSummary.Text =
                    string.IsNullOrWhiteSpace(_filmDetail.Resume)
                    ? "Aucun résumé disponible."
                    : _filmDetail.Resume;

                FilmWriters.Text =
                    "Scénaristes : " +
                    (_filmDetail.Scenaristes.Count == 0
                        ? "Non précisés"
                        : string.Join(", ",
                            _filmDetail.Scenaristes.Select(s => s.Nom)));

                FilmTrailers.ItemsSource = _filmDetail.BandesAnnonces;

                FilmActors.ItemsSource = _filmDetail.Acteurs;

                if (_filmDetail.Realisateur != null)
                {
                    FilmDirectorButton.Content =
                        "Consulter le réalisateur : " +
                        _filmDetail.Realisateur.Nom;

                    FilmDirectorButton.IsEnabled = true;
                }
                else
                {
                    FilmDirectorButton.Content =
                        "Réalisateur non précisé";

                    FilmDirectorButton.IsEnabled = false;
                }

                ShowView(ConsultView);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Impossible de charger les détails du film : " +
                    ex.Message);
            }
        }

        private void BackToSearch_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowView(SearchView);
        }

        // --------------------------------------------------
        // CU3 : CONSULTATION D'UN ACTEUR
        // GET /api/personnes/{id}
        // --------------------------------------------------

        private async void ConsultActor_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (FilmActors.SelectedItem is not ActeurDto actor)
            {
                MessageBox.Show("Veuillez sélectionner un acteur.");
                return;
            }

            await LoadPersonAsync(actor.IdPersonne, "Acteur");
        }

        // --------------------------------------------------
        // CU3 : CONSULTATION DU RÉALISATEUR
        // GET /api/personnes/{id}
        // --------------------------------------------------

        private async void ConsultDirector_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_filmDetail?.Realisateur == null)
                return;

            await LoadPersonAsync(
                _filmDetail.Realisateur.IdPersonne,
                "Réalisateur");
        }

        private async Task LoadPersonAsync(
            int personId,
            string role)
        {
            try
            {
                PersonneDto? person =
                    await Api.GetFromJsonAsync<PersonneDto>(
                        $"personnes/{personId}", _json);

                if (person == null)
                    return;

                ActorName.Text = person.Nom;
                ActorRole.Text = role;

                ActorBirthDate.Text =
                    "Date de naissance : " +
                    (person.DateNaissance?.ToString(
                        "dd MMMM yyyy",
                        CultureInfo.GetCultureInfo("fr-CA"))
                        ?? "Non précisée");

                ActorBirthPlace.Text =
                    "Lieu de naissance : " +
                    (person.LieuNaissance ?? "Non précisé");

                ActorBiography.Text =
                    person.Biographie ??
                    "Aucune biographie disponible.";

                // Le endpoint PersonneDto ne fournit pas
                // la filmographie complète.
                // On affiche donc le film depuis lequel
                // la fiche de la personne a été ouverte.

                ActorFilmAssociation.Text =
                    _filmDetail == null
                    ? ""
                    : "Film associé à cette consultation : " +
                      _filmDetail.Titre;

                ShowView(ActorView);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Impossible de charger la fiche de la personne : " +
                    ex.Message);
            }
        }

        private void BackFromActor_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowView(ConsultView);
        }

        // --------------------------------------------------
        // CU4 : LOCATION D'UN FILM
        // POST /api/locations
        // --------------------------------------------------

        private void Rent_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_client == null || _filmDetail == null)
                return;

            RentalFilmTitle.Text = _filmDetail.Titre;

            RentalMessage.Text =
                "Voulez-vous confirmer la location de ce film ?";

            ShowView(RentalView);
        }

        private async void ConfirmRental_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_client == null || _filmDetail == null)
                return;

            try
            {
                var demande = new
                {
                    idUtilisateur = _client.IdUtilisateur,
                    idFilm = _filmDetail.IdFilm
                };

                using HttpResponseMessage response =
                    await Api.PostAsJsonAsync("locations", demande);

                if (!response.IsSuccessStatusCode)
                {
                    RentalMessage.Text = await ApiError(response);
                    return;
                }

                LocationDto? location =
                    await response.Content
                        .ReadFromJsonAsync<LocationDto>(_json);

                RentalMessage.Text = location == null
                    ? "Location confirmée."
                    : $"Location confirmée. Numéro : {location.IdLocation}.";

                MessageBox.Show(
                    RentalMessage.Text,
                    "Location",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ShowView(ConsultView);
            }
            catch (Exception ex)
            {
                RentalMessage.Text =
                    "Erreur lors de la location : " + ex.Message;
            }
        }

        private void BackToConsult_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowView(ConsultView);
        }

        // --------------------------------------------------
        // GESTION DES ERREURS HTTP
        // Le backend renvoie ProblemDetails.
        // Le message utile se trouve dans Detail.
        // --------------------------------------------------

        private async Task<string> ApiError(
            HttpResponseMessage response)
        {
            try
            {
                ProblemDto? problem =
                    await response.Content
                        .ReadFromJsonAsync<ProblemDto>(_json);

                return problem?.Detail
                    ?? problem?.Title
                    ?? $"Erreur HTTP {(int)response.StatusCode}.";
            }
            catch
            {
                return $"Erreur HTTP {(int)response.StatusCode}.";
            }
        }
    }

    // ==================================================
    // DTOs DU BACKEND
    // ==================================================

    public class ClientDto
    {
        public int IdUtilisateur { get; set; }
        public string NomFamille { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Courriel { get; set; } = "";
    }

    public class CriteresRechercheDto
    {
        public List<string> Genres { get; set; } = new();
        public List<string> Pays { get; set; } = new();
        public List<string> Langues { get; set; } = new();
    }

    public class FilmResumeDto
    {
        public int IdFilm { get; set; }
        public string Titre { get; set; } = "";
        public int AnneeSortie { get; set; }
        public string? UrlAffiche { get; set; }
    }

    public class FilmDetailDto
    {
        public int IdFilm { get; set; }
        public string Titre { get; set; } = "";
        public int AnneeSortie { get; set; }

        public List<string> Pays { get; set; } = new();

        public string? LangueOriginale { get; set; }
        public int DureeMinutes { get; set; }

        public List<string> Genres { get; set; } = new();

        public PersonneResumeDto? Realisateur { get; set; }

        public List<PersonneResumeDto> Scenaristes { get; set; } = new();

        public List<ActeurDto> Acteurs { get; set; } = new();

        public string Resume { get; set; } = "";

        public string? UrlAffiche { get; set; }

        public List<string> BandesAnnonces { get; set; } = new();
    }

    public class ActeurDto
    {
        public int IdPersonne { get; set; }
        public string Nom { get; set; } = "";
        public List<string> Personnages { get; set; } = new();
    }

    public class PersonneResumeDto
    {
        public int IdPersonne { get; set; }
        public string Nom { get; set; } = "";
    }

    public class PersonneDto
    {
        public int IdPersonne { get; set; }
        public string Nom { get; set; } = "";
        public DateTime? DateNaissance { get; set; }
        public string? LieuNaissance { get; set; }
        public string? Photo { get; set; }
        public string? Biographie { get; set; }
    }

    public class LocationDto
    {
        public int IdLocation { get; set; }
        public int IdFilm { get; set; }
        public string CodeCopie { get; set; } = "";
        public DateTime DateLocation { get; set; }
    }

    public class ProblemDto
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
    }
}
