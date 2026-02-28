using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pokiwar.Core;
using Pokiwar.Multiplayer;

namespace Pokiwar.UI
{
    /// <summary>
    /// Main menu UI: player name entry, room code, game mode selection, and settings.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField roomCodeInput;

        [Header("Buttons")]
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button settingsButton;

        [Header("Game Mode")]
        [SerializeField] private Toggle ffaModeToggle;
        [SerializeField] private Toggle survivalModeToggle;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private TextMeshProUGUI roomCodeDisplay;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private NetworkManager networkManager;
        private GameMode selectedMode = GameMode.FFA;

        private void Start()
        {
            networkManager = FindObjectOfType<NetworkManager>();

            // Load saved values
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            if (playerNameInput != null)
                playerNameInput.text = savedName;

            // Load volume settings
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            // Wire up buttons
            createRoomButton?.onClick.AddListener(OnCreateRoom);
            joinRoomButton?.onClick.AddListener(OnJoinRoom);
            settingsButton?.onClick.AddListener(ToggleSettings);

            ffaModeToggle?.onValueChanged.AddListener(v => { if (v) selectedMode = GameMode.FFA; });
            survivalModeToggle?.onValueChanged.AddListener(v => { if (v) selectedMode = GameMode.Survival; });

            masterVolumeSlider?.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("MasterVolume", v);
            });

            settingsPanel?.SetActive(false);

            if (networkManager != null)
                networkManager.OnRoomCreated += OnRoomCreated;
        }

        private void OnDestroy()
        {
            if (networkManager != null)
                networkManager.OnRoomCreated -= OnRoomCreated;
        }

        private void OnCreateRoom()
        {
            SavePlayerName();
            networkManager?.StartHost(selectedMode);
        }

        private void OnJoinRoom()
        {
            SavePlayerName();
            string code = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpper() : "";
            networkManager?.StartClient(code);
        }

        private void SavePlayerName()
        {
            if (playerNameInput == null) return;

            string name = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Player";

            networkManager?.SetPlayerName(name);
        }

        private void OnRoomCreated(string code)
        {
            if (roomCodeDisplay != null)
                roomCodeDisplay.text = $"Room: {code}";
        }

        private void ToggleSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        private void Update()
        {
            if (playerCountText != null)
                playerCountText.text = $"Players: {GameState.PlayerCount}";
        }
    }
}
