using UnityEngine;
using System.Collections.Generic;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Data for a single pet's sprite set: breath animation frames and large sprite.
    /// </summary>
    [System.Serializable]
    public class PetSpriteData
    {
        [Tooltip("Unique pet ID (matches filename prefix, e.g. '001' for pet_001_*.png)")]
        public string petId;

        [Tooltip("Breath animation frames (idle animation sprites from breath-images folder)")]
        public Sprite[] breathFrames;

        [Tooltip("Large sprite for evolved/attack state (from large-images folder)")]
        public Sprite largeSprite;

        [Tooltip("Evolution tier this pet belongs to")]
        public EvolutionTier tier;

        [Tooltip("Rarity of this pet")]
        public SpriteRarity rarity;

        public bool HasBreathAnimation => breathFrames != null && breathFrames.Length > 0;
        public bool HasLargeSprite => largeSprite != null;
    }

    /// <summary>
    /// ScriptableObject that maps pet IDs to their sprite sets (breath frames + large sprite).
    /// Built by SpriteImporterEditor from the breath-images and large-images folders.
    ///
    /// Performance: caches tier lookup dictionaries to avoid per-call allocations.
    /// </summary>
    [CreateAssetMenu(fileName = "PetSpriteMapper", menuName = "Pokiwar/Pet Sprite Mapper")]
    public class PetSpriteMapper : ScriptableObject
    {
        [SerializeField] private List<PetSpriteData> pets = new List<PetSpriteData>();

        // Runtime lookup caches
        private Dictionary<string, PetSpriteData> petLookup;
        private Dictionary<EvolutionTier, List<PetSpriteData>> tierCache;
        private bool cachesDirty = true;

        public int PetCount => pets.Count;
        public IReadOnlyList<PetSpriteData> AllPets => pets;

        private void OnEnable()
        {
            RebuildCaches();
        }

        private void RebuildCaches()
        {
            // Build ID lookup
            petLookup = new Dictionary<string, PetSpriteData>(
                pets.Count, System.StringComparer.OrdinalIgnoreCase);

            // Build tier cache
            tierCache = new Dictionary<EvolutionTier, List<PetSpriteData>>();
            foreach (EvolutionTier tier in System.Enum.GetValues(typeof(EvolutionTier)))
                tierCache[tier] = new List<PetSpriteData>();

            foreach (PetSpriteData pet in pets)
            {
                if (!string.IsNullOrEmpty(pet.petId))
                    petLookup[pet.petId] = pet;

                if (tierCache.TryGetValue(pet.tier, out List<PetSpriteData> tierList))
                    tierList.Add(pet);
            }

            cachesDirty = false;
        }

        private void EnsureCaches()
        {
            if (cachesDirty || petLookup == null)
                RebuildCaches();
        }

        /// <summary>
        /// Gets pet data by ID. Returns null if not found.
        /// </summary>
        public PetSpriteData GetPetById(string petId)
        {
            EnsureCaches();
            return petLookup.TryGetValue(petId, out PetSpriteData data) ? data : null;
        }

        /// <summary>
        /// Gets a random pet from the given evolution tier.
        /// Uses cached tier lists - O(1) lookup.
        /// </summary>
        public PetSpriteData GetRandomPetForTier(EvolutionTier tier)
        {
            EnsureCaches();

            if (tierCache.TryGetValue(tier, out List<PetSpriteData> tierPets) && tierPets.Count > 0)
                return tierPets[Random.Range(0, tierPets.Count)];

            // Fallback: return any pet
            return pets.Count > 0 ? pets[Random.Range(0, pets.Count)] : null;
        }

        /// <summary>
        /// Gets all pets for a given tier. Returns cached list (read-only).
        /// </summary>
        public IReadOnlyList<PetSpriteData> GetPetsForTier(EvolutionTier tier)
        {
            EnsureCaches();
            return tierCache.TryGetValue(tier, out List<PetSpriteData> list)
                ? list
                : new List<PetSpriteData>();
        }

        /// <summary>
        /// Gets a pet by index (for level-based assignment).
        /// </summary>
        public PetSpriteData GetPetByIndex(int index)
        {
            if (pets.Count == 0) return null;
            return pets[Mathf.Clamp(index, 0, pets.Count - 1)];
        }

        /// <summary>
        /// Gets a random pet from any tier.
        /// </summary>
        public PetSpriteData GetRandomPet()
        {
            if (pets.Count == 0) return null;
            return pets[Random.Range(0, pets.Count)];
        }

        /// <summary>
        /// Adds or updates a pet entry (used by editor tools and PetUpdateScheduler).
        /// Marks caches as dirty for rebuild on next access.
        /// </summary>
        public void AddOrUpdatePet(PetSpriteData petData)
        {
            if (petData == null || string.IsNullOrEmpty(petData.petId)) return;

            for (int i = 0; i < pets.Count; i++)
            {
                if (string.Equals(pets[i].petId, petData.petId, System.StringComparison.OrdinalIgnoreCase))
                {
                    pets[i] = petData;
                    cachesDirty = true;
                    return;
                }
            }

            pets.Add(petData);
            cachesDirty = true;
        }

        /// <summary>
        /// Removes a pet by ID.
        /// </summary>
        public bool RemovePet(string petId)
        {
            int removed = pets.RemoveAll(p =>
                string.Equals(p.petId, petId, System.StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
                cachesDirty = true;

            return removed > 0;
        }

        /// <summary>
        /// Clears all pet data (used by editor tools during reimport).
        /// </summary>
        public void Clear()
        {
            pets.Clear();
            cachesDirty = true;
        }

        /// <summary>
        /// Returns count of pets in a specific tier.
        /// </summary>
        public int GetTierCount(EvolutionTier tier)
        {
            EnsureCaches();
            return tierCache.TryGetValue(tier, out List<PetSpriteData> list) ? list.Count : 0;
        }
    }
}
