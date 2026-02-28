using UnityEngine;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Rarity tiers for sprites.
    /// </summary>
    public enum SpriteRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Evolution tier (stage) for sprites.
    /// </summary>
    public enum EvolutionTier
    {
        Baby = 0,
        Basic = 1,
        Stage1 = 2,
        Stage2 = 3,
        Mega = 4
    }

    /// <summary>
    /// ScriptableObject database mapping levels to pet sprites with rarity and tier support.
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Pokiwar/Sprite Database")]
    public class SpriteDatabase : ScriptableObject
    {
        [Header("Sprites by Tier")]
        [SerializeField] private Sprite[] babySprites;
        [SerializeField] private Sprite[] basicSprites;
        [SerializeField] private Sprite[] stage1Sprites;
        [SerializeField] private Sprite[] stage2Sprites;
        [SerializeField] private Sprite[] megaSprites;

        [Header("Legacy / Flat List")]
        [SerializeField] private Sprite[] petSprites;

        public int MaxSprites => petSprites?.Length ?? 0;

        /// <summary>
        /// Returns a sprite for the given level using the flat list (legacy support).
        /// </summary>
        public Sprite GetSpriteForLevel(int level)
        {
            if (petSprites == null || petSprites.Length == 0)
                return null;

            int index = Mathf.Clamp(level, 1, petSprites.Length) - 1;
            return petSprites[index];
        }

        /// <summary>
        /// Returns a random sprite for the given evolution tier.
        /// </summary>
        public Sprite GetRandomSpriteForTier(EvolutionTier tier)
        {
            Sprite[] tierSprites = GetTierArray(tier);
            if (tierSprites == null || tierSprites.Length == 0)
                return GetSpriteForLevel(1);

            return tierSprites[Random.Range(0, tierSprites.Length)];
        }

        /// <summary>
        /// Returns the evolution tier for a given level.
        /// Thresholds: Baby(1-4), Basic(5-14), Stage1(15-29), Stage2(30-49), Mega(50+)
        /// </summary>
        public static EvolutionTier GetTierForLevel(int level)
        {
            if (level >= 50) return EvolutionTier.Mega;
            if (level >= 30) return EvolutionTier.Stage2;
            if (level >= 15) return EvolutionTier.Stage1;
            if (level >= 5)  return EvolutionTier.Basic;
            return EvolutionTier.Baby;
        }

        /// <summary>
        /// Returns the next evolution threshold level for a given current level.
        /// </summary>
        public static int GetNextEvolutionLevel(int currentLevel)
        {
            if (currentLevel < 5)  return 5;
            if (currentLevel < 15) return 15;
            if (currentLevel < 30) return 30;
            if (currentLevel < 50) return 50;
            return -1; // Max tier
        }

        public bool HasSprite(int level)
        {
            if (petSprites == null || petSprites.Length == 0)
                return false;
            return level >= 1 && level <= petSprites.Length;
        }

        private Sprite[] GetTierArray(EvolutionTier tier)
        {
            return tier switch
            {
                EvolutionTier.Baby   => babySprites,
                EvolutionTier.Basic  => basicSprites,
                EvolutionTier.Stage1 => stage1Sprites,
                EvolutionTier.Stage2 => stage2Sprites,
                EvolutionTier.Mega   => megaSprites,
                _                    => petSprites
            };
        }
    }
}
