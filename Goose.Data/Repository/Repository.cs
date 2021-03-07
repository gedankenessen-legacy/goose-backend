using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Goose.Data.Context;
using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Goose.Data.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : IDocument
    {
        protected readonly IDbContext _dbContext;
        protected IMongoCollection<TEntity> _dbCollection;

        protected Repository(IDbContext context, string alternativCollectionName = null)
        {
            _dbContext = context;
            _dbCollection = _dbContext.GetCollection<TEntity>(alternativCollectionName ?? typeof(TEntity).Name);
        }

        #region Read

        public virtual async Task<IList<TEntity>> GetAsync()
        {
            return await _dbCollection.Find(Builders<TEntity>.Filter.Empty).ToListAsync();
        }

        public virtual async Task<TEntity> GetAsync(string id)
        {
            var objectId = new ObjectId(id);

            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("_id", objectId);

            return await _dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task<IList<TEntity>> FilterByAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await _dbCollection.Find(filterExpression).ToListAsync();
        }

        #endregion

        #region Write

        public virtual Task CreateAsync(TEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException($"{typeof(TEntity).Name} object is null");

            return _dbCollection.InsertOneAsync(obj);
        }

        public virtual Task<ReplaceOneResult> UpdateAsync(TEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException($"{typeof(TEntity).Name} object is null");

            var filter = Builders<TEntity>.Filter.Eq(doc => doc.Id, obj.Id);

            return _dbCollection.ReplaceOneAsync(filter, obj);
        }

        #endregion

        #region Delete

        public virtual Task<DeleteResult> DeleteAsync(string id)
        {
            return _dbCollection.DeleteOneAsync(Builders<TEntity>.Filter.Eq("_id", id));
        }

        #endregion

        #region Etc

        public virtual IQueryable<TEntity> AsQueryable()
        {
            return _dbCollection.AsQueryable();
        }

        public virtual IMongoCollection<TEntity> Collection()
        {
            return _dbCollection;
        }

        #endregion

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
