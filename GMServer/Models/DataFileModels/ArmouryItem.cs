using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.Models.DataFileModels
{
    public class ArmouryItem
    {
        [JsonProperty(PropertyName = "ItemID")]
        public int ID;

        public string Name = "Item Name";

        public BonusType BonusType;
        public ItemGradeID Grade = ItemGradeID.Common;

        public float BaseEffect;
        public float LevelEffect;
    }
}
