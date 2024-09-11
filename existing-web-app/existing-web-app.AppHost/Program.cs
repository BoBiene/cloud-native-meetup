var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder
                    .AddAzureStorage("storage")
                    .RunAsEmulator(configure =>
                        configure.WithDataBindMount("persist")
                     )
                    .AddBlobs("blobs");

var blazorApp = builder.AddProject<Projects.BlazorWebApp>("blazorwebapp")
    .WithReference(blobs)
    .WithReplicas(2)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // in production
    blazorApp.WithEnvironment("REST_DASHBOARD", "https://webhook.site/#!/view/20413fda-ea9d-4274-8fce-bd2ef5b0ab69");
    blazorApp.WithEnvironment("REST_ENDPOINT", "https://webhook.site/20413fda-ea9d-4274-8fce-bd2ef5b0ab69");
}
else
{
    var webhook_guid = Guid.NewGuid().ToString();
    var webhook = builder.AddContainer("webhook", "ghcr.io/tarampampam/webhook-tester")
        .WithHttpEndpoint(targetPort: 8080)
        .WithEnvironment("CREATE_SESSION", webhook_guid);


    blazorApp.WithEnvironment(callback: env =>
    {
        if (webhook.Resource.TryGetEndpoints(out var webhookEndpoints))
        {
            var endpoint = webhookEndpoints.First(e => e.Name == "http");
            env.EnvironmentVariables.Add("REST_ENDPOINT", $"http://{endpoint?.AllocatedEndpoint?.EndPointString ?? string.Empty}/{webhook_guid}");
            env.EnvironmentVariables.Add("REST_DASHBOARD", $"http://{endpoint?.AllocatedEndpoint?.EndPointString ?? string.Empty}/#/{webhook_guid}");
        }
    });
}

builder.Build().Run();
