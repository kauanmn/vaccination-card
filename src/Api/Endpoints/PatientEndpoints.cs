using Api.Bootstrap;
using Api.Filters;
using Api.Http;
using Application.Dtos.Common;
using Application.Dtos.Patients;
using Application.Dtos.Vaccinations;
using Application.UseCases.Patients;
using Application.UseCases.Vaccinations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/patients").WithTags("Patients");

        group.MapGet("/", List)
            .WithName("ListPatients")
            .RequireAuthorization(AuthDiConfiguration.AdminOnlyPolicy)
            .Produces<SuccessResponse<PagedResponse<PatientResponse>>>()
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetPatientById")
            .RequireAuthorization()
            .AddEndpointFilter<PatientOwnershipFilter>()
            .Produces<SuccessResponse<PatientResponse>>()
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreatePatient")
            .RequireAuthorization(AuthDiConfiguration.AdminOnlyPolicy)
            .AddEndpointFilter<ValidationFilter<CreatePatientRequest>>()
            .Produces<SuccessResponse<CreatePatientResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeletePatient")
            .RequireAuthorization()
            .AddEndpointFilter<PatientOwnershipFilter>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{patientId:guid}/vaccinations", RegisterVaccination)
            .WithName("RegisterVaccination")
            .RequireAuthorization()
            .AddEndpointFilter<PatientOwnershipFilter>()
            .AddEndpointFilter<ValidationFilter<RegisterVaccinationRequest>>()
            .Produces<SuccessResponse<PatientResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{patientId:guid}/vaccinations/{vaccinationId:guid}", RemoveVaccination)
            .WithName("RemoveVaccination")
            .RequireAuthorization()
            .AddEndpointFilter<PatientOwnershipFilter>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> List(
        [FromServices] ListPatients useCase,
        int page = 1,
        int pageSize = 20)
    {
        var patients = await useCase.RunAsync(page, pageSize);
        return Results.Ok(patients);
    }

    private static async Task<IResult> GetById(Guid id, [FromServices] GetPatientById useCase)
    {
        var patient = await useCase.RunAsync(id);
        return Results.Ok(patient);
    }

    private static async Task<IResult> Create(
        CreatePatientRequest request,
        [FromServices] CreatePatient useCase)
    {
        var patient = await useCase.RunAsync(request);
        return Results.CreatedAtRoute("GetPatientById", new { id = patient.Id }, patient);
    }

    private static async Task<IResult> Delete(Guid id, [FromServices] DeletePatient useCase)
    {
        await useCase.RunAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> RegisterVaccination(
        Guid patientId,
        RegisterVaccinationRequest request,
        [FromServices] RegisterVaccination useCase)
    {
        var patient = await useCase.RunAsync(patientId, request);
        return Results.CreatedAtRoute("GetPatientById", new { id = patient.Id }, patient);
    }

    private static async Task<IResult> RemoveVaccination(
        Guid patientId,
        Guid vaccinationId,
        [FromServices] RemoveVaccination useCase)
    {
        await useCase.RunAsync(patientId, vaccinationId);
        return Results.NoContent();
    }
}
