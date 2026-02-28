using UnityEngine;
using System.Collections;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Handles visual effects for level-up and evolution stage changes.
    /// Flash/glow on level up, particle burst and screen shake on stage change.
    /// </summary>
    public class EvolutionEffectController : MonoBehaviour
    {
        [Header("Level Up Effects")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private int flashCount = 3;
        [SerializeField] private Color flashColor = Color.white;

        [Header("Evolution Stage Effects")]
        [SerializeField] private ParticleSystem evolutionParticles;
        [SerializeField] private float screenShakeDuration = 0.3f;
        [SerializeField] private float screenShakeMagnitude = 0.2f;

        [Header("Size Tween")]
        [SerializeField] private float tweenDuration = 0.4f;
        [SerializeField] private float tweenScaleMultiplier = 1.3f;

        private Camera mainCamera;
        private Vector3 originalCameraPos;
        private Color originalColor;
        private Coroutine activeFlashCoroutine;
        private Coroutine activeTweenCoroutine;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            mainCamera = Camera.main;
        }

        /// <summary>
        /// Plays a flash effect for a regular level-up.
        /// </summary>
        public void PlayLevelUpEffect()
        {
            if (activeFlashCoroutine != null)
                StopCoroutine(activeFlashCoroutine);

            activeFlashCoroutine = StartCoroutine(FlashRoutine());
        }

        /// <summary>
        /// Plays full evolution stage change effects.
        /// </summary>
        public void PlayEvolutionStageEffect(EvolutionTier newTier)
        {
            PlayLevelUpEffect();

            if (evolutionParticles != null)
                evolutionParticles.Play();

            if (activeTweenCoroutine != null)
                StopCoroutine(activeTweenCoroutine);

            activeTweenCoroutine = StartCoroutine(ScaleTweenRoutine());

            // Screen shake only for Mega evolution
            if (newTier == EvolutionTier.Mega && mainCamera != null)
                StartCoroutine(ScreenShakeRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer == null) yield break;

            originalColor = spriteRenderer.color;

            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashDuration);
            }

            spriteRenderer.color = originalColor;
        }

        private IEnumerator ScaleTweenRoutine()
        {
            Vector3 startScale = transform.localScale;
            Vector3 peakScale = startScale * tweenScaleMultiplier;
            float halfDuration = tweenDuration / 2f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(startScale, peakScale, t);
                yield return null;
            }

            elapsed = 0f;

            // Scale back down
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(peakScale, startScale, t);
                yield return null;
            }

            transform.localScale = startScale;
        }

        private IEnumerator ScreenShakeRoutine()
        {
            if (mainCamera == null) yield break;

            originalCameraPos = mainCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < screenShakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * screenShakeMagnitude;
                float y = Random.Range(-1f, 1f) * screenShakeMagnitude;
                mainCamera.transform.localPosition = originalCameraPos + new Vector3(x, y, 0f);
                yield return null;
            }

            mainCamera.transform.localPosition = originalCameraPos;
        }
    }
}
