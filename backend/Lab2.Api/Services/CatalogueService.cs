using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Models;
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

    public async Task<IReadOnlyList<FilmResumeDto>> RechercherFilmsAsync(RechercheFilms recherche, CancellationToken ct = default)
    {
        var films = await _repository.RechercherFilmsAsync(recherche, ct);
        return _mapper.Map<List<FilmResumeDto>>(films);
    }

    public async Task<FilmDto?> GetFilmByIdAsync(int id, CancellationToken ct = default)
    {
        var film = await _repository.GetFilmByIdAsync(id, ct);
        return film is null ? null : _mapper.Map<FilmDto>(film);
    }

    public async Task<PersonneDto?> GetPersonneByIdAsync(int id, CancellationToken ct = default)
    {
        var personne = await _repository.GetPersonneByIdAsync(id, ct);
        return personne is null ? null : _mapper.Map<PersonneDto>(personne);
    }
}
