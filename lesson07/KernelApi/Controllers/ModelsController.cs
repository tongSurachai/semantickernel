using Microsoft.AspNetCore.Mvc;

namespace KernelApi.Controllers;


[ApiController]
[Route("[controller]")]
public class ModelsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetModels()
    {
        var models = new
        {
            data = new[]
            {
                new
                {
                    id = "MyKernelApp",
                    obj = "model",
                    created = 1677610602,
                    owned_by = "ollama"
                }
            }
        };

        return Ok(models);
    }
}
