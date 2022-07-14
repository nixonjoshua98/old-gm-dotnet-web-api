using GMServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class PassiveBonus
    {
        [JsonProperty(PropertyName = "PassiveID")]
        public int ID;

        public BonusType BonusType;

        public float BonusValue;
    }

    public class MercPassiveBonus
    {
        public int PassiveID;
        public int UnlockLevel;
    }

    public class Merc
    {
        [JsonProperty(PropertyName = "MercID")]
        public int ID;

        public string Name;
        public float BaseDamage;
        public ItemGrade Grade;
        public float RechargeRate;
        public List<MercPassiveBonus> Passives;
    }

    public class MercsDataFile
    {
        public List<Merc> Mercs;
        public List<PassiveBonus> Passives;
    }
}
