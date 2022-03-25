using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.UserModels.DataFileModels
{
    public class Artefact
    {
        [JsonProperty(PropertyName = "ArtefactID")]
        public int ID { get; set; }

        public BonusType BonusType { get; set; }

        public int MaxLevel { get; set; } = 1_000;

        public float CostExpo { get; set; }

        public float CostCoeff { get; set; }

        public float BaseEffect { get; set; }

        public float LevelEffect { get; set; }
    }
}
