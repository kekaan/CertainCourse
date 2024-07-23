using CertainCourse.OrderService.Domain.Exceptions;

namespace CertainCourse.OrderService.Domain.ValueObjects.Customer;

public sealed class Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (!value.Contains('@'))
            throw new DomainException($"Email `{value}` не содержит символ `@`");

        return new Email(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Email email)
            return Equals(email);

        return false;
    }

    private bool Equals(Email other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(Email email) => email.Value;
}