using Api.Bootstrap;
using Api.Endpoints;
using Api.Middlewares;
using Infrastructure.Persistence.SQLite.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SqliteContext>();
    context.Database.EnsureCreated(); // ou Migrate() se você usar migrations
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

app.MapPatientEndpoints();

app.Run();