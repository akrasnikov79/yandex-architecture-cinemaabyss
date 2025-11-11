using MfbProxy.OcelotGateway.Midllewares;
using Ocelot.DependencyInjection;
using Ocelot.Values;

namespace MfbProxy.OcelotGateway.Extensions;

static class LoadBalancerExtensions
{
    public static IOcelotBuilder AddPercentageLoadBalancer(this IOcelotBuilder builder)
    {
        return builder.AddCustomLoadBalancer((serviceProvider, route, discoveryProvider) =>
        {
            if (discoveryProvider is null)
            {
                throw new InvalidOperationException("Service discovery provider is not configured for the current route.");
            }

            Func<Task<List<Service>>> services = discoveryProvider.GetAsync;
            var logger = serviceProvider.GetRequiredService<ILogger<PercentageLoadBalancer>>();
            return new PercentageLoadBalancer(services, logger);
        });
    }
}

