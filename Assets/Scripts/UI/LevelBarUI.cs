using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pokiwar.Evolution;
using Pokiwar.Combat;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays player level, XP bar, health bar, evolution stage, and shield indicator.
    /// </summary>
    public class LevelBarUI : MonoBehaviour
    {
        [Header("XP Bar")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Image xpFillImage;
        [SerializeField] private Gradient xpGradient;

        [Header("Health Bar")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Evolution")]
        [SerializeField] private TextMeshProUGUI evolutionStageText;
        [SerializeField] private Image nextEvolutionPreview;
        [SerializeField] private TextMeshProUGUI nextEvolutionLevelText;

        [Header("Shield")]
        [SerializeField] private GameObject shieldIndicator;

        [Header("References")]
        [SerializeField] private EvolutionManager evolutionManager;
        [SerializeField] private HealthController healthController;

        private void Start()
        {
            if (evolutionManager == null)
                evolutionManager = FindObjectOfType<EvolutionManager>();
            if (healthController == null)
                healthController = FindObjectOfType<HealthController>();

            if (xpSlider != null) xpSlider.minValue = 0f;
            if (healthSlider != null) healthSlider.minValue = 0f;
        }

        private void Update()
        {
            if (evolutionManager != null)
            {
                UpdateLevelText();
                UpdateXPBar();
                UpdateEvolutionInfo();
            }

            if (healthController != null)
            {
                UpdateHealthBar();
                UpdateShieldIndicator();
            }
        }

        private void UpdateLevelText()
        {
            if (levelText != null)
                levelText.text = $"Level {evolutionManager.GetLevel()}";
        }

        private void UpdateXPBar()
        {
            float currentXP = evolutionManager.GetXP();
            float maxXP = evolutionManager.GetMaxXP();
            float progress = evolutionManager.GetXPProgress();

            if (xpSlider != null)
            {
                xpSlider.maxValue = maxXP;
                xpSlider.value = currentXP;
            }

            if (xpText != null)
                xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(maxXP)} XP";

            if (xpFillImage != null)
                xpFillImage.color = xpGradient.Evaluate(progress);
        }

        private void UpdateHealthBar()
        {
            float hp = healthController.CurrentHealth;
            float maxHp = healthController.MaxHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHp;
                healthSlider.value = hp;
            }

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(maxHp)}";

            if (healthFillImage != null)
            {
                float percent = maxHp > 0f ? hp / maxHp : 0f;
                healthFillImage.color = Color.Lerp(Color.red, Color.green, percent);
            }
        }

        private void UpdateEvolutionInfo()
        {
            EvolutionTier tier = evolutionManager.GetCurrentTier();

            if (evolutionStageText != null)
                evolutionStageText.text = tier.ToString();

            int nextLevel = evolutionManager.GetNextEvolutionLevel();
            if (nextEvolutionLevelText != null)
            {
                nextEvolutionLevelText.text = nextLevel > 0
                    ? $"Next: Lv.{nextLevel}"
                    : "MAX";
            }

            if (nextEvolutionPreview != null)
            {
                Sprite preview = evolutionManager.GetNextEvolutionPreviewSprite();
                if (preview != null)
                    nextEvolutionPreview.sprite = preview;
            }
        }

        private void UpdateShieldIndicator()
        {
            if (shieldIndicator != null)
                shieldIndicator.SetActive(healthController.IsShielded);
        }

        public void SetEvolutionManager(EvolutionManager manager)
        {
            evolutionManager = manager;
        }

        public void SetHealthController(HealthController controller)
        {
            healthController = controller;
        }
    }
}
