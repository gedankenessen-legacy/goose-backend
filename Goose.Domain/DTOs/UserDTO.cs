using Goose.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class UserDTO : Document
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
