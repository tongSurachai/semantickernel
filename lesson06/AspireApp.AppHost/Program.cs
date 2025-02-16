using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true",
});
// Step 1: Define Ports
const string semanticKernelPort = "5234";
const string openWebUiPort = "3000";

// Step 2: Add Semantic Kernel Service with Proper Endpoint Configuration
var kernelApi = builder.AddProject<Projects.KernelApi>("KernelApi")
    .WithEnvironment("ASPNETCORE_URLS", $"http://*:{semanticKernelPort}")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(semanticKernelPort);
        e.TargetPort = int.Parse(semanticKernelPort);
        e.IsProxied = false;
    }); 
    

// Step 3: Add OpenWebUI Container with Correct Environment Variable Handling
builder.AddContainer("openwebui", "ghcr.io/open-webui/open-webui")
    .WithEnvironment("OLLAMA_API_BASE_URL", $"http://host.docker.internal:{semanticKernelPort}")
    .WithEnvironment("OLLAMA_API_BASE_URLS", $"http://host.docker.internal:{semanticKernelPort}")
    .WithEnvironment("OPENAI_API_BASE_URLS", $"http://host.docker.internal:{semanticKernelPort}")
    .WithEnvironment("PORT", openWebUiPort)
    .WithEnvironment("HOST", "0.0.0.0")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(openWebUiPort);
        e.TargetPort = int.Parse(openWebUiPort);
    })
    .WithReference(kernelApi)
    .WaitFor(kernelApi)
    .WithLifetime(ContainerLifetime.Persistent);

// Step 5: Build and Run the Distributed Application
builder.Build().Run();