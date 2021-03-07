using System;
using System.Collections.Generic;
using System.Text;

namespace Goose.Data.Settings
{
    public interface IDbSettings
    {
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
    }

    public class DbSettings : IDbSettings
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }
}
