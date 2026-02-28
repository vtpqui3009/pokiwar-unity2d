using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Manages player evolution - multi-stage chains, sprite updates, and stat scaling.
    /// Evolution thresholds: Baby(1), Basic(5), Stage1(15), Stage2(30), Mega(50+)
    /// </summary>
    public class EvolutionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteDatabase spriteDatabase;
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

        // Events
        public event System.Action<int> OnLevelUp;
        public event System.Action<EvolutionTier, EvolutionTier> OnEvolutionStageChanged;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            effectController = GetComponent<EvolutionEffectController>();
            playerData = new PlayerData("Player");
            currentTier = EvolutionTier.Baby;
        }

        private void Start()
        {
            InitializeStartingSprite();
        }

        private void InitializeStartingSprite()
        {
            if (spriteDatabase == null || playerController == null) return;

            Sprite startingSprite = spriteDatabase.GetRandomSpriteForTier(EvolutionTier.Baby);
            if (startingSprite == null)
                startingSprite = spriteDatabase.GetSpriteForLevel(1);

            playerController.SetSprite(startingSprite);
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

            UpdatePlayerSprite();
        }

        private void HandleLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
            PlayEvolutionEffect();
            UpdateSpeedForLevel(newLevel);
            UpdateScaleForTier(SpriteDatabase.GetTierForLevel(newLevel));
        }

        private void HandleEvolutionStageChange(EvolutionTier from, EvolutionTier to)
        {
            currentTier = to;
            OnEvolutionStageChanged?.Invoke(from, to);

            if (effectController != null)
                effectController.PlayEvolutionStageEffect(to);
        }

        private void UpdateSpeedForLevel(int level)
        {
            if (playerController == null) return;

            // Speed increases slightly per level, capped at a reasonable max
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

        public int GetNextEvolutionLevel()
        {
            return SpriteDatabase.GetNextEvolutionLevel(playerData.level);
        }

        public Sprite GetNextEvolutionPreviewSprite()
        {
            if (spriteDatabase == null) return null;

            EvolutionTier nextTier = (EvolutionTier)Mathf.Min((int)currentTier + 1, (int)EvolutionTier.Mega);
            return spriteDatabase.GetRandomSpriteForTier(nextTier);
        }
    }
}
