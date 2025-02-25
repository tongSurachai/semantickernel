using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using VectorStoreRAG;
using VectorStoreRAG.Options;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configure configuration and load the application configuration.
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<RagConfig>(builder.Configuration.GetSection(RagConfig.ConfigSectionName));
var appConfig = new ApplicationConfig(builder.Configuration);

// Create a cancellation token and source to pass to the application service to allow them
// to request a graceful application shutdown.
CancellationTokenSource appShutdownCancellationTokenSource = new();
CancellationToken appShutdownCancellationToken = appShutdownCancellationTokenSource.Token;
builder.Services.AddKeyedSingleton("AppShutdown", appShutdownCancellationTokenSource);

// Register the kernel with the dependency injection container
// and add Chat Completion and Text Embedding Generation services.
var kernelBuilder = builder.Services.AddKernel();

kernelBuilder.AddAzureOpenAIChatCompletion(
    appConfig.AzureOpenAIConfig.ChatDeploymentName,
    appConfig.AzureOpenAIConfig.Endpoint,
    new AzureCliCredential());


kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
    appConfig.AzureOpenAIEmbeddingsConfig.DeploymentName,
    appConfig.AzureOpenAIEmbeddingsConfig.Endpoint,
    new AzureCliCredential());

// register following to DI
//  - NpgsqlDataSource (connection to Postgres)
//  - IVectorStore
builder.Services.AddPostgresVectorStore(appConfig.RagConfig.PostgresConnectionString);
//builder.Services.AddPostgresVectorStore("Host=localhost;Port=5432;Database=sk_demo;User Id=postgres;Password=mysecretpassword");

//https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp
//https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/out-of-the-box-textsearch/vectorstore-textsearch?pivots=programming-language-csharp
//https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/out-of-the-box-textsearch/vectorstore-textsearch?pivots=programming-language-csharp
// - Register IVectorStoreRecordCollection
// - IVectorizedSearch
builder.Services.AddPostgresVectorStoreRecordCollection<Guid, ItsdTicket<Guid>>(
  appConfig.RagConfig.CollectionName
  );

//RegisterServices<string>(builder, kernelBuilder, appConfig);
RegisterServices<Guid>(builder, kernelBuilder, appConfig);


// Build and run the host.
using IHost host = builder.Build();
await host.RunAsync(appShutdownCancellationToken).ConfigureAwait(false);

static void RegisterServices<TKey>(HostApplicationBuilder builder, IKernelBuilder kernelBuilder, ApplicationConfig vectorStoreRagConfig)
    where TKey : notnull
{
    // Add a text search implementation that uses the registered vector store record collection for search.
    kernelBuilder.AddVectorStoreTextSearch<ItsdTicket<TKey>>(
        new TextSearchStringMapper((result) => (result as ItsdTicket<TKey>)!.text!),
        new TextSearchResultMapper((result) =>
        {
            // https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-vector-stores?pivots=programming-language-csharp
            // Create a mapping from the Vector Store data type to the data type returned by the Text Search.
            // This text search will ultimately be used in a plugin and this TextSearchResult will be returned to the prompt template
            // when the plugin is invoked from the prompt template.
            var castResult = result as ItsdTicket<TKey>;
            return new TextSearchResult(value: castResult!.text!) { Name = castResult.text, Value = castResult.metadata };
        }),
        new VectorStoreTextSearchOptions() 
        );

    // Add the key generator and data loader to the dependency injection container.
    builder.Services.AddSingleton<UniqueKeyGenerator<Guid>>(new UniqueKeyGenerator<Guid>(() => Guid.NewGuid()));
    builder.Services.AddSingleton<UniqueKeyGenerator<string>>(new UniqueKeyGenerator<string>(() => Guid.NewGuid().ToString()));
    //builder.Services.AddSingleton<IDataLoader, DataLoader<TKey>>();

    // Add the main service for this application.
    builder.Services.AddHostedService<RAGChatService<TKey>>();
}
