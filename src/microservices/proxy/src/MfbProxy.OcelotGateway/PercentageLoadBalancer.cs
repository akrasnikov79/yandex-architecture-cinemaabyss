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
    private long _counter = -1;

    public PercentageLoadBalancer(Func<Task<List<Service>>> services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
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

        var weights = GetWeights(services.Count);
        var totalWeight = weights.Sum();

        if (totalWeight <= 0)
        {
            return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
        }

        var nextIndex = GetNextIndex(totalWeight);
        var cumulative = 0;

        for (var i = 0; i < services.Count; i++)
        {
            cumulative += weights[i];
            if (nextIndex < cumulative)
            { 
                return new OkResponse<ServiceHostAndPort>(services[i].HostAndPort);
            }
        }

        return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
    } 

    private static int[] GetWeights(int serviceCount)
    {
        var migrationEnvironment = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT");

        if (string.IsNullOrWhiteSpace(migrationEnvironment) || !int.TryParse(migrationEnvironment, out var migrationPercent))
        {
            // Если переменная не задана или невалидна, равномерно распределяем нагрузку
            return [.. Enumerable.Repeat(1, serviceCount)];
        }

        // Ограничиваем процент в диапазоне 0-100
        migrationPercent = Math.Max(0, Math.Min(100, migrationPercent));

        if (serviceCount == 2)
        {
            // Для двух сервисов: movies-service получает migrationPercent%, monolith получает остальное
            var movies = migrationPercent;
            var monolith = 100 - migrationPercent;
            return [monolith, movies];
        }

        // Для других случаев равномерно распределяем
        return [.. Enumerable.Repeat(1, serviceCount)];
    }

    private int GetNextIndex(int totalWeight)
    {
        var current = Interlocked.Increment(ref _counter);
        
        // Безопасная обработка переполнения и отрицательных значений
        if (current < 0 || current > long.MaxValue - 1000)
        {
            current = 0;
            Interlocked.Exchange(ref _counter, 0);
        }

        // Используем Math.Abs для дополнительной безопасности
        return (int)(Math.Abs(current) % totalWeight);
    }
}
