using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Repositories;
using Lab2.Infrastructure.Repositories.Interfaces;

namespace Lab2.Api.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly IMapper _mapper;

    public ClientService(IClientRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ClientDto?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(idUtilisateur, ct);
        return client is null ? null : _mapper.Map<ClientDto>(client);
    }

    // shortcut: mot de passe en clair comme au labo 1 ; à hacher si la BD le permet un jour.
    public async Task<ClientDto?> AuthentifierAsync(string courriel, string motDePasse, CancellationToken ct = default)
    {
        var client = await _repository.GetByCourrielAsync(courriel.Trim(), ct);
        return client is null || client.Utilisateur.MotDePasse != motDePasse
            ? null
            : _mapper.Map<ClientDto>(client);
    }
}
