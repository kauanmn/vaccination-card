using FluentValidation;

namespace Api.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is not null)
        {
            var result = await _validator.ValidateAsync(argument);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}
