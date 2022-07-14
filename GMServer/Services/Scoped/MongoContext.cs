using MongoDB.Driver;
using System;
using Serilog;
using System.Threading.Tasks;
using GMServer.Common;

namespace GMServer.Services
{
    //public interface IMongoContext
    //{
    //    IClientSessionHandle Session { get; }
    //    bool HasSession { get; }
    //    Task<T> RunInTransaction<T>(Func<IClientSessionHandle, Task<T>> predicate);
    //}

    //public class MongoContext : IMongoContext
    //{
    //    IMongoClient _client;

    //    public IClientSessionHandle Session { get; private set; }
    //    public bool HasSession => Session is not null;

    //    public MongoContext(IMongoClient mongo)
    //    {
    //        _client = mongo;
    //    }

    //    public async Task<T> RunInTransaction<T>(Func<IClientSessionHandle, Task<T>> func)
    //    {
    //        Guard.ThrowIfNotNull(Session, "Mongo session already running");

    //        T resp;

    //        using (var localSession = await _client.StartSessionAsync())
    //        {
    //            Session = localSession;

    //            localSession.StartTransaction();

    //            try
    //            {
    //                resp = await func(Session);
    //            }
    //            catch (Exception ex)
    //            {
    //                Log.Error($"Mongo Transaction - {ex.Message}");
    //                await localSession.AbortTransactionAsync();
    //                throw;
    //            }
    //            finally
    //            {
    //                Session = null;
    //            }

    //            await localSession.CommitTransactionAsync();
    //        }

    //        return resp;
    //    }
    //}
}
