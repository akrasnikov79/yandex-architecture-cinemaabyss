using Microsoft.Extensions.Logging;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;

namespace MfbProxy.OcelotGateway;

/// <summary>
/// Distributes requests across downstream services according to percentage weights.
/// </summary>
public class PercentageLoadBalancer : ILoadBalancer
{
    private readonly Func<Task<List<Service>>> _services;
    private readonly ILogger<PercentageLoadBalancer> _logger;
    private long _counter = -1;

    public PercentageLoadBalancer(Func<Task<List<Service>>> services, ILogger<PercentageLoadBalancer> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Type => nameof(PercentageLoadBalancer);

    public void Release(ServiceHostAndPort hostAndPort)
    {
        // No resources to release.
    }

    public async Task<Response<ServiceHostAndPort>> LeaseAsync(HttpContext context)
    {
        var services = await _services.Invoke();

        if (services.Count == 0)
        {
            throw new InvalidOperationException("No downstream services are registered for PercentageLoadBalancer.");
        }

        try
        { 
            var (weights, total) = GetWeights(services.Count);

            var nextIndex = GetNextIndex(total);
            var cumulative = 0;

            for (var i = 0; i < services.Count; i++)
            {
                cumulative += weights[i];

                if (nextIndex < cumulative)
                {
                    return new OkResponse<ServiceHostAndPort>(services[i].HostAndPort);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get weights configuration for load balancing. Services count: {ServiceCount}. Falling back to first service.", services.Count);
            return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
        }

        return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
    }

    private static (int[] weights, int totalWeight) GetWeights(int serviceCount)
    {
        var migrationEnvironment = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT");
        int[] weights;

        if (string.IsNullOrWhiteSpace(migrationEnvironment) || !int.TryParse(migrationEnvironment, out var migrationPercent))
        {
            // Если переменная не задана или невалидна, равномерно распределяем нагрузку
            weights = [.. Enumerable.Repeat(1, serviceCount)];
        }
        else
        {
            // Ограничиваем процент в диапазоне 0-100
            migrationPercent = Math.Max(0, Math.Min(100, migrationPercent));

            if (serviceCount == 2)
            {
                // Для двух сервисов: movies-service получает migrationPercent%, monolith получает остальное
                var movies = migrationPercent;
                var monolith = 100 - migrationPercent;
                weights = [monolith, movies];
            }
            else
            {
                // Для других случаев равномерно распределяем
                weights = [.. Enumerable.Repeat(1, serviceCount)];
            }
        }

        var total = weights.Sum();

        if (total <= 0)
        {
            throw new InvalidOperationException("PercentageLoadBalancer received zero total weight configuration.");
        }

        return (weights, total);
    }

    /// <summary>
    /// Вычисляет позицию для выбора следующего сервиса в алгоритме взвешенного распределения нагрузки.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    private int GetNextIndex(int total)
    {
        var current = Interlocked.Increment(ref _counter);

        // Безопасная обработка переполнения и отрицательных значений
        if (current < 0 || current > long.MaxValue - 1000)
        {
            current = 0;
            Interlocked.Exchange(ref _counter, 0);
        }

        // Используем Math.Abs для дополнительной безопасности
        // Операция остатка от деления, которая дает значение от 0 до totalWeigh
        return (int)(Math.Abs(current) % total);
    }
}
