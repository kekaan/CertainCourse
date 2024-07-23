using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CustomerProto = CertainCourse.Customers.Grpc.Customer;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

internal static class CustomerExtension
{
    public static CustomerEntity ToDomain(this CustomerProto customer)
    {
        return new CustomerEntity(
            new CustomerId(customer.Id),
            customer.FirstName,
            customer.LastName,
            MobileNumber.Create(customer.MobileNumber),
            Email.Create(customer.Email),
            customer.DefaultAddress.ToDomain(),
            customer.Addressed.Select(e => e.ToDomain()).ToArray());
    }
    
    public static CustomerEntity ToDomain(this CustomerDal customer)
    {
        return new CustomerEntity(
            new CustomerId(customer.Id),
            customer.FirstName,
            customer.LastName,
            MobileNumber.Create(customer.MobileNumber),
            Email.Create(customer.Email),
            customer.DefaultAddress,
            customer.Addresses.ToArray());
    }
    
    public static CustomerDal ToDal(this CustomerEntity customer)
    {
        return new CustomerDal(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.MobileNumber,
            customer.Email,
            customer.DefaultAddress,
            customer.Addresses.ToArray());
    }

    public record CustomerDal(
        int Id,
        string FirstName,
        string LastName,
        string MobileNumber,
        string Email,
        Address DefaultAddress,
        Address[] Addresses);
}