using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ApiTest;

public class EndpointsIntegrationTests : IClassFixture<VaccinationApiFactory>
{
    private readonly VaccinationApiFactory _factory;

    public EndpointsIntegrationTests(VaccinationApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_AsAdmin_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { username = VaccinationApiFactory.AdminUsername, password = VaccinationApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal("Admin", body.GetProperty("data").GetProperty("role").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("data").GetProperty("token").GetString()));
    }

    [Fact]
    public async Task CreateVaccine_AsAdmin_ReturnsWrappedSuccessEnvelope()
    {
        var client = await AdminClientAsync();

        // Nome único: o seed já traz "COVID-19" e Vaccines.Name tem índice único.
        var name = $"Vac-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/vaccines",
            new { name, totalDoses = 3 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal(name, body.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(3, body.GetProperty("data").GetProperty("totalDoses").GetInt32());
    }

    [Fact]
    public async Task CreateVaccine_WithInvalidPayload_ReturnsValidationEnvelope()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/vaccines",
            new { name = "", totalDoses = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("error").GetProperty("code").GetString());
        Assert.True(body.GetProperty("error").GetProperty("details").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateVaccine_WithoutToken_ReturnsUnauthorizedEnvelope()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/vaccines",
            new { name = "COVID-19", totalDoses = 3 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("UNAUTHORIZED", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateVaccine_AsPatient_ReturnsForbiddenEnvelope()
    {
        var admin = await AdminClientAsync();
        var (_, username, password) = await CreatePatientAsync(admin, "Kauan");
        var patient = await AuthenticatedClientAsync(username, password);

        var response = await patient.PostAsJsonAsync("/api/vaccines",
            new { name = "COVID-19", totalDoses = 3 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("FORBIDDEN", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListVaccines_IsPublic_ReturnsPagedEnvelopeWithoutToken()
    {
        var admin = await AdminClientAsync();
        await admin.PostAsJsonAsync("/api/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 });

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vaccines?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());

        var data = body.GetProperty("data");
        Assert.Equal(1, data.GetProperty("page").GetInt32());
        Assert.Equal(10, data.GetProperty("pageSize").GetInt32());
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 1);
        Assert.True(data.GetProperty("items").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task GetPatient_WhenMissing_ReturnsNotFoundEnvelope()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync($"/api/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("NOT_FOUND", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task Patient_CanReadOwnCard_ButNotAnothersCard()
    {
        var admin = await AdminClientAsync();
        var (ownId, username, password) = await CreatePatientAsync(admin, "Kauan");
        var (otherId, _, _) = await CreatePatientAsync(admin, "Maria");

        var patient = await AuthenticatedClientAsync(username, password);

        var own = await patient.GetAsync($"/api/patients/{ownId}");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        var other = await patient.GetAsync($"/api/patients/{otherId}");
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);

        var body = await other.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FORBIDDEN", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task RegisterVaccination_OutOfOrder_ReturnsInvalidParametersEnvelope()
    {
        var client = await AdminClientAsync();

        var vaccine = await (await client.PostAsJsonAsync("/api/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccineId = vaccine.GetProperty("data").GetProperty("id").GetGuid();

        var (patientId, _, _) = await CreatePatientAsync(client, "Kauan");

        var response = await client.PostAsJsonAsync($"/api/patients/{patientId}/vaccinations",
            new { vaccineId, dose = 2, applicationDate = "2026-01-10" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("INVALID_PARAMETERS", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdateVaccine_AsAdmin_ReturnsUpdatedEnvelope()
    {
        var client = await AdminClientAsync();

        var created = await (await client.PostAsJsonAsync("/api/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccineId = created.GetProperty("data").GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonAsync($"/api/vaccines/{vaccineId}",
            new { name = "Vacina Renomeada", totalDoses = 4 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal("Vacina Renomeada", body.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(4, body.GetProperty("data").GetProperty("totalDoses").GetInt32());
    }

    [Fact]
    public async Task UpdateVaccine_AsPatient_ReturnsForbiddenEnvelope()
    {
        var admin = await AdminClientAsync();
        var (_, username, password) = await CreatePatientAsync(admin, "Kauan");
        var patient = await AuthenticatedClientAsync(username, password);

        var response = await patient.PatchAsJsonAsync($"/api/vaccines/{Guid.NewGuid()}",
            new { name = "X", totalDoses = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateVaccine_ReducingTotalDosesBelowAppliedDose_ReturnsInvalidParameters()
    {
        var client = await AdminClientAsync();

        var created = await (await client.PostAsJsonAsync("/api/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccineId = created.GetProperty("data").GetProperty("id").GetGuid();

        var (patientId, _, _) = await CreatePatientAsync(client, "Kauan");
        await client.PostAsJsonAsync($"/api/patients/{patientId}/vaccinations",
            new { vaccineId, dose = 1, applicationDate = "2026-01-10" });
        await client.PostAsJsonAsync($"/api/patients/{patientId}/vaccinations",
            new { vaccineId, dose = 2, applicationDate = "2026-01-11" });

        var response = await client.PatchAsJsonAsync($"/api/vaccines/{vaccineId}",
            new { name = "Vac", totalDoses = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INVALID_PARAMETERS", body.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdatePatient_OwnName_ReturnsUpdatedEnvelope_ButNotAnothers()
    {
        var admin = await AdminClientAsync();
        var (ownId, username, password) = await CreatePatientAsync(admin, "Kauan");
        var (otherId, _, _) = await CreatePatientAsync(admin, "Maria");

        var patient = await AuthenticatedClientAsync(username, password);

        var own = await patient.PatchAsJsonAsync($"/api/patients/{ownId}",
            new { name = "Kauan Manzato" });
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        var body = await own.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Kauan Manzato", body.GetProperty("data").GetProperty("name").GetString());

        var other = await patient.PatchAsJsonAsync($"/api/patients/{otherId}",
            new { name = "Hacker" });
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);
    }

    [Fact]
    public async Task UpdateVaccination_AsAdmin_ChangesApplicationDate()
    {
        var client = await AdminClientAsync();

        var created = await (await client.PostAsJsonAsync("/api/vaccines",
            new { name = $"Vac-{Guid.NewGuid():N}", totalDoses = 3 })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccineId = created.GetProperty("data").GetProperty("id").GetGuid();

        var (patientId, _, _) = await CreatePatientAsync(client, "Kauan");

        var registered = await (await client.PostAsJsonAsync($"/api/patients/{patientId}/vaccinations",
            new { vaccineId, dose = 1, applicationDate = "2026-01-10" })).Content.ReadFromJsonAsync<JsonElement>();
        var vaccinationId = registered.GetProperty("data").GetProperty("vaccinations")[0]
            .GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonAsync(
            $"/api/patients/{patientId}/vaccinations/{vaccinationId}",
            new { applicationDate = "2026-02-15" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("2026-02-15", body.GetProperty("data").GetProperty("vaccinations")[0]
            .GetProperty("applicationDate").GetString());
    }

    [Fact]
    public async Task UpdateVaccination_AsPatient_ReturnsForbiddenEnvelope()
    {
        var admin = await AdminClientAsync();
        var (patientId, username, password) = await CreatePatientAsync(admin, "Kauan");
        var patient = await AuthenticatedClientAsync(username, password);

        var response = await patient.PatchAsJsonAsync(
            $"/api/patients/{patientId}/vaccinations/{Guid.NewGuid()}",
            new { applicationDate = "2026-02-15" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateVaccination_WithFutureDate_ReturnsValidationError()
    {
        var client = await AdminClientAsync();
        var (patientId, _, _) = await CreatePatientAsync(client, "Kauan");

        var response = await client.PatchAsJsonAsync(
            $"/api/patients/{patientId}/vaccinations/{Guid.NewGuid()}",
            new { applicationDate = "2099-01-01" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("error").GetProperty("code").GetString());
    }

    private Task<HttpClient> AdminClientAsync() =>
        AuthenticatedClientAsync(VaccinationApiFactory.AdminUsername, VaccinationApiFactory.AdminPassword);

    private async Task<HttpClient> AuthenticatedClientAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, username, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetProperty("token").GetString()!;
    }

    private static async Task<(Guid Id, string Username, string Password)> CreatePatientAsync(
        HttpClient adminClient, string name)
    {
        var response = await adminClient.PostAsJsonAsync("/api/patients", new { name });
        response.EnsureSuccessStatusCode();

        var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        return (
            data.GetProperty("id").GetGuid(),
            data.GetProperty("username").GetString()!,
            data.GetProperty("password").GetString()!);
    }
}
