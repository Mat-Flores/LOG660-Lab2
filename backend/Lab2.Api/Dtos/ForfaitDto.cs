namespace Lab2.Api.Dtos;

public class ForfaitDto
{
    public string Code { get; set; } = string.Empty;
    public decimal Cout { get; set; }
    public int LocationsMax { get; set; }
    public int? DureeMaxJours { get; set; }   // null = illimité
}
