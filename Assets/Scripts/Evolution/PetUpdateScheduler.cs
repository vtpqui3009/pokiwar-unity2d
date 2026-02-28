using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Scheduled pet update entry.
    /// </summary>
    [System.Serializable]
    public class PetUpdateBatch
    {
        [Tooltip("Unique batch identifier")]
        public string batchId;

        [Tooltip("Display name for this update")]
        public string batchName;

        [Tooltip("When this batch becomes available (UTC timestamp as string, or empty for immediate)")]
        public string scheduledDateUtc;

        [Tooltip("New pets to add in this batch")]
        public List<PetSpriteData> newPets = new List<PetSpriteData>();

        [Tooltip("Whether this batch has been applied")]
        public bool isApplied;

        [Tooltip("Whether this batch is enabled")]
        public bool isEnabled = true;
    }

    /// <summary>
    /// Admin system for scheduling and automatically applying new pet batches.
    ///
    /// Features:
    /// - Define pet update batches with scheduled release dates
    /// - Auto-applies batches when their scheduled time arrives
    /// - Persists applied batch IDs to PlayerPrefs to avoid re-applying
    /// - Fires events when new pets are added (for UI refresh)
    /// - Can be triggered manually by admin
    ///
    /// Usage:
    /// 1. Add PetUpdateBatch entries in the Inspector
    /// 2. Set scheduledDateUtc (e.g. "2025-03-01T00:00:00Z") or leave empty for immediate
    /// 3. The scheduler checks every checkIntervalSeconds and applies due batches
    /// </summary>
    public class PetUpdateScheduler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PetSpriteMapper petSpriteMapper;
        [SerializeField] private PetCatalogManager catalogManager;

        [Header("Update Batches")]
        [SerializeField] private List<PetUpdateBatch> updateBatches = new List<PetUpdateBatch>();

        [Header("Settings")]
        [SerializeField] private float checkIntervalSeconds = 60f;
        [SerializeField] private bool autoApplyOnStart = true;
        [SerializeField] private bool logUpdates = true;

        private const string AppliedBatchesKey = "PokiwarAppliedBatches";

        // Events
        public event System.Action<PetUpdateBatch> OnBatchApplied;
        public event System.Action<int> OnNewPetsAdded; // count of new pets

        private void Start()
        {
            if (autoApplyOnStart)
                CheckAndApplyDueBatches();

            StartCoroutine(ScheduledCheckRoutine());
        }

        private IEnumerator ScheduledCheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkIntervalSeconds);
                CheckAndApplyDueBatches();
            }
        }

        /// <summary>
        /// Checks all batches and applies any that are due and not yet applied.
        /// </summary>
        public void CheckAndApplyDueBatches()
        {
            if (petSpriteMapper == null) return;

            HashSet<string> appliedIds = LoadAppliedBatchIds();
            int totalNewPets = 0;

            foreach (PetUpdateBatch batch in updateBatches)
            {
                if (!batch.isEnabled) continue;
                if (appliedIds.Contains(batch.batchId)) continue;
                if (!IsBatchDue(batch)) continue;

                int added = ApplyBatch(batch);
                totalNewPets += added;
                appliedIds.Add(batch.batchId);
                batch.isApplied = true;

                OnBatchApplied?.Invoke(batch);

                if (logUpdates)
                    Debug.Log($"[PetUpdateScheduler] Applied batch '{batch.batchName}' ({added} new pets)");
            }

            if (totalNewPets > 0)
            {
                SaveAppliedBatchIds(appliedIds);
                OnNewPetsAdded?.Invoke(totalNewPets);
                catalogManager?.RefreshCatalog();
            }
        }

        private bool IsBatchDue(PetUpdateBatch batch)
        {
            if (string.IsNullOrEmpty(batch.scheduledDateUtc))
                return true; // No schedule = immediate

            if (System.DateTime.TryParse(batch.scheduledDateUtc,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out System.DateTime scheduledTime))
            {
                return System.DateTime.UtcNow >= scheduledTime;
            }

            // If date parsing fails, apply immediately
            Debug.LogWarning($"[PetUpdateScheduler] Could not parse date '{batch.scheduledDateUtc}' for batch '{batch.batchId}'. Applying immediately.");
            return true;
        }

        private int ApplyBatch(PetUpdateBatch batch)
        {
            if (petSpriteMapper == null || batch.newPets == null) return 0;

            int count = 0;
            foreach (PetSpriteData pet in batch.newPets)
            {
                if (string.IsNullOrEmpty(pet.petId)) continue;

                // Only add if not already present
                if (petSpriteMapper.GetPetById(pet.petId) == null)
                {
                    petSpriteMapper.AddOrUpdatePet(pet);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Manually applies a specific batch by ID (admin use).
        /// </summary>
        public bool ForceApplyBatch(string batchId)
        {
            PetUpdateBatch batch = updateBatches.Find(b => b.batchId == batchId);
            if (batch == null)
            {
                Debug.LogWarning($"[PetUpdateScheduler] Batch '{batchId}' not found.");
                return false;
            }

            int added = ApplyBatch(batch);
            batch.isApplied = true;

            HashSet<string> appliedIds = LoadAppliedBatchIds();
            appliedIds.Add(batchId);
            SaveAppliedBatchIds(appliedIds);

            OnBatchApplied?.Invoke(batch);
            OnNewPetsAdded?.Invoke(added);
            catalogManager?.RefreshCatalog();

            if (logUpdates)
                Debug.Log($"[PetUpdateScheduler] Force-applied batch '{batch.batchName}' ({added} new pets)");

            return true;
        }

        /// <summary>
        /// Resets all applied batch records (admin use - for testing).
        /// </summary>
        public void ResetAllAppliedBatches()
        {
            PlayerPrefs.DeleteKey(AppliedBatchesKey);
            foreach (PetUpdateBatch batch in updateBatches)
                batch.isApplied = false;

            Debug.Log("[PetUpdateScheduler] Reset all applied batch records.");
        }

        /// <summary>
        /// Returns a summary of all batches and their status.
        /// </summary>
        public List<(string id, string name, bool applied, bool due)> GetBatchStatus()
        {
            HashSet<string> appliedIds = LoadAppliedBatchIds();
            var result = new List<(string, string, bool, bool)>();

            foreach (PetUpdateBatch batch in updateBatches)
            {
                result.Add((
                    batch.batchId,
                    batch.batchName,
                    appliedIds.Contains(batch.batchId),
                    IsBatchDue(batch)
                ));
            }

            return result;
        }

        private HashSet<string> LoadAppliedBatchIds()
        {
            string saved = PlayerPrefs.GetString(AppliedBatchesKey, "");
            HashSet<string> ids = new HashSet<string>();

            if (!string.IsNullOrEmpty(saved))
            {
                foreach (string id in saved.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        ids.Add(id);
                }
            }

            return ids;
        }

        private void SaveAppliedBatchIds(HashSet<string> ids)
        {
            PlayerPrefs.SetString(AppliedBatchesKey, string.Join(",", ids));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Adds a new batch at runtime (for dynamic content updates).
        /// </summary>
        public void AddBatch(PetUpdateBatch batch)
        {
            if (batch == null || string.IsNullOrEmpty(batch.batchId)) return;

            // Remove existing batch with same ID
            updateBatches.RemoveAll(b => b.batchId == batch.batchId);
            updateBatches.Add(batch);
        }
    }
}
