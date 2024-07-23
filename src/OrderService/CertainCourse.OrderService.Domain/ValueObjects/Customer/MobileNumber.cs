using System.Text.RegularExpressions;
using CertainCourse.OrderService.Domain.Exceptions;

namespace CertainCourse.OrderService.Domain.ValueObjects.Customer;

public sealed class MobileNumber
{
    public string Value { get; }

    private MobileNumber(string value) => Value = value;

    public static MobileNumber Create(string value)
    {
        var isMatch = Regex.IsMatch(value, "^[\\s\\d\\(\\)\\-\\+]+$");

        if (!isMatch)
        {
            throw new DomainException($"Номер телефона `{value}` содержит недопустимые символы");
        }

        return new MobileNumber(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is MobileNumber email)
            return Equals(email);

        return false;
    }

    private bool Equals(MobileNumber other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(MobileNumber mobileNumber) => mobileNumber.Value;
}