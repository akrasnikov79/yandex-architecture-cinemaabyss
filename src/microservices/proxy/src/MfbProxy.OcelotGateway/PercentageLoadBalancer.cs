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
    private readonly int[] _weights;
    private long _counter = -1;

    public PercentageLoadBalancer(Func<Task<List<Service>>> services, Ocelot.Configuration.DownstreamRoute route, string? weights)
    {
        var r = route ?? throw new ArgumentNullException(nameof(route));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _weights = ParseWeights(weights);
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

        var weights = AlignWeights(services.Count);
        var totalWeight = weights.Sum();

        if (totalWeight <= 0)
        {
            throw new InvalidOperationException("PercentageLoadBalancer received zero total weight configuration.");
        }

        var nextIndex = GetNextIndex(totalWeight);
        var cumulative = 0;

        for (var i = 0; i < services.Count; i++)
        {
            cumulative += weights[i];
            if (nextIndex < cumulative)
            {
                var a = services[0].Tags;
                return new OkResponse<ServiceHostAndPort>(services[i].HostAndPort);
            }
        }

        return new OkResponse<ServiceHostAndPort>(services[0].HostAndPort);
    }

    private static int[] ParseWeights(string? weights)
    {
        if (string.IsNullOrWhiteSpace(weights))
        {
            return Array.Empty<int>();
        }

        return weights
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : 1)
            .ToArray();
    }

    private int[] AlignWeights(int serviceCount)
    {
        if (_weights.Length == serviceCount)
        {
            return _weights;
        }

        if (_weights.Length == 0)
        {
            return Enumerable.Repeat(1, serviceCount).ToArray();
        }

        var aligned = new int[serviceCount];
        for (var i = 0; i < serviceCount; i++)
        {
            aligned[i] = _weights[i % _weights.Length];
        }

        return aligned;
    }

    private int GetNextIndex(int totalWeight)
    {
        var current = Interlocked.Increment(ref _counter);
        if (current < 0)
        {
            current = 0;
            Interlocked.Exchange(ref _counter, 0);
        }

        return (int)(current % totalWeight);
    }
}
