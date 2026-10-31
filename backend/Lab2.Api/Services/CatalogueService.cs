using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Repositories;
using Lab2.Infrastructure.Repositories.Interfaces;

namespace Lab2.Api.Services;

public class CatalogueService : ICatalogueService
{
    private readonly ICatalogueRepository _repository;
    private readonly IMapper _mapper;

    public CatalogueService(ICatalogueRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<FilmResumeDto>> RechercherFilmsAsync(RechercheFilmCriteres criteres, CancellationToken ct = default)
        => _mapper.Map<List<FilmResumeDto>>(await _repository.RechercherFilmsAsync(criteres, ct));

    public async Task<FilmDetailDto?> GetFilmAsync(int idFilm, CancellationToken ct = default)
    {
        var film = await _repository.GetFilmAsync(idFilm, ct);
        return film is null ? null : _mapper.Map<FilmDetailDto>(film);
    }

    public async Task<PersonneDto?> GetPersonneAsync(int idPersonne, CancellationToken ct = default)
    {
        var personne = await _repository.GetPersonneAsync(idPersonne, ct);
        return personne is null ? null : _mapper.Map<PersonneDto>(personne);
    }

    public async Task<CriteresRechercheDto> GetCriteresAsync(CancellationToken ct = default)
        => new()
        {
            Genres = await _repository.GetGenresAsync(ct),
            Pays = await _repository.GetPaysAsync(ct),
            Langues = await _repository.GetLanguesAsync(ct)
        };
}
