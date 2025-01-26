# Function Calling

To try out this sample:

-  `ollama run llama3.2`
- Clone the repo
- `dotnet run` !

# High Level notes

## Create the kernel builder for an ollama client

```csharp
// Create a kernel builder for an Ollama Model
var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
    new OllamaApiClient("http://localhost:11434","llama3.2:latest")
);
```

## Register the Plugin

```csharp
// Add function by type
builder.Plugins.AddFromType<DatePlugin>();
```

## Build kernel with the registered plugin

```csharp
// Create the kernel
var kernel = builder.Build();
```

## Configure settings with auto function calling

```csharp
// Define function calling behaviour. Sends all funcs to kernel to pick approppriate 
// plugin and invoke automatically
var settings = new PromptExecutionSettings{ 
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
```

## Invoke prompt with auto function caling settings:

```csharp
// invoke the prompt
var response = await kernel.InvokePromptAsync("What's today's date ?",new(settings));
```

## Run the project

```
❯ dotnet run
Today's date is January 23, 2025.
```
