// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace VectorStoreRAG.Options;

/// <summary>
/// Contains settings to control the RAG experience.
/// </summary>
internal sealed class RagConfig
{
    public const string ConfigSectionName = "Rag";

    [Required]
    public string AIChatService { get; set; } = string.Empty;

    [Required]
    public string AIEmbeddingService { get; set; } = string.Empty;

    [Required]
    public string CollectionName { get; set; } = string.Empty;

    [Required]
    public string PostgresConnectionString { get; set; } = string.Empty;

}
