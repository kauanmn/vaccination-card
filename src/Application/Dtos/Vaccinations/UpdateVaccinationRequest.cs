using System.Text.Json.Serialization;

namespace Application.Dtos.Vaccinations;

public record UpdateVaccinationRequest
{
    [JsonPropertyName("applicationDate")]
    public DateOnly ApplicationDate { get; init; }
}
