using System.Security.Claims;
using Application.Security;

namespace Api.Filters;

public class PatientOwnershipFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.IsInRole(Roles.Admin))
            return await next(context);

        var routePatientId = ExtractPatientId(context.HttpContext);
        var claimPatientId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (routePatientId is null
            || !Guid.TryParse(claimPatientId, out var tokenPatientId)
            || tokenPatientId != routePatientId)
            return Results.Forbid();

        return await next(context);
    }

    private static Guid? ExtractPatientId(HttpContext context)
    {
        var routeValues = context.Request.RouteValues;

        var raw = routeValues.TryGetValue("patientId", out var patientId)
            ? patientId?.ToString()
            : routeValues.TryGetValue("id", out var id)
                ? id?.ToString()
                : null;

        return Guid.TryParse(raw, out var value) ? value : null;
    }
}
