using System.Text.Json.Serialization;

namespace Application.Dtos.Vaccinations;

public record RegisterVaccinationRequest
{
    [JsonPropertyName("vaccineId")]
    public Guid VaccineId { get; init; }

    [JsonPropertyName("dose")]
    public int Dose { get; init; }

    [JsonPropertyName("applicationDate")]
    public DateOnly ApplicationDate { get; init; }
}
