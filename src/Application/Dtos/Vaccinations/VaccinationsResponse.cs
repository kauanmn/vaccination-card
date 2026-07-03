using System.Text.Json.Serialization;

namespace Application.Dtos.Vaccinations;

public record VaccinationResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("vaccineId")]
    public Guid VaccineId { get; init; }

    [JsonPropertyName("dose")]
    public int Dose { get; init; }

    [JsonPropertyName("applicationDate")]
    public DateOnly ApplicationDate { get; init; }
}