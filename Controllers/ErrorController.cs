using Microsoft.AspNetCore.Mvc;
using demo2;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;


[ApiController]
[Route("[controller]")]
public class ErrorController : ControllerBase
{

    [Route("/error")]
    public IActionResult HandleError()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        // for this demo, this error will almost surely mean the database was sleeping and azure is now waking it up.
        if (ex?.InnerException is SqlException)
        {
            return StatusCode(500, "Database waking up. Please give it a minute and try again later.");
        }
        if (ex is Exception)
        {
            return StatusCode(500, "Something went wrong.");
        }

        return Problem();
    }   
}