using Api.Bootstrap;
using Api.Endpoints;
using Api.Middlewares;
using Api.OpenApi;
using Infrastructure.Persistence.SQLite.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SqliteContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ResponseWrapperMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapPatientEndpoints();
app.MapVaccineEndpoints();

app.Run();

public partial class Program;