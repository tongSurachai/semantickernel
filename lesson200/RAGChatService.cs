using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using VectorStoreRAG.Options;

namespace VectorStoreRAG;

/// <summary>
/// Main service class for the application.
/// </summary>
/// <typeparam name="TKey">The type of the data model key.</typeparam>
/// <param name="vectorStoreTextSearch">Used to search the vector store.</param>
/// <param name="kernel">Used to make requests to the LLM.</param>
/// <param name="ragConfigOptions">The configuration options for the application.</param>
/// <param name="appShutdownCancellationTokenSource">Used to gracefully shut down the entire application when cancelled.</param>
internal sealed class RAGChatService<TKey>(
//    IDataLoader dataLoader,
    VectorStoreTextSearch<ItsdTicket<TKey>> vectorStoreTextSearch,
    Kernel kernel,
    IOptions<RagConfig> ragConfigOptions,
    [FromKeyedServices("AppShutdown")] CancellationTokenSource appShutdownCancellationTokenSource) : IHostedService
{
    private Task? _dataLoaded;
    private Task? _chatLoop;

    /// <summary>
    /// Start the service.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>An async task that completes when the service is started.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._dataLoaded = Task.CompletedTask;

        // Start the chat loop.
        this._chatLoop = this.ChatLoopAsync(cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop the service.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>An async task that completes when the service is stopped.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Contains the main chat loop for the application.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>An async task that completes when the chat loop is shut down.</returns>
    private async Task ChatLoopAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Assistant > Press enter with no prompt to exit.");

        /*
         * original code
        var plugin = vectorStoreTextSearch.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(plugin);
        kernel.Plugins.AddFromType<Plugins>("inventory_plugin");
        */

        // Create options to describe the function I want to register.
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "Search",
            Description = "Perform a search for content related to the specified query from a record collection.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("top") { Description = "Number of results", IsRequired = false, DefaultValue = 10 },
                new KernelParameterMetadata("skip") { Description = "Number of results to skip", IsRequired = false, DefaultValue = 0 },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
        };

        // Build a text search plugin with vector store search and add to the kernel
        //var searchPlugin = vectorStoreTextSearch.CreateWithGetTextSearchResults("SearchPlugin", "Search a record collection", [vectorStoreTextSearch.CreateSearch(options)]);
        var searchPlugin = vectorStoreTextSearch.CreateWithGetTextSearchResults("SearchTicket", "Search for itsd's ticket");
        kernel.Plugins.Add(searchPlugin);

        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        KernelArguments arguments = new(settings);


        // Start the chat loop.
        while (!cancellationToken.IsCancellationRequested)
        {
            // Prompt the user for a question.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Assistant > Ask ITSD");

            // Read the user question.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("User > ");
            var question = Console.ReadLine();

            // Exit the application if the user didn't type anything.
            if (string.IsNullOrWhiteSpace(question))
            {
                appShutdownCancellationTokenSource.Cancel();
                break;
            }

            var response = kernel.InvokePromptStreamingAsync(
                promptTemplate: question,
                arguments: arguments,
                cancellationToken: cancellationToken);

            /*
            // Invoke the LLM with a template that uses the search plugin to
            // 1. get related information to the user query from the vector store
            // 2. add the information to the LLM prompt.
            var prompt = """
                    <message role="system">
                    You are a precise, autoregressive question-answering system.
                    Use the provided tools to gather the necessary context and then answer the user's questions.

                    Obtain information about tickets and cases to use as context.

                    Context:
                        {{#with (SearchPlugin-GetTextSearchResults question)}}  
                          {{#each this}}  
                            {{Name}}
                            -----------------
                          {{/each}}
                        {{/with}}
                    Answer:

                    Guidelines for Summarizing Answer

                    1. You will summarize the context of a series of case/ticket/email provided by the user.
                    2. You will present all key topics discussed in the case/ticket/email thread as a bullet point list to enhance clarity.
                    3. Next Steps Recommendation: I will recommend actionable next steps based on the summarization for clarity and progression.

                    Output format:
                    1. Context Summary: Overview of main topics from the email thread.
                          Use Markdown to visually present information in an appealing way. 
                          Use bold text to emphasize important words or sentences.
                          Use concise lists to present multiple items or options.
                    2. Key Topics:
                          First, determine the text structure and key points, then use Markdown syntax to highlight these structures and key points.
                         Bullet points summarizing major points or discussions.
                    3. Next Steps:
                         Recommended actions or follow-ups for clarity and progression.

                    
                    **Include citations to relevant information** where it is referenced in the response.

                    **Make sure to address the customer by name** in the response.
                    </message>
                    <message role="user">
                       You are an assistant for personalized question-answering assistant. 

                        Provide concise and accurate answers based on the retrieved context, with a personalized greeting and response.
                        Use the provided context to answer the question.
                        If the answer is not known, explicitly state that you don't know.
                        Ensure that the answer is brief and to the point.
                        Question:  {{question}}

                    </message>
                    """;

            // Create the prompt template using handlebars format
            var templateFactory = new HandlebarsPromptTemplateFactory();
            var promptTemplateConfig = new PromptTemplateConfig()
            {
                Template = prompt,
                TemplateFormat = "handlebars",
                Name = "ContosoChatPrompt",
            };

            var arguments = new KernelArguments()
            { 
                    { "question", question }

            };

            // Render the prompt
            var promptTemplate = templateFactory.Create(promptTemplateConfig);
            var renderedPrompt = await promptTemplate.RenderAsync(kernel, arguments, cancellationToken).ConfigureAwait(true);
            Console.WriteLine($"Rendered Prompt:\n{renderedPrompt}\n");

            var response = kernel.InvokePromptStreamingAsync(
                promptTemplate: prompt,
                arguments: arguments,
                templateFormat: "handlebars",
                promptTemplateFactory: templateFactory,
                cancellationToken: cancellationToken);
            */


            // Stream the LLM response to the console with error handling.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nAssistant > ");

            try
            {
                await foreach (var message in response.ConfigureAwait(false))
                {
                    Console.Write(message);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Call to LLM failed with error: {ex}");
            }
        }
    }

}
