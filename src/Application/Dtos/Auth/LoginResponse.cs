using System.Text.Json.Serialization;

namespace Application.Dtos.Auth;

public record LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; init; }
}
