using System.Text.Json.Serialization;
using Application.Dtos.Vaccinations;

namespace Application.Dtos.Patients;

public record PatientResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("vaccinations")] public IReadOnlyCollection<VaccinationResponse> Vaccinations { get; init; } = [];
}