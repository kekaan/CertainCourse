namespace CertainCourse.OrderService.Domain.Exceptions;

public class DomainException : ApplicationException
{
    public int Code { get; }

    public DomainException(int code, string message) : base(message)
    {
        Code = code;
    }

    public DomainException(string message) : this(code: -1, message)
    {
    }
}