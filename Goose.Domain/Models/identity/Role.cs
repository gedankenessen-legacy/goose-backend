using Goose.Data.Models;

namespace Goose.Domain.Models.identity
{
    public class Role: Document
    {
        public string Name { get; set; }
    }
}