using System.Text.Json.Serialization;

namespace Application.Dtos.Auth;

public record LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}
