using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GMServer.Repositories
{
    public abstract class AbstractMongoRepository<TDocument>
    {
        private IMongoCollection<TDocument> _collection;

        public AbstractMongoRepository(IMongoDatabase mongo, string name)
        {
            _collection = mongo.GetCollection<TDocument>(name);
        }

        protected async Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        protected async Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinition<TDocument>> update)
        {
            await _collection.UpdateOneAsync(filter, update());
        }

        protected async Task InsertOneAsync(TDocument document)
        {
            await _collection.InsertOneAsync(document);
        }

        protected UpdateDefinitionBuilder<TDocument> Update => Builders<TDocument>.Update;
    }
}
