using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils
{
    public class TokenSettings : ITokenSettings
    {
        public double ExpireInHours { get; set; }
        public string Secret { get; set; }
    }

    interface ITokenSettings
    {
        double ExpireInHours { get; set; }
        string Secret { get; set; }
    }
}
