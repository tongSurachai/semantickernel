using Microsoft.Extensions.VectorData;
namespace VectorStoreRAG;

/// <summary>
/// The Semantic Kernel Vector Store connectors use a model first approach to interacting with databases.
/// All methods to upsert or get records use strongly typed model classes.The properties on these classes are decorated with attributes that indicate the purpose of each property.
/// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model?pivots=programming-language-csharp
/// </summary>
/// <typeparam name="TKey"></typeparam>
internal sealed class ItsdTicket<TKey>
{
    [VectorStoreRecordKey]
    public required TKey id { get; set; }

    [VectorStoreRecordData]
    public string? text { get; set; }

    [VectorStoreRecordData]
    public string? metadata { get; set; }

    //[VectorStoreRecordVector(4, DistanceFunction.CosineDistance, IndexKind.Hnsw)]
    [VectorStoreRecordVector(Dimensions: 3072)]
    public ReadOnlyMemory<float> embedding { get; set; }
}
