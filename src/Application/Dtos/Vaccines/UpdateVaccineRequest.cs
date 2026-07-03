using System.Text.Json.Serialization;

namespace Application.Dtos.Vaccines;

public record UpdateVaccineRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("totalDoses")]
    public int? TotalDoses { get; init; }
}
