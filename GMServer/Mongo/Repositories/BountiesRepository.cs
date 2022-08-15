using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IBountiesRepository : BaseClasses.IMongoRepository<UserBounties>
    {
        FilterDefinition<UserBounties> BountyFilter(string userId, int bountyId);
        FilterDefinition<UserBounties> LockedBountyFilter(string userId, int bountyId);
    }

    public class BountiesRepository : BaseClasses.MongoRepository<UserBounties>, IBountiesRepository
    {
        public BountiesRepository(IMongoClient mongo) : base(mongo, "GMServer.UserBounties")
        {

        }

        public FilterDefinition<UserBounties> BountyFilter(string userId, int bountyId)
        {
            return Filter.Where(x => x.UserID == userId) &
                   Filter.ElemMatch(doc => doc.UnlockedBounties, doc => doc.BountyID == bountyId);
        }

        /// <summary>
        /// Match upon a document if the bounty is not unlocked (provides uniqueness at the code level)
        /// </summary>
        public FilterDefinition<UserBounties> LockedBountyFilter(string userId, int bountyId)
        {
            return Filter.Eq(x => x.UserID, userId) &
                   Filter.Not(
                       Filter.ElemMatch(x => x.UnlockedBounties, x => x.BountyID == bountyId)
                   );
        }
    }
}