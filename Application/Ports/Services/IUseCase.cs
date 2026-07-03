namespace Application.Ports.Services;

public interface IUseCase<TInput>
    where TInput : class
{
    Task RunAsync(TInput input);
}

public interface IUseCase<TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    Task<TOutput> RunAsync(TInput input);
}