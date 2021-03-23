using Goose.Data.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Goose.Data.Context
{
    public interface IDbContext : IDisposable
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }

    public class DbContext : IDbContext
    {
        public IMongoDatabase Database { get; set; }
        public MongoClient MongoClient { get; set; }
        public IClientSessionHandle Session { get; set; }

        public DbContext(IOptions<DbSettings> configuration)
        {
            MongoClient = new MongoClient(configuration.Value.ConnectionString);
            Database = MongoClient.GetDatabase(configuration.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }

        public void Dispose()
        {
            Session?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
