using GMServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class PassiveBonus
    {
        [JsonProperty(PropertyName = "PassiveID")]
        public int ID { get; set; }
        public BonusType BonusType { get; set; }
        public float BonusValue { get; set; }
    }

    public class MercPassiveBonus
    {
        public int PassiveID { get; set; }
        public int UnlockLevel { get; set; }
    }

    public class Merc
    {
        [JsonProperty(PropertyName = "MercID")]
        public int ID { get; set; }
        public string Name { get; set; }
        public float BaseDamage { get; set; }
        public int SpawnEnergyRequired { get; set; }
        public List<MercPassiveBonus> Passives;
    }

    public class MercsDataFile
    {
        public int MaxSquadSize { get; set; }
        public List<Merc> Mercs;
        public List<PassiveBonus> Passives;
    }
}
