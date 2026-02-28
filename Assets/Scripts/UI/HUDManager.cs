using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.UI
{
    /// <summary>
    /// Singleton HUD manager. Coordinates all HUD elements and handles UI transitions.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("HUD Panels")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject levelBarPanel;
        [SerializeField] private GameObject minimapPanel;
        [SerializeField] private GameObject killFeedPanel;
        [SerializeField] private GameObject mobileControlsPanel;
        [SerializeField] private GameObject chatPanel;

        [Header("Overlay Panels")]
        [SerializeField] private DeathScreenUI deathScreen;
        [SerializeField] private EvolutionNotificationUI evolutionNotification;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ShowGameHUD();
        }

        public void ShowGameHUD()
        {
            SetPanelActive(leaderboardPanel, true);
            SetPanelActive(levelBarPanel, true);
            SetPanelActive(minimapPanel, true);
            SetPanelActive(killFeedPanel, true);
            SetPanelActive(mobileControlsPanel, Application.isMobilePlatform);
            SetPanelActive(chatPanel, true);
        }

        public void HideGameHUD()
        {
            SetPanelActive(leaderboardPanel, false);
            SetPanelActive(levelBarPanel, false);
            SetPanelActive(minimapPanel, false);
            SetPanelActive(killFeedPanel, false);
            SetPanelActive(mobileControlsPanel, false);
            SetPanelActive(chatPanel, false);
        }

        public void ShowDeathScreen(string killerName, int levelReached, int kills, float xpEarned, int foodCollected)
        {
            deathScreen?.Show(killerName, levelReached, kills, xpEarned, foodCollected);
        }

        public void HideDeathScreen()
        {
            deathScreen?.Hide();
        }

        public void ShowLevelUpNotification(int newLevel)
        {
            evolutionNotification?.ShowLevelUp(newLevel);
        }

        public void ShowEvolutionNotification(EvolutionTier newTier)
        {
            evolutionNotification?.ShowEvolution(newTier);
        }

        public void ShowKillFeedEntry(string killer, string victim)
        {
            KillFeedUI killFeed = killFeedPanel?.GetComponent<KillFeedUI>();
            killFeed?.AddEntry(killer, victim);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            panel?.SetActive(active);
        }
    }
}
