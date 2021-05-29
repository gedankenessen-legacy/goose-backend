using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils
{
    public static class StringExtentions
    {
        public static bool isNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool isNullOrWhitespace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
