using Microsoft.Extensions.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;

namespace CinemaAbyss.Proxy.Midllewares;

/// <summary>
/// Distributes requests across downstream services according to percentage weights.
/// </summary>
public class PercentageBalancer : ILoadBalancer
{
    private readonly Func<Task<List<Service>>> _services;
    private readonly ILogger<PercentageBalancer> _logger;
    private readonly IConfiguration _configuration;
    private long _counter = -1;

    public PercentageBalancer(Func<Task<List<Service>>> services, ILogger<PercentageBalancer> logger, IConfiguration configuration)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string Type => nameof(PercentageBalancer);

    public void Release(ServiceHostAndPort hostAndPort)
    {
        // No resources to release.
    }

    public async Task<Response<ServiceHostAndPort>> LeaseAsync(HttpContext context)
    {
        var services = await _services.Invoke();

        if (services.Count == 0)
        {
            throw new InvalidOperationException("No downstream services are registered for PercentageBalancer.");
        }

        try
        {
            var weights = GetWeights(services.Count, _configuration);

            var nextIndex = GetNextIndex(weights);
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
        }

        return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
    }

    private static int[] GetWeights(int serviceCount, IConfiguration configuration)
    {
        var migrationEnvironment = configuration["MOVIES_MIGRATION_PERCENT"];

        if (string.IsNullOrWhiteSpace(migrationEnvironment) || !int.TryParse(migrationEnvironment, out var migrationPercent))
        {
            // Если переменная не задана или невалидна, равномерно распределяем нагрузку
            return [.. Enumerable.Repeat(1, serviceCount)];
        }

        if (serviceCount == 2)
        {
            // Ограничиваем процент в диапазоне 0-100
            migrationPercent = Math.Max(0, Math.Min(100, migrationPercent));

            // movies-service получает migrationPercent, monolith получает остальное
            // 100% = 5 запросов, округление вверх
            const int totalSlots = 5;
            var movies = (int)Math.Round(migrationPercent / 100.0 * totalSlots, MidpointRounding.AwayFromZero);
            var monolith = totalSlots - movies;
            return [monolith, movies];
        }

        // Для других случаев равномерно распределяем
        return [.. Enumerable.Repeat(1, serviceCount)];
    }

    /// <summary>
    /// Вычисляет позицию для выбора следующего сервиса в алгоритме взвешенного распределения нагрузки.
    /// Возвращает индекс на основе текущего счетчика и общего веса, индекс не больще total weight.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    private int GetNextIndex(int[] weights)
    {
        var current = Interlocked.Increment(ref _counter);
        var total = weights.Sum();

        if (total <= 0)
        {
            throw new InvalidOperationException("PercentageBalancer received zero total weight configuration.");
        }

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
