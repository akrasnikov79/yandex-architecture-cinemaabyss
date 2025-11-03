using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace MfbProxy.Gateway;

public class CustomLoadBalancingPolicy : ILoadBalancingPolicy
{
    private long _counter = 0;

    public string Name => "CustomPolicy";

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        // Custom logic: Route based on user ID in header
        if (context.Request.Headers.TryGetValue("X-User-ID", out var userIdHeader))
        {
            if (int.TryParse(userIdHeader.FirstOrDefault(), out var userId))
            {
                // Route even user IDs to first server, odd to second server
                var destinationIndex = userId % availableDestinations.Count;
                return availableDestinations[destinationIndex];
            }
        }

        // Fallback to round-robin
        var index = (int)(Interlocked.Increment(ref _counter) % availableDestinations.Count);
        return availableDestinations[index];
    }
}

