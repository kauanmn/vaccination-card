using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ApiTest;

public class EndpointsIntegrationTests : IClassFixture<VaccinationApiFactory>
{
    private readonly HttpClient _client;

    public EndpointsIntegrationTests(VaccinationApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateVaccine_ReturnsWrappedSuccessEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/vaccines",
            new { name = "COVID-19", totalDoses = 3 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal("COVID-19", body.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(3, body.GetProperty("data").GetProperty("totalDoses").GetInt32());
    }

    [Fact]
    public async Task CreateVaccine_WithInvalidPayload_ReturnsValidationEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/vaccines",
            new { name = "", totalDoses = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("error").GetProperty("code").GetString());
        Assert.True(body.GetProperty("error").GetProperty("details").GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetPatient_WhenMissing_ReturnsNotFoundEnvelope()
    {
        var response = await _client.GetAsync($"/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("NOT_FOUND", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task RegisterVaccination_OutOfOrder_ReturnsInvalidParametersEnvelope()
    {
        var vaccine = await (await _client.PostAsJsonAsync("/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccineId = vaccine.GetProperty("data").GetProperty("id").GetGuid();

        var patient = await (await _client.PostAsJsonAsync("/patients",
            new { name = "Kauan" })).Content.ReadFromJsonAsync<JsonElement>();
        var patientId = patient.GetProperty("data").GetProperty("id").GetGuid();

        var response = await _client.PostAsJsonAsync($"/patients/{patientId}/vaccinations",
            new { vaccineId, dose = 2, applicationDate = "2026-01-10" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("INVALID_PARAMETERS", body.GetProperty("error").GetProperty("code").GetString());
    }
}
