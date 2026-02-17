using CinemaAbyss.Proxy.Midllewares;
using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Values;

namespace CinemaAbyss.Proxy.Extensions;

static class BalancerExtensions
{
    public static IOcelotBuilder AddPercentageBalancer(this IOcelotBuilder builder)
    {
        return builder.AddCustomLoadBalancer((serviceProvider, route, discoveryProvider) =>
        {
            if (discoveryProvider is null)
            {
                throw new InvalidOperationException("Service discovery provider is not configured for the current route.");
            }

            Func<Task<List<Service>>> services = discoveryProvider.GetAsync;
            var logger = serviceProvider.GetRequiredService<ILogger<PercentageBalancer>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return new PercentageBalancer(services, logger, configuration);
        });
    }
}

