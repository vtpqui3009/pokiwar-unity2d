using UnityEngine;
using TMPro;
using System.Collections;
using Pokiwar.Evolution;

namespace Pokiwar.UI
{
    /// <summary>
    /// Shows level-up popups and evolution stage change notifications with animations.
    /// </summary>
    public class EvolutionNotificationUI : MonoBehaviour
    {
        [Header("Level Up")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private float levelUpDisplayDuration = 2f;

        [Header("Evolution Stage")]
        [SerializeField] private GameObject evolutionPanel;
        [SerializeField] private TextMeshProUGUI evolutionText;
        [SerializeField] private UnityEngine.UI.Image evolutionSpritePreview;
        [SerializeField] private float evolutionDisplayDuration = 3f;

        [Header("Ability Unlock")]
        [SerializeField] private GameObject abilityUnlockPanel;
        [SerializeField] private TextMeshProUGUI abilityUnlockText;

        [Header("Animation")]
        [SerializeField] private float scaleAnimDuration = 0.3f;

        private Coroutine levelUpCoroutine;
        private Coroutine evolutionCoroutine;

        private void Awake()
        {
            levelUpPanel?.SetActive(false);
            evolutionPanel?.SetActive(false);
            abilityUnlockPanel?.SetActive(false);
        }

        public void ShowLevelUp(int newLevel)
        {
            if (levelUpCoroutine != null)
                StopCoroutine(levelUpCoroutine);

            levelUpCoroutine = StartCoroutine(ShowLevelUpRoutine(newLevel));
        }

        public void ShowEvolution(EvolutionTier newTier)
        {
            if (evolutionCoroutine != null)
                StopCoroutine(evolutionCoroutine);

            evolutionCoroutine = StartCoroutine(ShowEvolutionRoutine(newTier));

            // Show ability unlock at Stage1
            if (newTier == EvolutionTier.Stage1)
                ShowAbilityUnlock("Abilities Unlocked!");
        }

        public void ShowAbilityUnlock(string message)
        {
            StartCoroutine(ShowAbilityUnlockRoutine(message));
        }

        private IEnumerator ShowLevelUpRoutine(int level)
        {
            if (levelUpPanel == null) yield break;

            levelUpPanel.SetActive(true);

            if (levelUpText != null)
                levelUpText.text = $"Level Up!\nLevel {level}";

            yield return StartCoroutine(ScaleInRoutine(levelUpPanel.transform));
            yield return new WaitForSeconds(levelUpDisplayDuration);
            yield return StartCoroutine(FadeOutRoutine(levelUpPanel));

            levelUpPanel.SetActive(false);
        }

        private IEnumerator ShowEvolutionRoutine(EvolutionTier tier)
        {
            if (evolutionPanel == null) yield break;

            evolutionPanel.SetActive(true);

            if (evolutionText != null)
                evolutionText.text = $"EVOLVED!\n{tier}";

            yield return StartCoroutine(ScaleInRoutine(evolutionPanel.transform));
            yield return new WaitForSeconds(evolutionDisplayDuration);
            yield return StartCoroutine(FadeOutRoutine(evolutionPanel));

            evolutionPanel.SetActive(false);
        }

        private IEnumerator ShowAbilityUnlockRoutine(string message)
        {
            if (abilityUnlockPanel == null) yield break;

            abilityUnlockPanel.SetActive(true);

            if (abilityUnlockText != null)
                abilityUnlockText.text = message;

            yield return new WaitForSeconds(3f);
            abilityUnlockPanel.SetActive(false);
        }

        private IEnumerator ScaleInRoutine(Transform target)
        {
            float elapsed = 0f;
            target.localScale = Vector3.zero;

            while (elapsed < scaleAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleAnimDuration;
                target.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        private IEnumerator FadeOutRoutine(GameObject panel)
        {
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            float fadeDuration = 0.5f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            cg.alpha = 1f;
        }
    }
}
