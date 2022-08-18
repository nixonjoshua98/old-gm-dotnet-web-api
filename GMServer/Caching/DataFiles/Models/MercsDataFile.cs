using Newtonsoft.Json;
using SRC.Common.Enums;
using System.Collections.Generic;

namespace SRC.Caching.DataFiles.Models
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
        public Rarity Grade;
        public List<MercPassiveBonus> Passives;
    }

    public class MercsDataFile
    {
        public List<Merc> Mercs;
        public List<PassiveBonus> Passives;
    }
}
