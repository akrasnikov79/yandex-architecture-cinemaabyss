using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;

namespace MfbProxy.OcelotGateway.Midllewares;

/// <summary>
/// Distributes requests across downstream services according to percentage weights.
/// </summary>
public class PercentageBalancer : ILoadBalancer
{
    private readonly Func<Task<List<Service>>> _services;
    private readonly ILogger<PercentageBalancer> _logger;
    private readonly List<ServiceHostAndPort> _customHostsAndPorts;
    private long _counter = -1;

    public PercentageBalancer(Func<Task<List<Service>>> services, ILogger<PercentageBalancer> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _customHostsAndPorts = InitializeCustomHostsAndPorts();
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
            _logger.LogError("Services count 0. Falling back to first service.", services.Count);
            var fallbackHostAndPort = _customHostsAndPorts.Count > 0
                ? _customHostsAndPorts[0]
                : services[0].HostAndPort;
            return new OkResponse<ServiceHostAndPort>(fallbackHostAndPort);
        }

        try
        {
            var weights = GetWeights(services.Count);

            var nextIndex = GetNextIndex(weights);
            var cumulative = 0;

            for (var i = 0; i < services.Count; i++)
            {
                cumulative += weights[i];

                if (nextIndex < cumulative)
                {
                    // Переопределяем HostAndPort из переменных окружения, если они настроены
                    var hostAndPort = _customHostsAndPorts.Count > i
                        ? _customHostsAndPorts[i]
                        : services[i].HostAndPort;

                    return new OkResponse<ServiceHostAndPort>(hostAndPort);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weights configuration for load balancing. Services count: {ServiceCount}. Falling back to first service.", services.Count);
        }

        // Переопределяем HostAndPort из переменных окружения для fallback
        var fallbackHostAndPort = _customHostsAndPorts.Count > 0
            ? _customHostsAndPorts[0]
            : services[0].HostAndPort;

        return new OkResponse<ServiceHostAndPort>(fallbackHostAndPort);
    }

    private static int[] GetWeights(int serviceCount)
    {
        var migrationEnvironment = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT");

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
            var movies = migrationPercent;
            var monolith = 100 - migrationPercent;
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

    /// <summary>
    /// Инициализирует список HostAndPort из переменных окружения.
    /// Порядок: [0] = MONOLITH_URL, [1] = MOVIES_SERVICE_URL
    /// </summary>
    private List<ServiceHostAndPort> InitializeCustomHostsAndPorts()
    {
        var customHosts = new List<ServiceHostAndPort>();

        var monolithUrl = Environment.GetEnvironmentVariable("MONOLITH_URL");
        var moviesUrl = Environment.GetEnvironmentVariable("MOVIES_SERVICE_URL"); 

        ArgumentException.ThrowIfNullOrWhiteSpace(monolithUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(moviesUrl);

        customHosts.Add(CreateHostAndPort(monolithUrl));
        customHosts.Add(CreateHostAndPort(moviesUrl));
        

        if (customHosts.Count == 0)
        {
            _logger.LogWarning("No custom service URLs configured.");
        }

        return customHosts;
    }

    /// <summary>
    /// Парсит URL и создает ServiceHostAndPort.
    /// Поддерживает форматы: http://host:port, https://host:port
    /// </summary>
    private ServiceHostAndPort CreateHostAndPort(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var host = uri.Host;
            var port = uri.Port;

            // Если порт не указан явно, используем порт по умолчанию для схемы
            if (port == -1)
            {
                port = uri.Scheme.ToLowerInvariant() == "https" ? 443 : 80;
            }

            return new ServiceHostAndPort(host, port);
        }

        throw new InvalidOperationException("No create host and port.");

    }
}
