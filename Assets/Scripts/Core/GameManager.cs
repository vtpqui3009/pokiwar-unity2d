using UnityEngine;

namespace Pokiwar.Core
{
    /// <summary>
    /// Singleton game manager. Handles global game state and initialization.
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

        public float MapWidth => mapWidth;
        public float MapHeight => mapHeight;
        public int MaxPlayers => maxPlayers;
        public GameObject PlayerPrefab => playerPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(mapWidth, mapHeight, 0));
        }
    }
}
