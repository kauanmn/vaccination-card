using System.Text.Json.Serialization;

namespace Application.Dtos.Vaccines;

public record VaccineResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("totalDoses")]
    public int? TotalDoses { get; init; }
}
