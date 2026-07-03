using Api.Http;
using Application.Dtos.Patients;
using Application.UseCases.Patients;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/patients").WithTags("Patients");

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetPatientById")
            .Produces<SuccessResponse<PatientResponse>>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreatePatient")
            .Produces<SuccessResponse<PatientResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetById(Guid id, [FromServices] GetPatientById useCase)
    {
        var patient = await useCase.RunAsync(id);
        return Results.Ok(patient);
    }
    
    private static async Task<IResult> Create(CreatePatientRequest request, [FromServices] CreatePatient useCase)
    {
        var patient = await useCase.RunAsync(request);
        return Results.CreatedAtRoute("GetPatientById", new { id = patient.Id }, patient);
    }
}