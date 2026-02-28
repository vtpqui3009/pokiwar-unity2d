using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Pokiwar.UI
{
    /// <summary>
    /// Death screen overlay showing defeat info, stats, and respawn countdown.
    /// </summary>
    public class DeathScreenUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI defeatedByText;
        [SerializeField] private TextMeshProUGUI levelReachedText;
        [SerializeField] private TextMeshProUGUI killsText;
        [SerializeField] private TextMeshProUGUI xpEarnedText;
        [SerializeField] private TextMeshProUGUI foodCollectedText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Buttons")]
        [SerializeField] private Button respawnButton;
        [SerializeField] private Button spectateButton;

        [Header("Settings")]
        [SerializeField] private float respawnCountdown = 5f;

        private Coroutine countdownCoroutine;

        public event System.Action OnRespawnRequested;
        public event System.Action OnSpectateRequested;

        private void Awake()
        {
            respawnButton?.onClick.AddListener(() => OnRespawnRequested?.Invoke());
            spectateButton?.onClick.AddListener(() => OnSpectateRequested?.Invoke());

            panel?.SetActive(false);
        }

        public void Show(string killerName, int levelReached, int kills, float xpEarned, int foodCollected)
        {
            panel?.SetActive(true);

            if (defeatedByText != null)
                defeatedByText.text = $"You were defeated by {killerName}";
            if (levelReachedText != null)
                levelReachedText.text = $"Level Reached: {levelReached}";
            if (killsText != null)
                killsText.text = $"Kills: {kills}";
            if (xpEarnedText != null)
                xpEarnedText.text = $"XP Earned: {xpEarned:F0}";
            if (foodCollectedText != null)
                foodCollectedText.text = $"Food Collected: {foodCollected}";

            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);

            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        public void Hide()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            panel?.SetActive(false);
        }

        private IEnumerator CountdownRoutine()
        {
            float remaining = respawnCountdown;

            while (remaining > 0f)
            {
                if (countdownText != null)
                    countdownText.text = $"Respawning in {Mathf.CeilToInt(remaining)}...";

                remaining -= Time.deltaTime;
                yield return null;
            }

            if (countdownText != null)
                countdownText.text = "Respawning...";

            OnRespawnRequested?.Invoke();
            Hide();
        }
    }
}
