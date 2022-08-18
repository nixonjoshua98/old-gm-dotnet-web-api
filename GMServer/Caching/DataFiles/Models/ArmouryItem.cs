using Newtonsoft.Json;
using SRC.Common.Enums;

namespace SRC.Caching.DataFiles.Models
{
    public class ArmouryItem
    {
        [JsonProperty(PropertyName = "ItemID")]
        public int ID;

        public string Name = "Item Name";

        public BonusType BonusType;
        public Rarity Grade = Rarity.Common;

        public float BaseEffect;
        public float LevelEffect;
    }
}
