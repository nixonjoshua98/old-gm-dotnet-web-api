using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.Models.DataFileModels
{
    public class ArmouryItem
    {
        [JsonProperty(PropertyName = "ItemID")]
        public int ID { get; set; }

        public BonusType BonusType { get; set; }

        public float BaseEffect { get; set; }

        public float LevelEffect { get; set; }
    }
}
