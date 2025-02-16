using Microsoft.AspNetCore.Mvc;

namespace KernelApi.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    // POST /v1/chat/completions
    [HttpPost("/v1/chat/completions")]
    public IActionResult RedirectToChatCompletionsV1()
    {
        return RedirectToAction("Completions", "Chat");
    }

    // POST /api/chat/completions
    [HttpPost("/api/chat/completions")]
    public IActionResult RedirectToChatCompletionsApi()
    {
        return RedirectToAction("Completions", "Chat");
    }
}