using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Repositories;

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
}
