using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.Models.DataFileModels
{
    public class Artefact
    {
        [JsonProperty(PropertyName = "ArtefactID")]
        public int ID;

        public BonusType BonusType;

        public int MaxLevel = 1_000;

        public float CostExpo;

        public float CostCoeff;

        public float BaseEffect;

        public float LevelEffect;
    }
}
