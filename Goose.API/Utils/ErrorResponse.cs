using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }

        public ErrorResponse(Exception ex, int status, string stackTrace = "")
        {
            Type = ex.GetType().Name;
            Message = ex.Message;
            Status = status;
            StackTrace = stackTrace;
        }
    }
}
