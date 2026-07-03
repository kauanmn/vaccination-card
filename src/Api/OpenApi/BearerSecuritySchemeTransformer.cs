using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Cole o token JWT retornado por /auth/login (sem o prefixo 'Bearer ')."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = scheme;

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, document)] = new List<string>()
        });

        return Task.CompletedTask;
    }
}
