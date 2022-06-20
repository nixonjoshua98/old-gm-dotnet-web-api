using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.RequestModels
{
    public class GameState
    {
        public int Stage;
    }

    public class LocalUserUnitState
    {
        public int ID;
        public int EnemiesDefeatedSincePrestige;
    }

    public class LocalGameState
    {
        public GameState GameState;

        [JsonProperty(PropertyName = "MercStates")]
        public List<LocalUserUnitState> UnitStates;
    }

    public class PrestigeBody
    {
        public LocalGameState LocalState;
    }
}
