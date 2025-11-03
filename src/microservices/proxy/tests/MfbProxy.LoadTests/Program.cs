using NBomber.CSharp;
using NBomber.Http.CSharp;

var httpClient = new HttpClient();

var scenario = Scenario.Create("gateway_test", async context =>
{
    var request = Http.CreateRequest("GET", "https://my-gateway.example.com/api/resource")
        .WithHeader("Accept", "application/json");
                // Add auth if needed: .WithHeader("Authorization", "Bearer your-token");

            var response = await Http.Send(httpClient, request);

    return response.IsError ? Response.Fail() : Response.Ok(statusCode: response.Payload.Value.StatusCode.ToString(), sizeBytes: response.SizeBytes);
})
.WithLoadSimulations(
    Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();