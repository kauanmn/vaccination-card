using Api.Filters;
using Api.Http;
using Application.Dtos.Auth;
using Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", LoginHandler)
            .WithName("Login")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .Produces<SuccessResponse<LoginResponse>>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> LoginHandler(
        LoginRequest request,
        [FromServices] Login useCase)
    {
        var response = await useCase.RunAsync(request);
        return Results.Ok(response);
    }
}
