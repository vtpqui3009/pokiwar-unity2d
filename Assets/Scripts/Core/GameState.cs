using UnityEngine;

namespace Pokiwar.Core
{
    /// <summary>
    /// Static game state for runtime data access.
    /// </summary>
    public static class GameState
    {
        public static bool IsGameActive { get; set; }
        public static int PlayerCount { get; set; }
        public static float GameTime { get; set; }

        static GameState()
        {
            IsGameActive = false;
            PlayerCount = 0;
            GameTime = 0f;
        }

        public static void ResetGame()
        {
            IsGameActive = false;
            PlayerCount = 0;
            GameTime = 0f;
        }
    }
}
