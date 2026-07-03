using Api.Bootstrap;
using Api.Filters;
using Api.Http;
using Application.Dtos.Common;
using Application.Dtos.Vaccines;
using Application.UseCases.Vaccines;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class VaccineEndpoints
{
    public static void MapVaccineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/vaccines").WithTags("Vaccines");

        group.MapGet("/", List)
            .WithName("ListVaccines")
            .AllowAnonymous()
            .Produces<SuccessResponse<PagedResponse<VaccineResponse>>>();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetVaccineById")
            .RequireAuthorization()
            .Produces<SuccessResponse<VaccineResponse>>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateVaccine")
            .RequireAuthorization(AuthDiConfiguration.AdminOnlyPolicy)
            .AddEndpointFilter<ValidationFilter<CreateVaccineRequest>>()
            .Produces<SuccessResponse<VaccineResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id:guid}", Update)
            .WithName("UpdateVaccine")
            .RequireAuthorization(AuthDiConfiguration.AdminOnlyPolicy)
            .AddEndpointFilter<ValidationFilter<UpdateVaccineRequest>>()
            .Produces<SuccessResponse<VaccineResponse>>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> List(
        [FromServices] ListVaccines useCase,
        int page = 1,
        int pageSize = 20)
    {
        var vaccines = await useCase.RunAsync(page, pageSize);
        return Results.Ok(vaccines);
    }

    private static async Task<IResult> GetById(Guid id, [FromServices] GetVaccineById useCase)
    {
        var vaccine = await useCase.RunAsync(id);
        return Results.Ok(vaccine);
    }

    private static async Task<IResult> Create(
        CreateVaccineRequest request,
        [FromServices] CreateVaccine useCase)
    {
        var vaccine = await useCase.RunAsync(request);
        return Results.CreatedAtRoute("GetVaccineById", new { id = vaccine.Id }, vaccine);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateVaccineRequest request,
        [FromServices] UpdateVaccine useCase)
    {
        var vaccine = await useCase.RunAsync(id, request);
        return Results.Ok(vaccine);
    }
}
