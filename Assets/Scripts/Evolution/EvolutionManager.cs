using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Manages player evolution - updates sprites and stats on level up.
    /// </summary>
    public class EvolutionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteDatabase spriteDatabase;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerData playerData;

        [Header("Evolution Effects")]
        [SerializeField] private GameObject evolutionEffectPrefab;
        [SerializeField] private float effectDuration = 1f;

        private ParticleSystem evolutionParticle;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            playerData = new PlayerData("Player");
            InitializeStartingSprite();
        }

        private void Start()
        {
            UpdatePlayerSprite();
        }

        private void InitializeStartingSprite()
        {
            if (spriteDatabase != null && playerController != null)
            {
                Sprite startingSprite = spriteDatabase.GetSpriteForLevel(1);
                playerController.SetSprite(startingSprite);
            }
        }

        public void AddXP(float amount)
        {
            int previousLevel = playerData.level;
            playerData.AddXP(amount);

            if (playerData.level > previousLevel)
            {
                OnLevelUp(playerData.level);
            }

            UpdatePlayerSprite();
        }

        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"Level Up! Now level {newLevel}");
            PlayEvolutionEffect();

            if (playerController != null)
            {
                float newSpeed = 5f + (newLevel * 0.1f);
                playerController.SetMoveSpeed(newSpeed);
            }
        }

        private void UpdatePlayerSprite()
        {
            if (spriteDatabase != null && playerController != null)
            {
                Sprite sprite = spriteDatabase.GetSpriteForLevel(playerData.spriteId);
                playerController.SetSprite(sprite);
            }
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

        public void SetSpriteDatabase(SpriteDatabase database)
        {
            spriteDatabase = database;
            UpdatePlayerSprite();
        }
    }
}
