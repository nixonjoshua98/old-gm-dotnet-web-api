using MongoDB.Driver;
using System;
using Serilog;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public interface IMongoTransactionContext
    {
        Task<T> RunInTransaction<T>(Func<IClientSessionHandle, Task<T>> predicate);
    }

    public class MongoTransactionContext : IMongoTransactionContext
    {
        IMongoClient _mongo;

        public MongoTransactionContext(IMongoClient mongo)
        {
            _mongo = mongo;
        }

        public async Task<T> RunInTransaction<T>(Func<IClientSessionHandle, Task<T>> func)
        {
            T resp;

            using (IClientSessionHandle session = await _mongo.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    resp = await func(session);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    await session.AbortTransactionAsync();
                    throw;
                }

                await session.CommitTransactionAsync();
            }

            return resp;
        }
    }
}
