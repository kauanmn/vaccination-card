namespace Domain.Entities;

public class Vaccine
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required int TotalDoses { get; set; }
}