using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Entities;

namespace Lab2.Api.Mapping;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        CreateMap<Forfait, ForfaitDto>();

        CreateMap<Adresse, AdresseDto>();

        CreateMap<CarteCredit, CarteCreditDto>()
            .ForMember(d => d.NumeroMasque, o => o.MapFrom(s => MasquerNumero(s.Numero)));

        CreateMap<Client, ClientDto>();
    }

    internal static string MasquerNumero(string? numero)
    {
        if (string.IsNullOrEmpty(numero))
            return string.Empty;

        return numero.Length <= 4
            ? new string('*', numero.Length)
            : new string('*', numero.Length - 4) + numero[^4..];
    }
}
