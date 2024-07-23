using System.Diagnostics.CodeAnalysis;

namespace CertainCourse.OrderService.Application.MessageBrokers;

public class HandlerResult
{
    public HandlerException? Error { get; }
    
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; }
    
    private HandlerResult(bool success, HandlerException? error)
    {
        Success = success;
        Error = error;
    }
    
    public static HandlerResult Ok => new(true, null);
    
    public static implicit operator HandlerResult(HandlerException exception) => new(false, exception);
}

