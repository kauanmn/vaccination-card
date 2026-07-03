using Api.Bootstrap;
using Api.Endpoints;
using Api.Middlewares;
using Api.OpenApi;
using Infrastructure.Persistence.SQLite.Client;
using Infrastructure.Persistence.SQLite.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SqliteContext>();
    context.Database.EnsureCreated();
    await VaccineSeeder.SeedAsync(context);
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ResponseWrapperMiddleware>();

app.UseCors(CorsDiConfiguration.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
api.MapAuthEndpoints();
api.MapPatientEndpoints();
api.MapVaccineEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;