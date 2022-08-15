﻿using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.Caching.DataFiles.Models
{
    public class ArmouryItem
    {
        [JsonProperty(PropertyName = "ItemID")]
        public int ID;

        public string Name = "Item Name";

        public BonusType BonusType;
        public ItemGrade Grade = ItemGrade.Common;

        public float BaseEffect;
        public float LevelEffect;
    }
}