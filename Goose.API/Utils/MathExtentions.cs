using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils
{
    public static class MathExtentions
    {
        public static double Round(this double value,  int digits)
        {
            return Math.Round(value, digits);
        }
    }
}
