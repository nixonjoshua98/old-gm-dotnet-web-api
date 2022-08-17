using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.Mongo
{
    public interface IMongoSessionFactory
    {
        Task<T> RunInTransaction<T>(Func<IClientSessionHandle, CancellationToken, Task<T>> predicate);
    }

    public class MongoSessionFactory : IMongoSessionFactory
    {
        private readonly IMongoClient _client;

        public MongoSessionFactory(IMongoClient mongo)
        {
            _client = mongo;
        }

        public async Task<T> RunInTransaction<T>(Func<IClientSessionHandle, CancellationToken, Task<T>> func)
        {
            using var session = await _client.StartSessionAsync();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            return await session.WithTransactionAsync(func, cancellationToken: cts.Token);
        }
    }
}
