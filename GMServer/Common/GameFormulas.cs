using GMServer.Common.Classes;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMServer.Common
{
    public static class GameFormulas
    {
        public static double SumNonIntegerPowerSeq(int start, int total, float exponent)
        {
            // https://math.stackexchange.com/questions/82588/is-there-a-formula-for-sums-of-consecutive-powers-where-the-powers-are-non-inte

            double Predicate(int startValue)
            {
                double x = Math.Pow(startValue, exponent + 1) / (exponent + 1);
                double y = Math.Pow(startValue, exponent) / 2;
                double z = Math.Sqrt(Math.Pow(startValue, exponent - 1));

                return x + y + z;
            }

            return Predicate(start + total) - Predicate(start);
        }

        public static long MercXPEarned(int numEnemyDefeats)
        {
            return numEnemyDefeats * 10;
        }

        public static List<BonusTypeValuePair> CreateArtefactBonusList(List<UserArtefact> userArtefacts, List<Artefact> artefacts)
        {
            List<BonusTypeValuePair> ls = new();

            artefacts.ForEach(art =>
            {
                var state = userArtefacts.FirstOrDefault(x => x.ArtefactID == art.ID);

                if (state is not null)
                {
                    double bonusValue = ArtefactBaseEffect(state, art);

                    ls.Add(new(art.BonusType, bonusValue));
                }
            });

            return ls;
        }

        public static double ArtefactBaseEffect(UserArtefact userArtefact, Artefact artefact)
        {
            return artefact.BaseEffect + (artefact.LevelEffect * (userArtefact.Level - 1));
        }

        public static double BasePrestigePoints(int stage)
        {
            return Math.Pow(Math.Ceiling((stage - 65) / 10.0f), 2.2);
        }

        public static Dictionary<BonusType, double> CreateResolvedBonusDictionary(IEnumerable<BonusTypeValuePair> ls)
        {
            Dictionary<BonusType, double> result = new();

            foreach (var pair in ls)
            {
                if (!result.TryGetValue(pair.BonusType, out double totalValue))
                {
                    result[pair.BonusType] = pair.Value;
                }
                else
                {
                    result[pair.BonusType] = pair.BonusType switch
                    {
                        BonusType.FLAT_CRIT_CHANCE => totalValue + pair.Value,
                        BonusType.FLAT_TAP_DMG => totalValue + pair.Value,

                        _ => totalValue * pair.Value
                    };
                }
            }

            return result;
        }
    }
}
