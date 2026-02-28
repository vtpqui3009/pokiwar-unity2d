using UnityEngine;
using Pokiwar.Core;
using Pokiwar.Evolution;
using Pokiwar.World;

namespace Pokiwar.GameModes
{
    /// <summary>
    /// Survival game mode: shrinking safe zone, no respawn, last player alive wins.
    /// Match duration ~10 minutes.
    /// </summary>
    public class SurvivalMode : MonoBehaviour, IGameMode
    {
        [Header("Settings")]
        [SerializeField] private float matchDuration = 600f; // 10 minutes

        private System.Action<string> gameOverCallback;
        private float matchTimer;
        private bool isActive;
        private int alivePlayers;
        private MapBorderController borderController;

        public float TimeRemaining => Mathf.Max(0f, matchDuration - matchTimer);
        public bool IsActive => isActive;

        public void Initialize()
        {
            isActive = true;
            matchTimer = 0f;

            borderController = FindObjectOfType<MapBorderController>();

            // Count initial players
            alivePlayers = GameObject.FindGameObjectsWithTag("Player").Length;

            Debug.Log($"[Survival] Mode initialized - {alivePlayers} players, {matchDuration}s match");
        }

        public void Update()
        {
            if (!isActive) return;

            matchTimer += Time.deltaTime;

            // Check win condition
            CheckWinCondition();

            // Time limit
            if (matchTimer >= matchDuration)
            {
                DeclareWinner();
            }
        }

        private void CheckWinCondition()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int alive = 0;
            GameObject lastAlive = null;

            foreach (GameObject p in players)
            {
                Combat.HealthController hc = p.GetComponent<Combat.HealthController>();
                if (hc != null && !hc.IsDead)
                {
                    alive++;
                    lastAlive = p;
                }
            }

            if (alive <= 1 && alivePlayers > 1)
            {
                string winnerName = "Unknown";
                if (lastAlive != null)
                {
                    EvolutionManager em = lastAlive.GetComponent<EvolutionManager>();
                    winnerName = em?.GetPlayerName() ?? lastAlive.name;
                }

                EndGame(winnerName);
            }
        }

        private void DeclareWinner()
        {
            // Find highest level player as winner
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            string winnerName = "Unknown";
            int topLevel = 0;

            foreach (GameObject p in players)
            {
                EvolutionManager em = p.GetComponent<EvolutionManager>();
                if (em != null && em.GetLevel() > topLevel)
                {
                    topLevel = em.GetLevel();
                    winnerName = em.GetPlayerName();
                }
            }

            EndGame(winnerName);
        }

        private void EndGame(string winner)
        {
            isActive = false;
            GameState.EndGame();
            gameOverCallback?.Invoke(winner);
            Debug.Log($"[Survival] Game over! Winner: {winner}");
        }

        public void Cleanup()
        {
            isActive = false;
            gameOverCallback = null;
        }

        public bool IsPvPAllowed(Vector2 position)
        {
            return true; // Survival: PvP always allowed
        }

        public bool ShouldRespawn()
        {
            return false; // Survival: no respawn
        }

        public void OnGameOver(System.Action<string> callback)
        {
            gameOverCallback = callback;
        }
    }
}
