using GMServer.Models.MongoModels;
using GMServer.Models.RequestModels;
using GMServer.Models.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMServer.Calculations
{
    public class MercCalculations
    {
        public static List<MercUpdateModel> GetMercUpdateModels(List<LocalUserMercState> localStates, List<UserMerc> mercStates)
        {
            return mercStates.Select(merc =>
            {
                var state = localStates.First(m => m.ID == merc.MercID);

                return CreateMercUpdateModel(merc, state);

            }).ToList();
        }

        static MercUpdateModel CreateMercUpdateModel(UserMerc state, LocalUserMercState localState)
        {
            var model = new MercUpdateModel()
            {
                MercID = state.MercID,
                ExpertiseExp = XPEarned(localState)
            };

            while (true)
            {
                long xpRequired = XPToNextLevel(state.ExpertiseLevel + model.Levels);

                if (model.ExpertiseExp < xpRequired)
                    break;

                model.Levels++;
                model.ExpertiseExp -= xpRequired;
            }

            model.UpgradePoints = UpgradePointsEarned(model);

            return model;
        }

        static long XPEarned(LocalUserMercState localState)
        {
            return localState.EnemiesDefeatedSincePrestige * 10;
        }

        static int UpgradePointsEarned(MercUpdateModel model)
        {
            return model.Levels;
        }

        static long XPToNextLevel(int level)
        {
            return (long)(250 * Math.Pow(1.05, level));
        }
    }
}