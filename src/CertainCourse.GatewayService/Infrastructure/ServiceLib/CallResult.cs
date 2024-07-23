namespace CertainCourse.GatewayService.Infrastructure.ServiceLib;

/// <summary>
/// Результат операции
/// </summary>
public record CallResult
{
    /// <summary>
    /// Был ли вызов успешным
    /// </summary>
    public bool Success => StatusCode is CallResultStatusCode.OK;

    /// <summary>
    /// Код результата выполнения
    /// </summary>
    public CallResultStatusCode StatusCode { get; init; }
        
    /// <summary>
    /// Сообщение ошибки
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="statusCode">Код результата выполнения</param>
    /// <param name="errorMessage">Сообщение ошибки</param>
    public CallResult(CallResultStatusCode statusCode, string? errorMessage = null)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }
}


/// <inheritdoc />
public sealed record CallResult<T> : CallResult where T: class
{
    /// <summary>
    /// Данные, возвращаемые вызовом, доступны только при значении Success = true
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="statusCode">Код результата выполнения</param>
    /// <param name="data">Данные</param>
    /// <param name="errorMessage">Сообщение ошибки</param>
    public CallResult(CallResultStatusCode statusCode, T? data = null, string? errorMessage = null)
        : base(statusCode, errorMessage)
    {
        Data = data;
    }
}