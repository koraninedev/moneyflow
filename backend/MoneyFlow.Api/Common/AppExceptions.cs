namespace MoneyFlow.Api.Common;

public abstract class ApiException(int statusCode, string code, string message, Dictionary<string, string>? fields = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
    public Dictionary<string, string>? Fields { get; } = fields;
}

public sealed class NotFoundException(string message = "Resource not found")
    : ApiException(StatusCodes.Status404NotFound, "NOT_FOUND", message);

public sealed class ConflictException(string message)
    : ApiException(StatusCodes.Status409Conflict, "CONFLICT", message);

public sealed class ValidationException(string message, Dictionary<string, string>? fields = null)
    : ApiException(StatusCodes.Status400BadRequest, "VALIDATION_ERROR", message, fields);

public sealed class UnauthorizedException(string message = "Not authenticated")
    : ApiException(StatusCodes.Status401Unauthorized, "UNAUTHORIZED", message);
