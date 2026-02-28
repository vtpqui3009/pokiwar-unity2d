using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Manages player evolution - multi-stage chains, sprite/animation updates, and stat scaling.
    /// Evolution thresholds: Baby(1), Basic(5), Stage1(15), Stage2(30), Mega(50+)
    ///
    /// Animation integration:
    /// - Uses PetAnimationController for breath (idle) and large (attack/evolve) states
    /// - Falls back to SpriteDatabase static sprites if no PetSpriteMapper is assigned
    /// </summary>
    public class EvolutionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteDatabase spriteDatabase;
        [SerializeField] private PetSpriteMapper petSpriteMapper;
        [SerializeField] private PlayerController playerController;

        [Header("Evolution Effects")]
        [SerializeField] private GameObject evolutionEffectPrefab;
        [SerializeField] private float effectDuration = 1f;

        [Header("Scale Settings")]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float scalePerTier = 0.15f;

        private PlayerData playerData;
        private EvolutionTier currentTier;
        private EvolutionEffectController effectController;
        private PetAnimationController animationController;

        // Events
        public event System.Action<int> OnLevelUp;
        public event System.Action<EvolutionTier, EvolutionTier> OnEvolutionStageChanged;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            effectController = GetComponent<EvolutionEffectController>();
            animationController = GetComponent<PetAnimationController>();

            playerData = new PlayerData("Player");
            currentTier = EvolutionTier.Baby;
        }

        private void Start()
        {
            InitializeStartingSprite();
        }

        private void InitializeStartingSprite()
        {
            if (animationController != null && petSpriteMapper != null)
            {
                // Use animation system
                animationController.SetSpriteMapper(petSpriteMapper);
                animationController.SetPetForTier(EvolutionTier.Baby);
            }
            else if (spriteDatabase != null && playerController != null)
            {
                // Fallback to static sprite
                Sprite startingSprite = spriteDatabase.GetRandomSpriteForTier(EvolutionTier.Baby);
                if (startingSprite == null)
                    startingSprite = spriteDatabase.GetSpriteForLevel(1);
                playerController.SetSprite(startingSprite);
            }
        }

        public void AddXP(float amount)
        {
            if (amount == 0f) return;

            int previousLevel = playerData.level;
            EvolutionTier previousTier = currentTier;

            playerData.AddXP(amount);

            if (playerData.level > previousLevel)
            {
                for (int lvl = previousLevel + 1; lvl <= playerData.level; lvl++)
                {
                    HandleLevelUp(lvl);
                }
            }

            EvolutionTier newTier = SpriteDatabase.GetTierForLevel(playerData.level);
            if (newTier != previousTier)
            {
                HandleEvolutionStageChange(previousTier, newTier);
            }
            else
            {
                // Update sprite for new level (only if no stage change - stage change handles it)
                UpdatePlayerSprite();
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
            PlayEvolutionEffect();
            UpdateSpeedForLevel(newLevel);
            UpdateScaleForTier(SpriteDatabase.GetTierForLevel(newLevel));

            // Show large sprite briefly on level up
            animationController?.ShowLargeSprite(1.0f);
        }

        private void HandleEvolutionStageChange(EvolutionTier from, EvolutionTier to)
        {
            currentTier = to;
            OnEvolutionStageChanged?.Invoke(from, to);

            if (animationController != null)
            {
                // Play full evolution animation, then switch to new tier's pet
                animationController.PlayEvolutionAnimation(to, () =>
                {
                    // After animation completes, update static sprite fallback too
                    UpdatePlayerSprite();
                });
            }
            else
            {
                // Fallback: just update sprite
                UpdatePlayerSprite();
            }

            if (effectController != null)
                effectController.PlayEvolutionStageEffect(to);
        }

        private void UpdateSpeedForLevel(int level)
        {
            if (playerController == null) return;

            float newSpeed = 5f + (level * 0.08f);
            newSpeed = Mathf.Min(newSpeed, 12f);
            playerController.SetBaseSpeed(newSpeed);
        }

        private void UpdateScaleForTier(EvolutionTier tier)
        {
            float scale = baseScale + ((int)tier * scalePerTier);
            transform.localScale = Vector3.one * scale;
        }

        private void UpdatePlayerSprite()
        {
            // If animation controller is active, it manages the sprite
            if (animationController != null && petSpriteMapper != null) return;

            // Fallback to static sprite database
            if (spriteDatabase == null || playerController == null) return;

            Sprite sprite = spriteDatabase.GetSpriteForLevel(playerData.spriteId);
            if (sprite == null)
                sprite = spriteDatabase.GetRandomSpriteForTier(currentTier);

            if (sprite != null)
                playerController.SetSprite(sprite);
        }

        private void PlayEvolutionEffect()
        {
            if (evolutionEffectPrefab != null)
            {
                GameObject effect = Instantiate(evolutionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }

        /// <summary>
        /// Triggers the large sprite display (e.g., when taking damage or attacking).
        /// </summary>
        public void TriggerLargeSprite(float duration = 1.5f)
        {
            animationController?.ShowLargeSprite(duration);
        }

        public int GetLevel() => playerData.level;
        public float GetXP() => playerData.currentXP;
        public float GetMaxXP() => playerData.maxXP;
        public float GetXPProgress() => playerData.GetXPProgress();
        public int GetSpriteId() => playerData.spriteId;
        public EvolutionTier GetCurrentTier() => currentTier;
        public PlayerData GetPlayerData() => playerData;
        public int GetKillCount() => playerData.killCount;

        public void SetPlayerName(string name)
        {
            if (playerData != null)
                playerData.playerName = name;
        }

        public string GetPlayerName() => playerData?.playerName ?? "Player";

        public void RecordKill()
        {
            playerData?.RecordKill();
        }

        public void RecordDeath()
        {
            playerData?.RecordDeath();
        }

        public void SetSpriteDatabase(SpriteDatabase database)
        {
            spriteDatabase = database;
            UpdatePlayerSprite();
        }

        public void SetPetSpriteMapper(PetSpriteMapper mapper)
        {
            petSpriteMapper = mapper;
            if (animationController != null)
            {
                animationController.SetSpriteMapper(mapper);
                animationController.SetPetForTier(currentTier);
            }
        }

        public int GetNextEvolutionLevel()
        {
            return SpriteDatabase.GetNextEvolutionLevel(playerData.level);
        }

        public Sprite GetNextEvolutionPreviewSprite()
        {
            EvolutionTier nextTier = (EvolutionTier)Mathf.Min((int)currentTier + 1, (int)EvolutionTier.Mega);

            // Try animation controller first
            if (petSpriteMapper != null)
            {
                PetSpriteData nextPet = petSpriteMapper.GetRandomPetForTier(nextTier);
                if (nextPet?.HasBreathAnimation == true) return nextPet.breathFrames[0];
                if (nextPet?.HasLargeSprite == true) return nextPet.largeSprite;
            }

            // Fallback to sprite database
            return spriteDatabase?.GetRandomSpriteForTier(nextTier);
        }

        public Sprite GetCurrentPreviewSprite()
        {
            if (animationController != null)
                return animationController.GetPreviewSprite();

            return spriteDatabase?.GetSpriteForLevel(playerData.spriteId);
        }
    }
}
