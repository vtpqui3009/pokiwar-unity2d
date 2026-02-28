using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Manages the pet catalog/gallery shown at game start.
    /// Shows a paginated subset of all available pets.
    /// Supports filtering by tier and rarity.
    /// New pets added via PetUpdateScheduler automatically appear here.
    /// </summary>
    public class PetCatalogManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PetSpriteMapper petSpriteMapper;

        [Header("UI")]
        [SerializeField] private Transform catalogContainer;
        [SerializeField] private GameObject petCardPrefab;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI pageInfoText;
        [SerializeField] private TextMeshProUGUI totalPetsText;

        [Header("Filter UI")]
        [SerializeField] private TMPro.TMP_Dropdown tierFilterDropdown;
        [SerializeField] private TMPro.TMP_Dropdown rarityFilterDropdown;

        [Header("Settings")]
        [SerializeField] private int petsPerPage = 12;
        [SerializeField] private bool showOnlyUnlocked = false;

        private int currentPage;
        private List<PetSpriteData> filteredPets = new List<PetSpriteData>();
        private EvolutionTier? tierFilter = null;
        private SpriteRarity? rarityFilter = null;

        public int TotalPages => filteredPets.Count > 0
            ? Mathf.CeilToInt((float)filteredPets.Count / petsPerPage)
            : 1;

        public int CurrentPage => currentPage;
        public int TotalPets => filteredPets.Count;

        private void Start()
        {
            prevPageButton?.onClick.AddListener(PreviousPage);
            nextPageButton?.onClick.AddListener(NextPage);

            tierFilterDropdown?.onValueChanged.AddListener(OnTierFilterChanged);
            rarityFilterDropdown?.onValueChanged.AddListener(OnRarityFilterChanged);

            RefreshCatalog();
        }

        /// <summary>
        /// Refreshes the catalog display. Call this after new pets are added.
        /// </summary>
        public void RefreshCatalog()
        {
            BuildFilteredList();
            currentPage = 0;
            DisplayCurrentPage();
        }

        private void BuildFilteredList()
        {
            filteredPets.Clear();

            if (petSpriteMapper == null) return;

            foreach (PetSpriteData pet in petSpriteMapper.AllPets)
            {
                if (tierFilter.HasValue && pet.tier != tierFilter.Value) continue;
                if (rarityFilter.HasValue && pet.rarity != rarityFilter.Value) continue;
                filteredPets.Add(pet);
            }

            // Sort by tier then by petId
            filteredPets.Sort((a, b) =>
            {
                int tierCompare = a.tier.CompareTo(b.tier);
                if (tierCompare != 0) return tierCompare;
                return string.Compare(a.petId, b.petId, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private void DisplayCurrentPage()
        {
            if (catalogContainer == null) return;

            // Clear existing cards
            for (int i = catalogContainer.childCount - 1; i >= 0; i--)
                Destroy(catalogContainer.GetChild(i).gameObject);

            int startIndex = currentPage * petsPerPage;
            int endIndex = Mathf.Min(startIndex + petsPerPage, filteredPets.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                CreatePetCard(filteredPets[i]);
            }

            UpdateNavigationUI();
        }

        private void CreatePetCard(PetSpriteData petData)
        {
            if (petCardPrefab == null || catalogContainer == null) return;

            GameObject card = Instantiate(petCardPrefab, catalogContainer);

            // Set pet image
            Image petImage = card.GetComponentInChildren<Image>();
            if (petImage != null)
            {
                Sprite preview = petData.HasBreathAnimation
                    ? petData.breathFrames[0]
                    : petData.largeSprite;
                if (preview != null)
                    petImage.sprite = preview;
            }

            // Set pet info text
            TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 1) texts[0].text = $"#{petData.petId}";
            if (texts.Length >= 2) texts[1].text = petData.tier.ToString();
            if (texts.Length >= 3) texts[2].text = petData.rarity.ToString();

            // Color card by rarity
            Image cardBg = card.GetComponent<Image>();
            if (cardBg != null)
                cardBg.color = GetRarityColor(petData.rarity);
        }

        private Color GetRarityColor(SpriteRarity rarity)
        {
            return rarity switch
            {
                SpriteRarity.Common    => new Color(0.8f, 0.8f, 0.8f, 0.3f),
                SpriteRarity.Uncommon  => new Color(0.4f, 0.8f, 0.4f, 0.3f),
                SpriteRarity.Rare      => new Color(0.4f, 0.6f, 1f, 0.3f),
                SpriteRarity.Epic      => new Color(0.7f, 0.3f, 1f, 0.3f),
                SpriteRarity.Legendary => new Color(1f, 0.8f, 0f, 0.3f),
                _                      => Color.clear
            };
        }

        private void UpdateNavigationUI()
        {
            if (pageInfoText != null)
                pageInfoText.text = $"Page {currentPage + 1} / {TotalPages}";

            if (totalPetsText != null)
                totalPetsText.text = $"{filteredPets.Count} pets";

            if (prevPageButton != null)
                prevPageButton.interactable = currentPage > 0;

            if (nextPageButton != null)
                nextPageButton.interactable = currentPage < TotalPages - 1;
        }

        public void NextPage()
        {
            if (currentPage < TotalPages - 1)
            {
                currentPage++;
                DisplayCurrentPage();
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                DisplayCurrentPage();
            }
        }

        public void GoToPage(int page)
        {
            currentPage = Mathf.Clamp(page, 0, TotalPages - 1);
            DisplayCurrentPage();
        }

        private void OnTierFilterChanged(int index)
        {
            // 0 = All, 1-5 = specific tiers
            tierFilter = index == 0 ? (EvolutionTier?)null : (EvolutionTier)(index - 1);
            RefreshCatalog();
        }

        private void OnRarityFilterChanged(int index)
        {
            // 0 = All, 1-5 = specific rarities
            rarityFilter = index == 0 ? (SpriteRarity?)null : (SpriteRarity)(index - 1);
            RefreshCatalog();
        }

        public void SetPetSpriteMapper(PetSpriteMapper mapper)
        {
            petSpriteMapper = mapper;
            RefreshCatalog();
        }

        /// <summary>
        /// Returns the pets on the current page (for external use).
        /// </summary>
        public IReadOnlyList<PetSpriteData> GetCurrentPagePets()
        {
            int startIndex = currentPage * petsPerPage;
            int endIndex = Mathf.Min(startIndex + petsPerPage, filteredPets.Count);
            return filteredPets.GetRange(startIndex, endIndex - startIndex);
        }
    }
}
