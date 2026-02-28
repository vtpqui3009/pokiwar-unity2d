using UnityEngine;
using Unity.Netcode;
using Pokiwar.Core;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Custom NetworkManager for game initialization, lobby system, and player spawning.
    /// Handles room code generation, player name sync, and reconnection.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject playerPrefab;

        [Header("UI")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject lobbyUI;

        [Header("Lobby Settings")]
        [SerializeField] private int minPlayersToStart = 1;

        private Unity.Netcode.NetworkManager networkManager;
        private string currentRoomCode;
        private string localPlayerName = "Player";

        public string RoomCode => currentRoomCode;
        public string LocalPlayerName => localPlayerName;
        public bool IsConnected => networkManager != null && networkManager.IsConnectedClient;

        // Events
        public event System.Action<string> OnRoomCreated;
        public event System.Action<ulong> OnPlayerJoined;
        public event System.Action<ulong> OnPlayerLeft;

        private void Awake()
        {
            networkManager = GetComponent<Unity.Netcode.NetworkManager>();

            // Load saved player name
            localPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        }

        private void Start()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnected += OnClientConnected;
                networkManager.OnClientDisconnect += OnClientDisconnected;
            }

            ShowMainMenu();
        }

        public void SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            localPlayerName = name.Trim();
            PlayerPrefs.SetString("PlayerName", localPlayerName);
            PlayerPrefs.Save();
        }

        public void StartHost(GameMode mode = GameMode.FFA)
        {
            if (networkManager == null) return;

            currentRoomCode = GameManager.Instance != null
                ? GameManager.Instance.GenerateRoomCode()
                : GenerateFallbackRoomCode();

            networkManager.StartHost();
            OnStartServer(mode);
            OnRoomCreated?.Invoke(currentRoomCode);
        }

        public void StartClient(string roomCode = "")
        {
            if (networkManager == null) return;

            currentRoomCode = roomCode;
            networkManager.StartClient();
        }

        public void StartServer(GameMode mode = GameMode.FFA)
        {
            if (networkManager == null) return;

            currentRoomCode = GameManager.Instance != null
                ? GameManager.Instance.GenerateRoomCode()
                : GenerateFallbackRoomCode();

            networkManager.StartServer();
            OnStartServer(mode);
        }

        private void OnStartServer(GameMode mode)
        {
            GameState.StartGame(mode, currentRoomCode);
            GameManager.Instance?.StartSession(mode, currentRoomCode);

            ShowGameUI();
        }

        private void OnClientConnected(ulong clientId)
        {
            GameState.PlayerCount++;
            SpawnPlayer(clientId);
            OnPlayerJoined?.Invoke(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            GameState.PlayerCount = Mathf.Max(0, GameState.PlayerCount - 1);
            OnPlayerLeft?.Invoke(clientId);
        }

        private void SpawnPlayer(ulong clientId)
        {
            if (playerPrefab == null || GameManager.Instance == null) return;

            Vector2 spawnPos = GameManager.Instance.GetRandomPosition();
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
                networkObject.SpawnAsPlayerObject(clientId);

            GameManager.Instance.RegisterPlayer(player);
        }

        public void Disconnect()
        {
            networkManager?.Shutdown();
            GameState.ResetGame();
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            mainMenuUI?.SetActive(true);
            gameUI?.SetActive(false);
            lobbyUI?.SetActive(false);
        }

        private void ShowLobby()
        {
            mainMenuUI?.SetActive(false);
            gameUI?.SetActive(false);
            lobbyUI?.SetActive(true);
        }

        private void ShowGameUI()
        {
            mainMenuUI?.SetActive(false);
            gameUI?.SetActive(true);
            lobbyUI?.SetActive(false);
        }

        private string GenerateFallbackRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[6];
            for (int i = 0; i < code.Length; i++)
                code[i] = chars[Random.Range(0, chars.Length)];
            return new string(code);
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
