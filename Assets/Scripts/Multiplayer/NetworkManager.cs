using UnityEngine;
using Unity.Netcode;
using Pokiwar.Core;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Custom NetworkManager for game initialization and player spawning.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject playerPrefab;

        [Header("UI")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject gameUI;

        private Unity.Netcode.NetworkManager networkManager;

        private void Awake()
        {
            networkManager = GetComponent<Unity.Netcode.NetworkManager>();
        }

        private void Start()
        {
            networkManager.OnClientConnected += OnClientConnected;
            networkManager.OnClientDisconnect += OnClientDisconnected;

            if (mainMenuUI != null)
                mainMenuUI.SetActive(true);
            if (gameUI != null)
                gameUI.SetActive(false);
        }

        public void StartHost()
        {
            if (networkManager != null)
            {
                networkManager.StartHost();
                OnStartServer();
            }
        }

        public void StartClient()
        {
            if (networkManager != null)
            {
                networkManager.StartClient();
            }
        }

        public void StartServer()
        {
            if (networkManager != null)
            {
                networkManager.StartServer();
                OnStartServer();
            }
        }

        private void OnStartServer()
        {
            Debug.Log("Server started");

            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);
            if (gameUI != null)
                gameUI.SetActive(true);

            GameState.IsGameActive = true;
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");
            GameState.PlayerCount++;
            SpawnPlayer(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client disconnected: {clientId}");
            GameState.PlayerCount--;
        }

        private void SpawnPlayer(ulong clientId)
        {
            if (playerPrefab == null || GameManager.Instance == null)
                return;

            Vector2 spawnPos = GameManager.Instance.GetRandomPosition();
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
        }

        public void Disconnect()
        {
            if (networkManager != null)
            {
                networkManager.Shutdown();
            }
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnected -= OnClientConnected;
                networkManager.OnClientDisconnect -= OnClientDisconnected;
            }
        }
    }
}
