using AutoMapper;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Entities;

namespace Lab2.Api.Mapping;

public class CatalogueProfile : Profile
{
    public CatalogueProfile()
    {
        CreateMap<Film, FilmResumeDto>();
        CreateMap<Personne, PersonneResumeDto>();
        CreateMap<Personne, PersonneDto>();
        CreateMap<Interpretation, InterpretationDto>()
            .ForMember(d => d.Nom, o => o.MapFrom(s => s.Personne.Nom));

        CreateMap<Film, FilmDto>()
            .ForMember(d => d.Acteurs, o => o.MapFrom(s => s.Interpretations.OrderBy(i => i.Personne.Nom)))
            .ForMember(d => d.Scenaristes, o => o.MapFrom(s => s.Scenaristes
                .Select(x => x.Personne).OrderBy(p => p.Nom)))
            .ForMember(d => d.Genres, o => o.MapFrom(s => s.FilmGenres.Select(g => g.NomGenre).OrderBy(n => n)))
            .ForMember(d => d.Pays, o => o.MapFrom(s => s.FilmPays.Select(p => p.NomPays).OrderBy(n => n)))
            .ForMember(d => d.BandesAnnonces, o => o.MapFrom(s => s.BandesAnnonces.Select(b => b.Lien).OrderBy(l => l)));
    }
}
