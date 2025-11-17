using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace CinemaAbyss.Proxy.Gateway;

/// <summary>
/// Weighted Round Robin load balancing policy.
/// Distributes traffic based on weight metadata assigned to destinations.
/// </summary>
public class WeightedRoundRobinPolicy : ILoadBalancingPolicy
{
    private long _counter = 0;

    public string Name => "WeightedRoundRobin";

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        if (availableDestinations.Count == 1)
        {
            return availableDestinations[0];
        }

        // Build weighted list of destinations
        var weightedDestinations = new List<DestinationState>();

        foreach (var destination in availableDestinations)
        {
            // Get weight from metadata, default to 1 if not specified
            var weight = 1;
            if (destination.Model.Config.Metadata != null &&
                destination.Model.Config.Metadata.TryGetValue("Weight", out var weightStr))
            {
                if (int.TryParse(weightStr, out var parsedWeight) && parsedWeight > 0)
                {
                    weight = parsedWeight;
                }
            }

            // Add destination 'weight' times to the list
            for (int i = 0; i < weight; i++)
            {
                weightedDestinations.Add(destination);
            }
        }

        // Round-robin over the weighted list
        var index = (int)(Interlocked.Increment(ref _counter) % weightedDestinations.Count);
        return weightedDestinations[index];
    }
}


