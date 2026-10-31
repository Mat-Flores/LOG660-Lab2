using AutoMapper;
using Lab2.Api.Dtos;
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

    // Le format du mot de passe reste celui des données du labo 1.
    public async Task<ClientDto?> AuthentifierAsync(string courriel, string motDePasse, CancellationToken ct = default)
    {
        var client = await _repository.GetByCourrielAsync(courriel.Trim(), ct);
        return client is null || !string.Equals(client.MotDePasse, motDePasse, StringComparison.Ordinal)
            ? null
            : _mapper.Map<ClientDto>(client);
    }
}
