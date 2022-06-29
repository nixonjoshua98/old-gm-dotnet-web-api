using GMServer.Common;
using GMServer.Common.Classes;
using GMServer.Extensions;
using GMServer.Models.DataFileModels;
using GMServer.Models.RequestModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer
{
    public class ResolvedBonuses
    {
        List<BonusTypeValuePair> ArtefactBonuses;

        Dictionary<BonusType, double> Bonuses;

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

        void Add(List<UserArtefact> artefacts, List<Artefact> datafile)
        {
            ArtefactBonuses = CreateArtefactBonusList(artefacts, datafile);
        }

        void Calculate()
        {
            Bonuses = GameFormulas.CreateResolvedBonusDictionary(
                ArtefactBonuses
                );
        }

        static List<BonusTypeValuePair> CreateArtefactBonusList(List<UserArtefact> userArtefacts, List<Artefact> artefacts)
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
