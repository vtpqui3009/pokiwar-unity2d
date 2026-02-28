using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.GameModes
{
    /// <summary>
    /// Manages game mode selection, initialization, and win condition checking.
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [Header("Mode References")]
        [SerializeField] private FFAMode ffaMode;
        [SerializeField] private SurvivalMode survivalMode;

        private IGameMode activeMode;

        public event System.Action<string> OnGameOver;

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
            InitializeMode(GameState.CurrentGameMode);
        }

        private void Update()
        {
            activeMode?.Update();
        }

        public void InitializeMode(GameMode mode)
        {
            activeMode?.Cleanup();

            switch (mode)
            {
                case GameMode.FFA:
                    activeMode = ffaMode ?? gameObject.AddComponent<FFAMode>();
                    break;
                case GameMode.Survival:
                    activeMode = survivalMode ?? gameObject.AddComponent<SurvivalMode>();
                    break;
                case GameMode.Practice:
                    activeMode = ffaMode ?? gameObject.AddComponent<FFAMode>();
                    break;
            }

            activeMode?.Initialize();
            activeMode?.OnGameOver(winner => OnGameOver?.Invoke(winner));
        }

        public bool IsPvPAllowed(Vector2 position)
        {
            return activeMode?.IsPvPAllowed(position) ?? true;
        }

        public bool ShouldRespawn()
        {
            return activeMode?.ShouldRespawn() ?? true;
        }
    }

    /// <summary>
    /// Interface for game mode implementations.
    /// </summary>
    public interface IGameMode
    {
        void Initialize();
        void Update();
        void Cleanup();
        bool IsPvPAllowed(Vector2 position);
        bool ShouldRespawn();
        void OnGameOver(System.Action<string> callback);
    }
}
