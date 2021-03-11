using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Goose.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public ErrorResponse Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;
            var code = StatusCodes.Status500InternalServerError;

            if (exception is HttpStatusException) code = (exception as HttpStatusException).Status;

            Response.StatusCode = code;

            return new ErrorResponse(exception, code);
        }
    }
}
