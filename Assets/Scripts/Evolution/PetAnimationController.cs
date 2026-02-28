using UnityEngine;
using System.Collections;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Animation states for a pet.
    /// </summary>
    public enum PetAnimationState
    {
        Idle,       // Breath animation (cycling through breath frames)
        Large,      // Large sprite shown (evolved/attack state)
        Evolving,   // Transition animation during evolution
        Dead        // No animation
    }

    /// <summary>
    /// Controls pet sprite animation using breath-images frames for idle
    /// and large-images sprite for evolved/attack state.
    ///
    /// Animation system:
    /// - Idle: cycles through breathFrames at breathFPS (e.g. 8 FPS)
    /// - Large: shows largeSprite statically (used during combat/evolution)
    /// - Evolving: flash + scale tween, then switch to new pet's idle
    ///
    /// Filename convention expected:
    ///   breath-images: {petId}_breath_{frameNumber}.png  (e.g. 001_breath_0.png, 001_breath_1.png)
    ///   large-images:  {petId}_large.png                 (e.g. 001_large.png)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PetAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float breathFPS = 8f;
        [SerializeField] private float largeDuration = 1.5f; // How long to show large sprite before returning to idle

        [Header("References")]
        [SerializeField] private PetSpriteMapper spriteMapper;

        private SpriteRenderer spriteRenderer;
        private PetSpriteData currentPetData;
        private PetAnimationState currentState = PetAnimationState.Idle;

        private int currentBreathFrame;
        private float breathTimer;
        private float largeTimer;
        private bool isLargeTimerActive;

        private Coroutine evolutionCoroutine;

        public PetAnimationState CurrentState => currentState;
        public string CurrentPetId => currentPetData?.petId ?? "";

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            switch (currentState)
            {
                case PetAnimationState.Idle:
                    UpdateBreathAnimation();
                    break;

                case PetAnimationState.Large:
                    if (isLargeTimerActive)
                    {
                        largeTimer -= Time.deltaTime;
                        if (largeTimer <= 0f)
                            SetState(PetAnimationState.Idle);
                    }
                    break;
            }
        }

        private void UpdateBreathAnimation()
        {
            if (currentPetData == null || !currentPetData.HasBreathAnimation) return;

            breathTimer += Time.deltaTime;
            float frameInterval = 1f / Mathf.Max(1f, breathFPS);

            if (breathTimer >= frameInterval)
            {
                breathTimer -= frameInterval;
                currentBreathFrame = (currentBreathFrame + 1) % currentPetData.breathFrames.Length;
                ApplyCurrentFrame();
            }
        }

        private void ApplyCurrentFrame()
        {
            if (spriteRenderer == null) return;

            switch (currentState)
            {
                case PetAnimationState.Idle:
                    if (currentPetData?.HasBreathAnimation == true)
                        spriteRenderer.sprite = currentPetData.breathFrames[currentBreathFrame];
                    break;

                case PetAnimationState.Large:
                    if (currentPetData?.HasLargeSprite == true)
                        spriteRenderer.sprite = currentPetData.largeSprite;
                    else if (currentPetData?.HasBreathAnimation == true)
                        spriteRenderer.sprite = currentPetData.breathFrames[0];
                    break;
            }
        }

        /// <summary>
        /// Sets the current pet by ID. Resets animation to idle frame 0.
        /// </summary>
        public void SetPet(string petId)
        {
            if (spriteMapper == null) return;

            PetSpriteData data = spriteMapper.GetPetById(petId);
            if (data == null) return;

            SetPetData(data);
        }

        /// <summary>
        /// Sets the current pet by tier (random selection).
        /// </summary>
        public void SetPetForTier(EvolutionTier tier)
        {
            if (spriteMapper == null) return;

            PetSpriteData data = spriteMapper.GetRandomPetForTier(tier);
            if (data == null) return;

            SetPetData(data);
        }

        /// <summary>
        /// Sets the current pet by index (for level-based assignment).
        /// </summary>
        public void SetPetByIndex(int index)
        {
            if (spriteMapper == null) return;

            PetSpriteData data = spriteMapper.GetPetByIndex(index);
            if (data == null) return;

            SetPetData(data);
        }

        private void SetPetData(PetSpriteData data)
        {
            currentPetData = data;
            currentBreathFrame = 0;
            breathTimer = 0f;
            SetState(PetAnimationState.Idle);
            ApplyCurrentFrame();
        }

        /// <summary>
        /// Shows the large sprite temporarily (e.g., during combat hit or attack).
        /// </summary>
        public void ShowLargeSprite(float duration = -1f)
        {
            if (currentPetData == null || !currentPetData.HasLargeSprite) return;

            SetState(PetAnimationState.Large);
            ApplyCurrentFrame();

            if (duration > 0f)
            {
                isLargeTimerActive = true;
                largeTimer = duration;
            }
            else
            {
                isLargeTimerActive = true;
                largeTimer = largeDuration;
            }
        }

        /// <summary>
        /// Plays the evolution animation: flash + scale tween, then switch to new pet.
        /// </summary>
        public void PlayEvolutionAnimation(EvolutionTier newTier, System.Action onComplete = null)
        {
            if (evolutionCoroutine != null)
                StopCoroutine(evolutionCoroutine);

            evolutionCoroutine = StartCoroutine(EvolutionAnimationRoutine(newTier, onComplete));
        }

        private IEnumerator EvolutionAnimationRoutine(EvolutionTier newTier, System.Action onComplete)
        {
            SetState(PetAnimationState.Evolving);

            // Flash white
            Color originalColor = spriteRenderer.color;
            float flashDuration = 0.1f;
            int flashCount = 5;

            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashDuration);
            }

            // Scale up briefly
            Vector3 originalScale = transform.localScale;
            float tweenTime = 0.3f;
            float elapsed = 0f;

            while (elapsed < tweenTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / tweenTime;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.4f, t);
                yield return null;
            }

            // Switch to new pet for the tier
            SetPetForTier(newTier);

            // Scale back down
            elapsed = 0f;
            while (elapsed < tweenTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / tweenTime;
                transform.localScale = Vector3.Lerp(originalScale * 1.4f, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
            SetState(PetAnimationState.Idle);

            onComplete?.Invoke();
            evolutionCoroutine = null;
        }

        private void SetState(PetAnimationState newState)
        {
            currentState = newState;

            if (newState == PetAnimationState.Idle)
            {
                isLargeTimerActive = false;
                currentBreathFrame = 0;
                breathTimer = 0f;
            }
        }

        /// <summary>
        /// Returns to idle animation immediately.
        /// </summary>
        public void ReturnToIdle()
        {
            SetState(PetAnimationState.Idle);
            ApplyCurrentFrame();
        }

        /// <summary>
        /// Sets the sprite mapper reference (can be set at runtime).
        /// </summary>
        public void SetSpriteMapper(PetSpriteMapper mapper)
        {
            spriteMapper = mapper;
        }

        /// <summary>
        /// Gets the first breath frame sprite (for UI previews).
        /// </summary>
        public Sprite GetPreviewSprite()
        {
            if (currentPetData == null) return null;
            if (currentPetData.HasBreathAnimation) return currentPetData.breathFrames[0];
            if (currentPetData.HasLargeSprite) return currentPetData.largeSprite;
            return null;
        }

        /// <summary>
        /// Gets the large sprite (for UI previews of evolved state).
        /// </summary>
        public Sprite GetLargeSprite()
        {
            return currentPetData?.largeSprite;
        }
    }
}
