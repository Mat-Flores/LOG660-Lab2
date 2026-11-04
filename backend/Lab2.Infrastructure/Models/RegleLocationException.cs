namespace Lab2.Infrastructure.Models;

public enum MotifRefusLocation
{
    NonClient,
    FilmIntrouvable,
    ForfaitInvalide,
    QuotaAtteint,
    AucuneCopie,
    DemandeConcurrente
}

public class RegleLocationException : Exception
{
    public MotifRefusLocation Motif { get; }

    public RegleLocationException(MotifRefusLocation motif, string message) : base(message)
        => Motif = motif;
}
