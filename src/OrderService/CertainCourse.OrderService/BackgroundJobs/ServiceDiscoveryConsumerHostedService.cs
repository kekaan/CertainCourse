using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.BackgroundJobs;

public sealed class ServiceDiscoveryConsumerHostedService : BackgroundService
{
    private const int SD_TIME_TO_DELAY_MS = 1000;
    private readonly IServiceDiscoveryClient _client;
    private readonly IDbStore _dbStore;
    private readonly ILogger<ServiceDiscoveryConsumerHostedService> _logger;
    private readonly ShardDbOptions _dbOptions;

    public ServiceDiscoveryConsumerHostedService(
        IServiceDiscoveryClient client,
        IDbStore dbStore,
        ILogger<ServiceDiscoveryConsumerHostedService> logger,
        IOptions<ShardDbOptions> dbOptions)
    {
        _client = client;
        _dbStore = dbStore;
        _logger = logger;
        _dbOptions = dbOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var response in _client.GetEndpoints(_dbOptions.ClusterName, stoppingToken))
                {
                    _logger.LogInformation(
                        "Get a new data from SD. Timestamp {Timestamp}",
                        response.LastUpdated);

                    await _dbStore.UpdateEndpointAsync(response.Endpoints);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Ошибка получения данных из SD");
                await Task.Delay(SD_TIME_TO_DELAY_MS, stoppingToken);
            }
        }
    }
}