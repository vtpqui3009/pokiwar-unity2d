using UnityEngine;

namespace Pokiwar.Core
{
    /// <summary>
    /// Game mode options.
    /// </summary>
    public enum GameMode
    {
        FFA,
        Survival,
        Practice
    }

    /// <summary>
    /// Game phase lifecycle.
    /// </summary>
    public enum GamePhase
    {
        Lobby,
        Playing,
        GameOver
    }

    /// <summary>
    /// Static game state for runtime data access.
    /// </summary>
    public static class GameState
    {
        public static bool IsGameActive { get; set; }
        public static int PlayerCount { get; set; }
        public static float GameTime { get; set; }

        // Enhanced fields
        public static GameMode CurrentGameMode { get; set; }
        public static GamePhase CurrentPhase { get; set; }
        public static string RoomCode { get; set; }
        public static string SessionId { get; set; }
        public static GameObject TopPlayer { get; set; }

        static GameState()
        {
            IsGameActive = false;
            PlayerCount = 0;
            GameTime = 0f;
            CurrentGameMode = GameMode.FFA;
            CurrentPhase = GamePhase.Lobby;
            RoomCode = string.Empty;
            SessionId = string.Empty;
            TopPlayer = null;
        }

        public static void ResetGame()
        {
            IsGameActive = false;
            PlayerCount = 0;
            GameTime = 0f;
            CurrentPhase = GamePhase.Lobby;
            TopPlayer = null;
        }

        public static void StartGame(GameMode mode, string roomCode)
        {
            IsGameActive = true;
            CurrentGameMode = mode;
            CurrentPhase = GamePhase.Playing;
            RoomCode = roomCode;
            SessionId = System.Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        public static void EndGame()
        {
            IsGameActive = false;
            CurrentPhase = GamePhase.GameOver;
        }
    }
}
