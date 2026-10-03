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

        // Client = Utilisateur (infos personnelles) + spécialisation : on aplatit.
        CreateMap<Client, ClientDto>()
            .ForMember(d => d.NomFamille, o => o.MapFrom(s => s.Utilisateur.NomFamille))
            .ForMember(d => d.Prenom, o => o.MapFrom(s => s.Utilisateur.Prenom))
            .ForMember(d => d.Courriel, o => o.MapFrom(s => s.Utilisateur.Courriel))
            .ForMember(d => d.Telephone, o => o.MapFrom(s => s.Utilisateur.Telephone))
            .ForMember(d => d.DateNaissance, o => o.MapFrom(s => s.Utilisateur.DateNaissance))
            .ForMember(d => d.Adresse, o => o.MapFrom(s => s.Utilisateur.Adresse));
    }

    internal static string MasquerNumero(string numero)
        => numero.Length <= 4
            ? new string('*', numero.Length)
            : new string('*', numero.Length - 4) + numero[^4..];
}
