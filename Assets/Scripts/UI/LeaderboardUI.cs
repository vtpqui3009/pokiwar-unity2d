using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Pokiwar.Evolution;
using Pokiwar.Multiplayer;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays ranked leaderboard of players by level.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private int maxEntries = 10;
        [SerializeField] private float updateInterval = 1f;

        private float updateTimer;
        private List<EvolutionManager> allPlayers = new List<EvolutionManager>();

        private void Start()
        {
            if (leaderboardEntryPrefab != null)
            {
                for (int i = 0; i < maxEntries; i++)
                {
                    GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                    entry.SetActive(false);
                }
            }
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                RefreshLeaderboard();
            }
        }

        private void RefreshLeaderboard()
        {
            allPlayers.Clear();

            EvolutionManager[] localPlayers = FindObjectsOfType<EvolutionManager>();
            allPlayers.AddRange(localPlayers);

            var sortedPlayers = allPlayers
                .OrderByDescending(p => p.GetLevel())
                .ThenByDescending(p => p.GetXP())
                .Take(maxEntries)
                .ToList();

            UpdateLeaderboardDisplay(sortedPlayers);
        }

        private void UpdateLeaderboardDisplay(List<EvolutionManager> players)
        {
            if (leaderboardContainer == null)
                return;

            for (int i = 0; i < leaderboardContainer.childCount; i++)
            {
                Transform child = leaderboardContainer.GetChild(i);
                if (i < players.Count)
                {
                    child.gameObject.SetActive(true);
                    UpdateEntry(child, players[i], i + 1);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateEntry(Transform entry, EvolutionManager player, int rank)
        {
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = $"#{rank}";
                texts[1].text = $"Level {player.GetLevel()}";
            }
        }

        public void SetUpdateInterval(float interval)
        {
            updateInterval = interval;
        }
    }
}
