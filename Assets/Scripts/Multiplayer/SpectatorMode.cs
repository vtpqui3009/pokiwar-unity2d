using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pokiwar.Combat;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Spectator mode: auto-follows killer after death, allows cycling through alive players.
    /// </summary>
    public class SpectatorMode : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float respawnCountdown = 5f;
        [SerializeField] private KeyCode nextPlayerKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode prevPlayerKey = KeyCode.LeftArrow;

        [Header("UI")]
        [SerializeField] private GameObject spectatorOverlay;

        private Camera mainCamera;
        private List<GameObject> alivePlayers = new List<GameObject>();
        private int currentSpectateIndex;
        private bool isSpectating;
        private float respawnTimer;
        private HealthController localHealthController;

        public bool IsSpectating => isSpectating;
        public float RespawnTimeRemaining => respawnTimer;

        public event System.Action OnRespawnReady;

        private void Awake()
        {
            mainCamera = Camera.main;
            localHealthController = GetComponent<HealthController>();
        }

        private void OnEnable()
        {
            if (localHealthController != null)
                localHealthController.OnDeath += OnLocalPlayerDied;
        }

        private void OnDisable()
        {
            if (localHealthController != null)
                localHealthController.OnDeath -= OnLocalPlayerDied;
        }

        private void Update()
        {
            if (!isSpectating) return;

            HandleSpectatorInput();
            FollowCurrentTarget();
            TickRespawnTimer();
        }

        private void OnLocalPlayerDied(GameObject killer)
        {
            StartSpectating(killer);
        }

        public void StartSpectating(GameObject initialTarget = null)
        {
            isSpectating = true;
            respawnTimer = respawnCountdown;
            spectatorOverlay?.SetActive(true);

            RefreshAlivePlayers();

            if (initialTarget != null && alivePlayers.Contains(initialTarget))
            {
                currentSpectateIndex = alivePlayers.IndexOf(initialTarget);
            }
            else if (alivePlayers.Count > 0)
            {
                currentSpectateIndex = 0;
            }

            DetachCamera();
        }

        public void StopSpectating()
        {
            isSpectating = false;
            spectatorOverlay?.SetActive(false);
            ReattachCamera();
        }

        private void HandleSpectatorInput()
        {
            if (Input.GetKeyDown(nextPlayerKey))
                CycleToNextPlayer();
            else if (Input.GetKeyDown(prevPlayerKey))
                CycleToPrevPlayer();
        }

        public void CycleToNextPlayer()
        {
            RefreshAlivePlayers();
            if (alivePlayers.Count == 0) return;

            currentSpectateIndex = (currentSpectateIndex + 1) % alivePlayers.Count;
        }

        public void CycleToPrevPlayer()
        {
            RefreshAlivePlayers();
            if (alivePlayers.Count == 0) return;

            currentSpectateIndex = (currentSpectateIndex - 1 + alivePlayers.Count) % alivePlayers.Count;
        }

        private void FollowCurrentTarget()
        {
            if (alivePlayers.Count == 0 || mainCamera == null) return;

            // Clean up destroyed players
            alivePlayers.RemoveAll(p => p == null);
            if (alivePlayers.Count == 0) return;

            currentSpectateIndex = Mathf.Clamp(currentSpectateIndex, 0, alivePlayers.Count - 1);
            GameObject target = alivePlayers[currentSpectateIndex];

            if (target != null)
            {
                Vector3 targetPos = target.transform.position;
                targetPos.z = mainCamera.transform.position.z;
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, 5f * Time.deltaTime);
            }
        }

        private void TickRespawnTimer()
        {
            if (respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    respawnTimer = 0f;
                    OnRespawnReady?.Invoke();
                }
            }
        }

        private void RefreshAlivePlayers()
        {
            alivePlayers.Clear();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                HealthController hc = p.GetComponent<HealthController>();
                if (hc != null && !hc.IsDead && p != gameObject)
                    alivePlayers.Add(p);
            }
        }

        private void DetachCamera()
        {
            if (mainCamera != null)
                mainCamera.transform.SetParent(null);
        }

        private void ReattachCamera()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.SetParent(transform);
                mainCamera.transform.localPosition = new Vector3(0, 0, -10);
            }
        }
    }
}
