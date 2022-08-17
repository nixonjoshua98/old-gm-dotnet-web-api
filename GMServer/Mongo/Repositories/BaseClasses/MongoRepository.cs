using MongoDB.Driver;
using SRC.Mongo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SRC.Mongo.Repositories.BaseClasses
{
    public interface IMongoRepository<TDocument>
    {
        FilterDefinitionBuilder<TDocument> Filter { get; }
        UpdateDefinitionBuilder<TDocument> Update { get; }

        IAggregateFluent<TDocument> Aggregate();
        Task DeleteManyAsync(FilterDefinition<TDocument> filter);
        Task<List<TField>> DistinctAsync<TField>(Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter);
        Task<List<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filter);
        Task<List<TDocument>> FindAsync(FilterDefinition<TDocument> filter);
        Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter);

        Task<TDocument> FindOneAndUpdateAsync(Expression<Func<TDocument, bool>> filter,
                                              UpdateDefinition<TDocument> update,
                                              bool isUpsert,
                                              ReturnDocument returnDocument = ReturnDocument.After);

        Task<TDocument> FindOneAndUpdateAsync(IClientSessionHandle session,
                                              Expression<Func<TDocument, bool>> filter,
                                              UpdateDefinition<TDocument> update,
                                              bool upsert,
                                              ReturnDocument returnDocument = ReturnDocument.After);

        Task<TDocument> FindOneAndUpdateAsync(Expression<Func<TDocument, bool>> filter,
                                              Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update,
                                              bool isUpsert,
                                              ReturnDocument returnDocument = ReturnDocument.After);

        Task InsertManyAsync(List<TDocument> documents);
        Task InsertOneAsync(TDocument document);
        Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update);
        Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update, bool upsert);
        Task UpdateOneAsync(IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, bool upsert);

        Task UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);
        Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update);

        Task BulkWriteAsync<T>(List<T> ls) where T : WriteModel<TDocument>;
        Task ReplaceOneAsync(Expression<Func<TDocument, bool>> filter, TDocument document, bool isUpsert);
    }

    public abstract class MongoRepository<TDocument> : IMongoRepository<TDocument>
    {
        private readonly IMongoCollection<TDocument> _collection;

        public UpdateDefinitionBuilder<TDocument> Update => Builders<TDocument>.Update;
        public FilterDefinitionBuilder<TDocument> Filter => Builders<TDocument>.Filter;

        public MongoRepository(IMongoClient mongo, string name)
        {
            _collection = mongo.GetCollection<TDocument>(name);
        }

        public IAggregateFluent<TDocument> Aggregate()
        {
            return _collection.Aggregate();
        }

        public async Task BulkWriteAsync<T>(List<T> ls) where T : WriteModel<TDocument>
        {
            await _collection.BulkWriteAsync(ls);
        }

        public async Task ReplaceOneAsync(Expression<Func<TDocument, bool>> filter, TDocument document, bool isUpsert)
        {
            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions() { IsUpsert = isUpsert });
        }

        public async Task InsertManyAsync(List<TDocument> documents)
        {
            await _collection.InsertManyAsync(documents);
        }

        public async Task<List<TField>> DistinctAsync<TField>(Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter)
        {
            return await _collection.Distinct(field, filter).ToListAsync();
        }

        public async Task<TDocument> FindOneAndUpdateAsync(Expression<Func<TDocument, bool>> filter,
                                                           UpdateDefinition<TDocument> update,
                                                           bool isUpsert = false,
                                                           ReturnDocument returnDocument = ReturnDocument.After)
        {
            return await _collection.FindOneAndUpdateAsync(filter, update, new()
            {
                IsUpsert = isUpsert,
                ReturnDocument = returnDocument
            });
        }

        public async Task<TDocument> FindOneAndUpdateAsync(IClientSessionHandle session, Expression<Func<TDocument, bool>> filter,
                                                   UpdateDefinition<TDocument> update,
                                                   bool isUpsert = false,
                                                   ReturnDocument returnDocument = ReturnDocument.After)
        {
            return await _collection.FindOneAndUpdateAsync(session, filter, update, new()
            {
                IsUpsert = isUpsert,
                ReturnDocument = returnDocument
            });
        }

        public async Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<TDocument>> FindAsync(FilterDefinition<TDocument> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task DeleteManyAsync(FilterDefinition<TDocument> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }

        public async Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update)
        {
            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task InsertOneAsync(TDocument document)
        {
            await _collection.InsertOneAsync(document);
        }

        public async Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update)
        {
            await _collection.FindOneAndUpdateAsync(filter, update(Update));
        }

        public void UpdateOne(IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update)
        {
            _collection.FindOneAndUpdate(session, filter, update(Update));
        }

        public async Task<TDocument> FindOneAndUpdateAsync(Expression<Func<TDocument, bool>> filter,
                                                           Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update,
                                                           bool upsert,
                                                           ReturnDocument returnDocument = ReturnDocument.After)
        {
            return await _collection.FindOneAndUpdateAsync(filter, update(Update), new()
            {
                IsUpsert = upsert,
                ReturnDocument = returnDocument
            });
        }

        public async Task UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
        {
            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> update, bool upsert)
        {
            await _collection.UpdateOneAsync(filter, update(Update), new UpdateOptions() { IsUpsert = upsert });
        }

        public async Task UpdateOneAsync(IClientSessionHandle session,
                                         Expression<Func<TDocument, bool>> filter,
                                         UpdateDefinition<TDocument> update,
                                         bool upsert)
        {
            await _collection.UpdateOneAsync(session, filter, update, new UpdateOptions() { IsUpsert = upsert });
        }
    }
}
