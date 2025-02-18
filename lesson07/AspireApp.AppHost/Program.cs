using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true",
});
// Step 1: Define Ports
const string semanticKernelPort = "5234";
const string openWebUiPort = "3000";
const string kernelMemoryPort = "9001";
const string pgSqlPort = "5432";

// Step 2: Add PostgreSQL container setup with pgvector installation
var scriptsPath = Path.Combine(AppContext.BaseDirectory, "scripts");

var postgres = builder.AddContainer("postgres-db", "postgres")
    .WithEnvironment("POSTGRES_USER", "myuser")
    .WithEnvironment("POSTGRES_PASSWORD", "mypassword")
    .WithEnvironment("POSTGRES_DB", "mydatabase")
    .WithVolume("pgdata", "/var/lib/postgresql/data")
    .WithBindMount(scriptsPath, "/scripts") // ✅ Mount the entire scripts folder
    .WithArgs("sh", "/scripts/entrypoint.sh")  // ✅ Execute the shell script
    .WithEndpoint("tcp", e =>
    {
        e.Port = int.Parse(pgSqlPort);
        e.TargetPort = int.Parse(pgSqlPort);
    });




    //.WithEndpoint(port: 5432, env: "POSTGRES_CONNECTION_STRING"); 

// Step 3: Add Kernel Memory
var kernelMemory = builder.AddProject<Projects.KernelApi>("KernelMemory")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(kernelMemoryPort);
        e.TargetPort = int.Parse(kernelMemoryPort);
        e.IsProxied = false;
    })
    .WaitFor(postgres);

// Step 3: Add Semantic Kernel Service with Proper Endpoint Configuration
var kernelApi = builder.AddProject<Projects.KernelApi>("KernelApi")
    .WithEnvironment("ASPNETCORE_URLS", $"http://*:{semanticKernelPort}")
    .WithEndpoint("http", e =>
    {
        e.Port = int.Parse(semanticKernelPort);
        e.TargetPort = int.Parse(semanticKernelPort);
        e.IsProxied = false;
    })
    .WaitFor(kernelMemory); 


// Step 5: Add OpenWebUI Container with Correct Environment Variable Handling
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
    .WaitFor(kernelMemory)
    .WithLifetime(ContainerLifetime.Persistent);

// Step 5: Build and Run the Distributed Application
builder.Build().Run();