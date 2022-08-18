using MongoDB.Driver;
using SRC.Caching.DataFiles.Models;
using SRC.Common;
using SRC.Common.Classes;
using SRC.Common.Enums;
using SRC.Extensions;
using SRC.Mongo.Models;
using System.Collections.Generic;
using System.Linq;

namespace SRC
{
    public class ResolvedBonuses
    {
        private List<BonusTypeValuePair> ArtefactBonuses;
        private Dictionary<BonusType, double> Bonuses;

        /// <summary>
        /// Preferred creation method
        /// </summary>
        public static ResolvedBonuses Create(
            List<UserArtefact> artefacts, List<Artefact> artefactsDatafile
            )
        {
            var inst = new ResolvedBonuses();

            inst.Add(artefacts, artefactsDatafile);

            inst.Calculate();

            return inst;
        }

        /// <summary>
        /// Pull a resolved bonus type (or default value)
        /// </summary>
        public double Get(BonusType bonus)
        {
            double defaultValue = bonus switch
            {
                BonusType.FLAT_CRIT_CHANCE => 0.0,
                BonusType.FLAT_TAP_DMG => 0.0,
                _ => 1
            };

            return Bonuses.Get(bonus, defaultValue);
        }

        /// <summary>
        /// Calculate the prestige points at stage using the resolved bonuses
        /// </summary>
        public double PrestigePointsAtStage(int stage)
        {
            return GameFormulas.PrestigePointsBase(stage) * Get(BonusType.MULTIPLY_PRESTIGE_BONUS);
        }

        private void Add(List<UserArtefact> artefacts, List<Artefact> datafile)
        {
            ArtefactBonuses = CreateArtefactBonusList(artefacts, datafile);
        }

        private void Calculate()
        {
            Bonuses = GameFormulas.CreateResolvedBonusDictionary(
                ArtefactBonuses
                );
        }

        private static List<BonusTypeValuePair> CreateArtefactBonusList(List<UserArtefact> userArtefacts, List<Artefact> artefacts)
        {
            List<BonusTypeValuePair> ls = new();

            artefacts.ForEach(art =>
            {
                var state = userArtefacts.FirstOrDefault(x => x.ArtefactID == art.ID);

                if (state is not null)
                {
                    double bonusValue = GameFormulas.ArtefactEffectBase(state, art);

                    ls.Add(new(art.BonusType, bonusValue));
                }
            });

            return ls;
        }
    }
}
