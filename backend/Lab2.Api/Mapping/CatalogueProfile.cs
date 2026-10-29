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

        // Les tables d'association (FILMPAYS, FILMGENRE, ...) sont aplaties en listes simples.
        CreateMap<Film, FilmDetailDto>()
            .ForMember(d => d.Pays, o => o.MapFrom(s => s.FilmPays.Select(p => p.NomPays).OrderBy(n => n)))
            .ForMember(d => d.Genres, o => o.MapFrom(s => s.FilmGenres.Select(g => g.NomGenre).OrderBy(n => n)))
            .ForMember(d => d.Scenaristes, o => o.MapFrom(s => s.Scenaristes.Select(x => x.Personne)))
            .ForMember(d => d.Acteurs, o => o.MapFrom(s => s.Interpretations
                .GroupBy(i => i.IdPersonne)
                .Select(g => new ActeurDto
                {
                    IdPersonne = g.Key,
                    Nom = g.First().Personne.Nom,
                    Personnages = g.Select(i => i.NomPersonnage).ToList()
                })))
            .ForMember(d => d.BandesAnnonces, o => o.MapFrom(s => s.BandesAnnonces.Select(b => b.Lien)))
            .ForMember(d => d.CopiesDisponibles, o => o.Ignore());
    }
}
