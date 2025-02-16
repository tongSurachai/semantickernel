using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace KernelApi.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly KernelService _kernelService;

        public ChatController(KernelService kernelService)
        {
            _kernelService = kernelService;
        }

        [HttpPost("completions2")]
        public async IAsyncEnumerable<ChatResponse> GetChatCompletion2( [FromBody] ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (request.Messages.Length == 0)
            {
                yield break;
            }

            var messages = request.Messages.Select(m => m.Content).ToList();
            var lastMessage = messages.LastOrDefault() ?? "";

            await foreach (var responseSegment in _kernelService.GetCompletionResponseStreamAsync(lastMessage, cancellationToken))
            {
                var chatResponse = new ChatResponse
                {
                    Id = "chat-completion-" + Guid.NewGuid().ToString("N"),
                    Obj = "chat.completion",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = "llama2",
                    Choices = new[]
                    {
                        new Choice
                        {
                            Index = 0,
                            Message = new Message
                            {
                                Role = "assistant",
                                Content = responseSegment
                            },
                            FinishReason = "stop"
                        }
                    },
                    Usage = new Usage
                    {
                        PromptTokens = 0,
                        CompletionTokens = 0,
                        TotalTokens = 0
                    }
                };

                yield return chatResponse;
            }
        }
        
        
        [HttpPost("completions")]
        public async Task<IActionResult> GetChatCompletion(
            [FromBody] ChatRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Messages.Length == 0)
            {
                return BadRequest("Messages cannot be null or empty.");
            }

            var messages = request.Messages.Select(m => m.Content).ToList();
            var lastMessage = messages.LastOrDefault() ?? "";

            var response = await _kernelService.GetCompletionResponseAsync(lastMessage);

            var chatResponse = new ChatResponse
            {
                Id = "chat-completion-" + Guid.NewGuid().ToString("N"),
                Obj = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "llama2",
                Choices = new[]
                {
                    new Choice
                    {
                        Index = 0,
                        Message = new Message
                        {
                            Role = "assistant",
                            Content = response
                        },
                        FinishReason = "stop"
                    }
                },
                Usage = new Usage
                {
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0
                }
            };

            return Ok(chatResponse);
        }

    }
}
