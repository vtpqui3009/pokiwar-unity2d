using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pokiwar.Evolution;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays player level, XP bar, and stats.
    /// </summary>
    public class LevelBarUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Gradient xpGradient;

        [Header("References")]
        [SerializeField] private EvolutionManager evolutionManager;

        private void Start()
        {
            if (evolutionManager == null)
                evolutionManager = FindObjectOfType<EvolutionManager>();

            if (xpSlider != null)
                xpSlider.minValue = 0f;
        }

        private void Update()
        {
            if (evolutionManager == null)
                return;

            UpdateLevelText();
            UpdateXPBar();
        }

        private void UpdateLevelText()
        {
            if (levelText != null)
            {
                levelText.text = $"Level {evolutionManager.GetLevel()}";
            }
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
            {
                xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(maxXP)} XP";
            }

            if (fillImage != null)
            {
                fillImage.color = xpGradient.Evaluate(progress);
            }
        }

        public void SetEvolutionManager(EvolutionManager manager)
        {
            evolutionManager = manager;
        }
    }
}
