namespace Api.Http;

public record SuccessResponse<T>(bool Success, T Data);

public record ErrorResponse(bool Success, ApiError Error);

public record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details = null);

public record ApiErrorDetail(string Field, string Message);
