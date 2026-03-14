using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("auth service is running");
    }
}