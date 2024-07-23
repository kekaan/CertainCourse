using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.CustomerServiceClient;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.DataAccess.Common.CallResult;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.Redis.Repositories;
using StackExchange.Redis;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;

internal sealed class CustomerRepository : BaseRedisRepository, ICustomerRepository
{
    private const string CUSTOMERS_KEY_PREFIX = "customers";
    private readonly ICustomerServiceClient _customerServiceClient;

    public CustomerRepository(ICustomerServiceClient customerServiceClient, IOptions<RedisOptions> redisSettings,
        IConnectionMultiplexer connectionMultiplexer) :
        base(connectionMultiplexer, redisSettings, CUSTOMERS_KEY_PREFIX)
    {
        _customerServiceClient = customerServiceClient;
    }

    public async Task<CustomerEntity> GetCustomerByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = BuildKey(id);

        var customerFromCacheCallResult = await GetCustomerFromRedisCache(key, cancellationToken);
        if (customerFromCacheCallResult.Success)
            return customerFromCacheCallResult.Data!;

        var customer = await _customerServiceClient.GetCustomer(id, cancellationToken: cancellationToken);

        await AddCustomerToRedisCache(key, customer, cancellationToken);

        return customer;
    }

    private async Task<CallResult<CustomerEntity>> GetCustomerFromRedisCache(RedisKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var isKeyExist = await _database.KeyExistsAsync(key);

        if (!isKeyExist)
            return new CallResult<CustomerEntity>(statusCode: CallResultStatusCode.NotFound);

        var resultRedis = await _database.StringGetAsync(key);

        var result = ToObject<CustomerExtension.CustomerDal>(resultRedis);

        return new CallResult<CustomerEntity>(CallResultStatusCode.OK, result!.ToDomain());
    }

    private async Task AddCustomerToRedisCache(RedisKey key, CustomerEntity? customer,
        CancellationToken cancellationToken)
    {
        if (customer is null)
            return;
        
        cancellationToken.ThrowIfCancellationRequested();
        var resultRedis = ToRedisString(customer.ToDal());
        await _database.StringSetAsync(key, resultRedis);
    }
}