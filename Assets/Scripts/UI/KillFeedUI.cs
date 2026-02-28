using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Pokiwar.UI
{
    /// <summary>
    /// Kill feed showing last 5 kills with fade-out and slide-in animation.
    /// </summary>
    public class KillFeedUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform feedContainer;
        [SerializeField] private GameObject entryPrefab;

        [Header("Settings")]
        [SerializeField] private int maxEntries = 5;
        [SerializeField] private float displayDuration = 5f;
        [SerializeField] private Color localKillColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color normalKillColor = Color.white;

        private readonly Queue<GameObject> activeEntries = new Queue<GameObject>();
        private string localPlayerName;

        private void Start()
        {
            localPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        }

        public void AddEntry(string killer, string victim)
        {
            if (feedContainer == null) return;

            // Remove oldest if at max
            if (activeEntries.Count >= maxEntries)
            {
                GameObject oldest = activeEntries.Dequeue();
                if (oldest != null) Destroy(oldest);
            }

            GameObject entry = CreateEntry(killer, victim);
            if (entry != null)
            {
                activeEntries.Enqueue(entry);
                StartCoroutine(FadeOutEntry(entry, displayDuration));
            }
        }

        private GameObject CreateEntry(string killer, string victim)
        {
            if (entryPrefab == null)
            {
                // Create a simple text entry
                GameObject obj = new GameObject("KillFeedEntry");
                obj.transform.SetParent(feedContainer, false);

                TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
                text.text = $"{killer} defeated {victim}";
                text.fontSize = 14f;

                bool isLocalKill = killer == localPlayerName;
                text.color = isLocalKill ? localKillColor : normalKillColor;

                return obj;
            }

            GameObject entry = Instantiate(entryPrefab, feedContainer);
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();

            bool isLocal = killer == localPlayerName;
            Color entryColor = isLocal ? localKillColor : normalKillColor;

            if (texts.Length >= 1)
            {
                texts[0].text = $"{killer} defeated {victim}";
                texts[0].color = entryColor;
            }

            return entry;
        }

        private IEnumerator FadeOutEntry(GameObject entry, float duration)
        {
            yield return new WaitForSeconds(duration * 0.7f);

            float fadeTime = duration * 0.3f;
            float elapsed = 0f;

            CanvasGroup cg = entry.GetComponent<CanvasGroup>();
            if (cg == null) cg = entry.AddComponent<CanvasGroup>();

            while (elapsed < fadeTime && entry != null)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / fadeTime);
                yield return null;
            }

            if (entry != null)
            {
                if (activeEntries.Count > 0)
                    activeEntries.Dequeue();
                Destroy(entry);
            }
        }
    }
}
