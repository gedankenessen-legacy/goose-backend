using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Goose.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        
        public ErrorController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Route("error")]
        public ErrorResponse Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;
            var code = StatusCodes.Status500InternalServerError;

            if (exception is HttpStatusException statusException)         
                code = statusException.Status; 
            
            Response.StatusCode = code;

            return new ErrorResponse(exception, code, _env.IsDevelopment() ? exception.StackTrace : "");
        }
    }
}
