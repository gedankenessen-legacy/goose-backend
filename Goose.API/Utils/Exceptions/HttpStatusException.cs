using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils.Exceptions
{
    public class HttpStatusException : Exception
    {
        public int Status { get; private set; }

        public HttpStatusException(int status, string msg) : base(msg)
        {
            Status = status;
        }
    }
}
