using Microsoft.AspNetCore.Mvc;

namespace RfidAppApi.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            var exception = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred",
                error = exception?.Error?.Message ?? "Unknown error",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
