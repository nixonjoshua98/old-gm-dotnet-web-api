﻿using GMServer.Common;
using Newtonsoft.Json;

namespace GMServer.Models.DataFileModels
{
    public class Artefact
    {
        [JsonProperty(PropertyName = "ArtefactID")]
        public int ID;

        public string Name = "Artefact Name";

        public BonusType BonusType;

        public ItemGrade GradeID;

        public int MaxLevel = 1_000;

        public float CostExpo;

        public float CostCoeff;

        public float BaseEffect;

        public float LevelEffect;
    }
}
