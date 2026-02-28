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
    /// </summary>
    [CreateAssetMenu(fileName = "PetSpriteMapper", menuName = "Pokiwar/Pet Sprite Mapper")]
    public class PetSpriteMapper : ScriptableObject
    {
        [SerializeField] private List<PetSpriteData> pets = new List<PetSpriteData>();

        // Runtime lookup cache
        private Dictionary<string, PetSpriteData> petLookup;

        public int PetCount => pets.Count;
        public IReadOnlyList<PetSpriteData> AllPets => pets;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            petLookup = new Dictionary<string, PetSpriteData>(System.StringComparer.OrdinalIgnoreCase);
            foreach (PetSpriteData pet in pets)
            {
                if (!string.IsNullOrEmpty(pet.petId))
                    petLookup[pet.petId] = pet;
            }
        }

        /// <summary>
        /// Gets pet data by ID. Returns null if not found.
        /// </summary>
        public PetSpriteData GetPetById(string petId)
        {
            if (petLookup == null) BuildLookup();
            return petLookup.TryGetValue(petId, out PetSpriteData data) ? data : null;
        }

        /// <summary>
        /// Gets a random pet from the given evolution tier.
        /// </summary>
        public PetSpriteData GetRandomPetForTier(EvolutionTier tier)
        {
            List<PetSpriteData> tierPets = new List<PetSpriteData>();
            foreach (PetSpriteData pet in pets)
            {
                if (pet.tier == tier)
                    tierPets.Add(pet);
            }

            if (tierPets.Count == 0)
            {
                // Fallback: return any pet
                return pets.Count > 0 ? pets[Random.Range(0, pets.Count)] : null;
            }

            return tierPets[Random.Range(0, tierPets.Count)];
        }

        /// <summary>
        /// Gets all pets for a given tier.
        /// </summary>
        public List<PetSpriteData> GetPetsForTier(EvolutionTier tier)
        {
            List<PetSpriteData> result = new List<PetSpriteData>();
            foreach (PetSpriteData pet in pets)
            {
                if (pet.tier == tier)
                    result.Add(pet);
            }
            return result;
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
        /// Adds or updates a pet entry (used by editor tools).
        /// </summary>
        public void AddOrUpdatePet(PetSpriteData petData)
        {
            for (int i = 0; i < pets.Count; i++)
            {
                if (pets[i].petId == petData.petId)
                {
                    pets[i] = petData;
                    BuildLookup();
                    return;
                }
            }
            pets.Add(petData);
            BuildLookup();
        }

        /// <summary>
        /// Clears all pet data (used by editor tools during reimport).
        /// </summary>
        public void Clear()
        {
            pets.Clear();
            petLookup?.Clear();
        }
    }
}
