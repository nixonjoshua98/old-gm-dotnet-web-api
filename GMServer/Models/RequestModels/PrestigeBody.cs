﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.RequestModels
{
    public class GameState
    {
        public int Stage;
    }

    public class LocalUserMercState
    {
        public int ID;
        public int EnemiesDefeatedSincePrestige;
    }

    public class LocalGameState
    {
        public GameState GameState;

        public List<LocalUserMercState> MercStates;
    }

    public class PrestigeBody
    {
        public LocalGameState LocalState;
    }
}
