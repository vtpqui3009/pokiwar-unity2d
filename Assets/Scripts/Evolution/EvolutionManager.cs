using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Manages player evolution - multi-stage chains, sprite/animation updates, and stat scaling.
    /// Evolution thresholds: Baby(1), Basic(5), Stage1(15), Stage2(30), Mega(100+)
    ///
    /// Animation integration:
    /// - Uses PetAnimationController for breath (idle) and large (attack/evolve) states
    /// - Uses BattleAnimationController for lunge/recoil/death/respawn movement
    /// - Falls back to SpriteDatabase static sprites if no PetSpriteMapper is assigned
    ///
    /// Fixes:
    /// - AddXP() handles negative XP (poison food) with minimum level guard
    /// - SetLevel() implemented for proper level reduction on death
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
        private BattleAnimationController battleAnimController;

        // Events
        public event System.Action<int> OnLevelUp;
        public event System.Action<EvolutionTier, EvolutionTier> OnEvolutionStageChanged;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            effectController = GetComponent<EvolutionEffectController>();
            animationController = GetComponent<PetAnimationController>();
            battleAnimController = GetComponent<BattleAnimationController>();

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
                animationController.SetSpriteMapper(petSpriteMapper);
                animationController.SetPetForTier(EvolutionTier.Baby);
            }
            else if (spriteDatabase != null && playerController != null)
            {
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

            // Handle negative XP (poison food) - minimum level is 1
            if (amount < 0f)
            {
                playerData.currentXP = Mathf.Max(0f, playerData.currentXP + amount);
                // Don't go below level 1
                return;
            }

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
                UpdatePlayerSprite();
            }
        }

        /// <summary>
        /// Sets the player's level directly (used for death penalty - lose half level).
        /// Recalculates XP, tier, speed, and scale.
        /// </summary>
        public void SetLevel(int newLevel)
        {
            newLevel = Mathf.Max(1, newLevel);

            EvolutionTier previousTier = currentTier;

            playerData.level = newLevel;
            playerData.spriteId = newLevel;
            playerData.currentXP = 0f;
            playerData.maxXP = 10f + (newLevel * 5f);

            if (newLevel > playerData.bestLevel)
                playerData.bestLevel = newLevel;

            EvolutionTier newTier = SpriteDatabase.GetTierForLevel(newLevel);
            if (newTier != previousTier)
            {
                currentTier = newTier;
                OnEvolutionStageChanged?.Invoke(previousTier, newTier);
            }

            UpdateSpeedForLevel(newLevel);
            UpdateScaleForTier(newTier);
            UpdatePlayerSprite();
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
                animationController.PlayEvolutionAnimation(to, () =>
                {
                    UpdatePlayerSprite();
                    // Update battle anim base scale after evolution
                    battleAnimController?.UpdateBaseScale(transform.localScale);
                });
            }
            else
            {
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
            battleAnimController?.UpdateBaseScale(transform.localScale);
        }

        private void UpdatePlayerSprite()
        {
            if (animationController != null && petSpriteMapper != null) return;

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

            if (petSpriteMapper != null)
            {
                PetSpriteData nextPet = petSpriteMapper.GetRandomPetForTier(nextTier);
                if (nextPet?.HasBreathAnimation == true) return nextPet.breathFrames[0];
                if (nextPet?.HasLargeSprite == true) return nextPet.largeSprite;
            }

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
