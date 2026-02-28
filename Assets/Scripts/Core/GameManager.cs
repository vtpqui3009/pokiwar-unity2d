using UnityEngine;
using System.Collections.Generic;

namespace Pokiwar.Core
{
    /// <summary>
    /// Singleton game manager. Handles global game state, initialization, and player registry.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private float mapWidth = 100f;
        [SerializeField] private float mapHeight = 100f;
        [SerializeField] private int maxPlayers = 20;

        [Header("References")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Room Settings")]
        [SerializeField] private GameMode defaultGameMode = GameMode.FFA;

        // Player registry
        private readonly List<GameObject> activePlayers = new List<GameObject>();

        // Session timer
        private float sessionTimer;
        private bool sessionRunning;

        public float MapWidth => mapWidth;
        public float MapHeight => mapHeight;
        public int MaxPlayers => maxPlayers;
        public GameObject PlayerPrefab => playerPrefab;
        public float SessionTime => sessionTimer;
        public IReadOnlyList<GameObject> ActivePlayers => activePlayers;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            if (sessionRunning)
            {
                sessionTimer += Time.deltaTime;
                GameState.GameTime = sessionTimer;
            }

            // Clean up destroyed players from registry
            activePlayers.RemoveAll(p => p == null);
        }

        public void StartSession(GameMode mode, string roomCode)
        {
            sessionTimer = 0f;
            sessionRunning = true;
            GameState.StartGame(mode, roomCode);
        }

        public void EndSession()
        {
            sessionRunning = false;
            GameState.EndGame();
        }

        public string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[6];
            for (int i = 0; i < code.Length; i++)
                code[i] = chars[Random.Range(0, chars.Length)];
            return new string(code);
        }

        public void RegisterPlayer(GameObject player)
        {
            if (player != null && !activePlayers.Contains(player))
            {
                activePlayers.Add(player);
                GameState.PlayerCount = activePlayers.Count;
            }
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (activePlayers.Remove(player))
            {
                GameState.PlayerCount = activePlayers.Count;
            }
        }

        public GameObject GetTopPlayer()
        {
            GameObject top = null;
            int topLevel = 0;

            foreach (GameObject p in activePlayers)
            {
                if (p == null) continue;
                Evolution.EvolutionManager em = p.GetComponent<Evolution.EvolutionManager>();
                if (em != null && em.GetLevel() > topLevel)
                {
                    topLevel = em.GetLevel();
                    top = p;
                }
            }

            GameState.TopPlayer = top;
            return top;
        }

        public Vector2 GetRandomPosition()
        {
            float x = Random.Range(-mapWidth / 2f, mapWidth / 2f);
            float y = Random.Range(-mapHeight / 2f, mapHeight / 2f);
            return new Vector2(x, y);
        }

        public bool IsPositionInBounds(Vector2 position)
        {
            return position.x >= -mapWidth / 2f && position.x <= mapWidth / 2f &&
                   position.y >= -mapHeight / 2f && position.y <= mapHeight / 2f;
        }

        public Vector2 ClampToBounds(Vector2 position)
        {
            float halfW = mapWidth / 2f;
            float halfH = mapHeight / 2f;
            return new Vector2(
                Mathf.Clamp(position.x, -halfW, halfW),
                Mathf.Clamp(position.y, -halfH, halfH)
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(mapWidth, mapHeight, 0));
        }
    }
}
