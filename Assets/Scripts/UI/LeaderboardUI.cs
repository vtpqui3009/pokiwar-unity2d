using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Pokiwar.Evolution;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays ranked leaderboard of players by level with name, kills, rank change animations.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private int maxEntries = 10;
        [SerializeField] private float updateInterval = 1f;

        [Header("Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color localPlayerColor = new Color(0.4f, 1f, 0.4f);

        private float updateTimer;
        private List<EvolutionManager> allPlayers = new List<EvolutionManager>();
        private List<EvolutionManager> previousOrder = new List<EvolutionManager>();
        private GameObject localPlayer;

        private void Start()
        {
            if (leaderboardEntryPrefab != null && leaderboardContainer != null)
            {
                for (int i = 0; i < maxEntries; i++)
                {
                    GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                    entry.SetActive(false);
                }
            }

            localPlayer = GameObject.FindGameObjectWithTag("Player");
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
            EvolutionManager[] found = FindObjectsOfType<EvolutionManager>();
            allPlayers.AddRange(found);

            var sortedPlayers = allPlayers
                .OrderByDescending(p => p.GetLevel())
                .ThenByDescending(p => p.GetXP())
                .Take(maxEntries)
                .ToList();

            UpdateLeaderboardDisplay(sortedPlayers);
            previousOrder = new List<EvolutionManager>(sortedPlayers);
        }

        private void UpdateLeaderboardDisplay(List<EvolutionManager> players)
        {
            if (leaderboardContainer == null) return;

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
            Image background = entry.GetComponent<Image>();

            // Determine rank change
            int previousRank = previousOrder.IndexOf(player) + 1;
            string rankChangeSymbol = "";
            if (previousRank > 0 && previousRank != rank)
                rankChangeSymbol = rank < previousRank ? " ▲" : " ▼";

            // Set text fields: [0]=rank, [1]=name, [2]=level, [3]=kills
            if (texts.Length >= 1) texts[0].text = $"#{rank}{rankChangeSymbol}";
            if (texts.Length >= 2) texts[1].text = player.GetPlayerName();
            if (texts.Length >= 3) texts[2].text = $"Lv.{player.GetLevel()}";
            if (texts.Length >= 4) texts[3].text = $"{player.GetKillCount()} kills";

            // Color coding
            Color entryColor = Color.white;
            if (rank == 1) entryColor = goldColor;
            else if (rank == 2) entryColor = silverColor;
            else if (rank == 3) entryColor = bronzeColor;

            // Highlight local player
            bool isLocal = player.gameObject == localPlayer ||
                           (localPlayer != null && player.gameObject == localPlayer);
            if (isLocal) entryColor = localPlayerColor;

            foreach (TextMeshProUGUI t in texts)
                t.color = entryColor;
        }

        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }
    }
}
