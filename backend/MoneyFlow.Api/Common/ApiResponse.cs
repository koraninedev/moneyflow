namespace MoneyFlow.Api.Common;

public sealed class ApiResponse<T>
{
    public T? Data { get; init; }
    public object? Error { get; init; }
    public object? Meta { get; init; }
    public static ApiResponse<T> Ok(T data, object? meta = null) => new() { Data = data, Meta = meta };
}

public sealed class ApiError
{
    public string Code { get; init; } = "ERROR";
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string>? Fields { get; init; }
}
