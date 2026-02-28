using UnityEngine;
using Pokiwar.GameModes;

namespace Pokiwar.GameModes
{
    /// <summary>
    /// Free-For-All game mode: no time limit, respawn on death (lose half level), leaderboard by level.
    /// </summary>
    public class FFAMode : MonoBehaviour, IGameMode
    {
        private System.Action<string> gameOverCallback;

        public void Initialize()
        {
            Debug.Log("[FFA] Mode initialized - no time limit, respawn enabled");
        }

        public void Update()
        {
            // FFA has no win condition - continuous play
        }

        public void Cleanup()
        {
            gameOverCallback = null;
        }

        public bool IsPvPAllowed(Vector2 position)
        {
            // FFA: PvP always allowed (except safe zones handled by ZoneController)
            return true;
        }

        public bool ShouldRespawn()
        {
            // FFA: always respawn
            return true;
        }

        public void OnGameOver(System.Action<string> callback)
        {
            gameOverCallback = callback;
        }
    }
}
