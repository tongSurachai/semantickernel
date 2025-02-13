var builder = DistributedApplication.CreateBuilder(args);

// Solution: Fixing Semantic Kernel Port Exposure Issue

// Step 1: Define Ports
const string semanticKernelPort = "5234";
const string openWebUiPort = "3000";
const string webFrontendPort = "5057";

// Step 2: Add Semantic Kernel Service with Proper Endpoint Configuration
var semanticKernelService = builder.AddProject<Projects.hellokernel>("semantickernel")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(semanticKernelPort);
        e.TargetPort = int.Parse(semanticKernelPort + 1);
    });

// Step 3: Add OpenWebUI Container with Correct Environment Variable Handling
builder.AddContainer("openwebui", "ghcr.io/open-webui/open-webui:main")
    .WithEnvironment("OLLAMA_API_BASE_URL", $"http://semantickernel:{semanticKernelPort}")
    .WithEnvironment("PORT", openWebUiPort)
    .WithEnvironment("HOST", "0.0.0.0")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(openWebUiPort);
        e.TargetPort = int.Parse(openWebUiPort);
    })
    .WithReference(semanticKernelService);

// Step 4: Add Web Frontend with Correct Endpoint Configuration
builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithReference(semanticKernelService)
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(webFrontendPort);
        e.TargetPort = int.Parse(webFrontendPort + 1);
    });

// Step 5: Build and Run the Distributed Application
builder.Build().Run();