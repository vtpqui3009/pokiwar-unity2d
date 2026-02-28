using UnityEngine;
using System.Collections;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Controls battle-specific sprite movement animations:
    /// - Lunge: quick movement toward the enemy when attacking
    /// - Recoil: knockback bounce when taking damage
    /// - Death: fade + shrink when dying
    /// - Respawn: scale-in when respawning
    ///
    /// Works alongside PetAnimationController (handles sprite frames)
    /// and PlayerController (handles actual physics movement).
    /// This controller handles VISUAL-ONLY positional offsets using transform.localPosition.
    /// </summary>
    public class BattleAnimationController : MonoBehaviour
    {
        [Header("Lunge Settings")]
        [SerializeField] private float lungeDistance = 0.4f;
        [SerializeField] private float lungeDuration = 0.12f;
        [SerializeField] private float lungeReturnDuration = 0.18f;

        [Header("Recoil Settings")]
        [SerializeField] private float recoilDistance = 0.3f;
        [SerializeField] private float recoilDuration = 0.08f;
        [SerializeField] private float recoilReturnDuration = 0.15f;

        [Header("Death Settings")]
        [SerializeField] private float deathFadeDuration = 0.5f;
        [SerializeField] private float deathShrinkDuration = 0.4f;

        [Header("Respawn Settings")]
        [SerializeField] private float respawnScaleInDuration = 0.3f;

        private SpriteRenderer spriteRenderer;
        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private Color baseColor;

        private Coroutine lungeCoroutine;
        private Coroutine recoilCoroutine;
        private Coroutine deathCoroutine;
        private bool isAnimating;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;
            if (spriteRenderer != null)
                baseColor = spriteRenderer.color;
        }

        /// <summary>
        /// Plays a lunge animation toward the target direction.
        /// Called when this pet attacks another.
        /// </summary>
        public void PlayLunge(Vector2 directionToTarget)
        {
            if (lungeCoroutine != null)
                StopCoroutine(lungeCoroutine);

            lungeCoroutine = StartCoroutine(LungeRoutine(directionToTarget.normalized));
        }

        /// <summary>
        /// Plays a recoil animation away from the attacker.
        /// Called when this pet takes damage.
        /// </summary>
        public void PlayRecoil(Vector2 directionFromAttacker)
        {
            if (recoilCoroutine != null)
                StopCoroutine(recoilCoroutine);

            recoilCoroutine = StartCoroutine(RecoilRoutine(directionFromAttacker.normalized));
        }

        /// <summary>
        /// Plays death animation (fade + shrink).
        /// </summary>
        public void PlayDeath(System.Action onComplete = null)
        {
            if (deathCoroutine != null)
                StopCoroutine(deathCoroutine);

            deathCoroutine = StartCoroutine(DeathRoutine(onComplete));
        }

        /// <summary>
        /// Plays respawn animation (scale in from zero).
        /// </summary>
        public void PlayRespawn()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator LungeRoutine(Vector2 direction)
        {
            isAnimating = true;
            Vector3 startPos = baseLocalPosition;
            Vector3 lungePos = baseLocalPosition + (Vector3)(direction * lungeDistance);

            // Lunge forward
            float elapsed = 0f;
            while (elapsed < lungeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / lungeDuration);
                transform.localPosition = Vector3.Lerp(startPos, lungePos, t);
                yield return null;
            }

            // Return to base
            elapsed = 0f;
            while (elapsed < lungeReturnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / lungeReturnDuration);
                transform.localPosition = Vector3.Lerp(lungePos, startPos, t);
                yield return null;
            }

            transform.localPosition = startPos;
            isAnimating = false;
            lungeCoroutine = null;
        }

        private IEnumerator RecoilRoutine(Vector2 awayDirection)
        {
            isAnimating = true;
            Vector3 startPos = baseLocalPosition;
            Vector3 recoilPos = baseLocalPosition + (Vector3)(awayDirection * recoilDistance);

            // Flash red briefly
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;

            // Recoil back
            float elapsed = 0f;
            while (elapsed < recoilDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / recoilDuration;
                transform.localPosition = Vector3.Lerp(startPos, recoilPos, t);
                yield return null;
            }

            // Restore color
            if (spriteRenderer != null)
                spriteRenderer.color = baseColor;

            // Return to base
            elapsed = 0f;
            while (elapsed < recoilReturnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / recoilReturnDuration);
                transform.localPosition = Vector3.Lerp(recoilPos, startPos, t);
                yield return null;
            }

            transform.localPosition = startPos;
            isAnimating = false;
            recoilCoroutine = null;
        }

        private IEnumerator DeathRoutine(System.Action onComplete)
        {
            isAnimating = true;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float duration = Mathf.Max(deathFadeDuration, deathShrinkDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fadeT = Mathf.Clamp01(elapsed / deathFadeDuration);
                float shrinkT = Mathf.Clamp01(elapsed / deathShrinkDuration);

                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - fadeT);

                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, shrinkT);
                yield return null;
            }

            transform.localScale = Vector3.zero;
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

            isAnimating = false;
            deathCoroutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator RespawnRoutine()
        {
            // Reset state
            transform.localPosition = baseLocalPosition;
            if (spriteRenderer != null)
                spriteRenderer.color = baseColor;

            // Scale in from zero
            transform.localScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < respawnScaleInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / respawnScaleInDuration);
                transform.localScale = Vector3.Lerp(Vector3.zero, baseLocalScale, t);
                yield return null;
            }

            transform.localScale = baseLocalScale;
        }

        /// <summary>
        /// Updates the base scale (called when evolution tier changes the player's scale).
        /// </summary>
        public void UpdateBaseScale(Vector3 newScale)
        {
            baseLocalScale = newScale;
        }

        /// <summary>
        /// Resets all visual state to defaults immediately.
        /// </summary>
        public void ResetVisuals()
        {
            StopAllCoroutines();
            transform.localPosition = baseLocalPosition;
            transform.localScale = baseLocalScale;
            if (spriteRenderer != null)
                spriteRenderer.color = baseColor;
            isAnimating = false;
        }

        public bool IsAnimating => isAnimating;
    }
}
