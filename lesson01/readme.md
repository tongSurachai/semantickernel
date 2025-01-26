# Semantic Kernel Hello World

This is a hello world sample to get started using Semantic Kernel.

## Assumes Ollama is running on local
Please refer to Ollama docs for downloading and running Ollama models on your dev computer. This samples your locally running llama3.2 model on ollama

```
ollama run llama3.2
```

## Nuget Packages required:

```
dotnet add package OllamaSharp
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Connectors.Ollama --prerelease
```

## Create .editorconfig 
Create editor config file to suppress warnings

```
[*.cs]

# SKEXP0070: Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
dotnet_diagnostic.SKEXP0070.severity = none
```
