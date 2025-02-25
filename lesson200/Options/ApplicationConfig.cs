// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Configuration;

namespace VectorStoreRAG.Options;

/// <summary>
/// Helper class to load all configuration settings for the VectorStoreRAG project.
/// </summary>
internal sealed class ApplicationConfig
{
    private readonly AzureOpenAIConfig _azureOpenAIConfig;
    private readonly AzureOpenAIEmbeddingsConfig _azureOpenAIEmbeddingsConfig = new();
    private readonly RagConfig _ragConfig = new();

    public ApplicationConfig(ConfigurationManager configurationManager)
    {
        this._azureOpenAIConfig = new();
        configurationManager
            .GetRequiredSection($"AIServices:{AzureOpenAIConfig.ConfigSectionName}")
            .Bind(this._azureOpenAIConfig);
        configurationManager
            .GetRequiredSection($"AIServices:{AzureOpenAIEmbeddingsConfig.ConfigSectionName}")
            .Bind(this._azureOpenAIEmbeddingsConfig);
         configurationManager
            .GetRequiredSection(RagConfig.ConfigSectionName)
            .Bind(this._ragConfig);

    }

    public AzureOpenAIConfig AzureOpenAIConfig => this._azureOpenAIConfig;

    public AzureOpenAIEmbeddingsConfig AzureOpenAIEmbeddingsConfig => this._azureOpenAIEmbeddingsConfig;

 
    public RagConfig RagConfig => this._ragConfig;

 
}
