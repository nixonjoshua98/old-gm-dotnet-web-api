using Newtonsoft.Json;
using SRC.Common.Enums;

namespace SRC.Caching.DataFiles.Models
{
    public class Artefact
    {
        [JsonProperty(PropertyName = "ArtefactID")]
        public int ID;

        public string Name = "Artefact Name";

        public BonusType BonusType;

        public Rarity GradeID;

        public int MaxLevel = 1_000;

        public float CostExpo;

        public float CostCoeff;

        public float BaseEffect;

        public float LevelEffect;
    }
}
