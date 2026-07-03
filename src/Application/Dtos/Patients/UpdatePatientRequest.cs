using System.Text.Json.Serialization;

namespace Application.Dtos.Patients;

public record UpdatePatientRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
