using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Goose.Data.Repository
{
    public interface IRepository<TEntity> : IDisposable where TEntity : IDocument
    {
        IQueryable<TEntity> AsQueryable();
        IMongoCollection<TEntity> Collection();

        Task<IList<TEntity>> GetAsync();
        Task<TEntity> GetAsync(ObjectId id);
        Task<IList<TEntity>> GetAsync(IEnumerable<ObjectId> ids);
        Task<IList<TEntity>> FilterByAsync(Expression<Func<TEntity, bool>> filterExpression);

        Task CreateAsync(TEntity obj);

        Task<ReplaceOneResult> UpdateAsync(TEntity obj);

        Task<UpdateResult> UpdateByIdAsync(ObjectId id, UpdateDefinition<TEntity> update, UpdateOptions options = null);

        Task<DeleteResult> DeleteAsync(ObjectId id);
    }
}
