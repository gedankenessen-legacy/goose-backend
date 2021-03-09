using Goose.Data.Models;

namespace Goose.Domain.Models.identity
{
    public class User: Document
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string HashedPassword { get; set; }
    }
}