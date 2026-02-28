using UnityEngine;
using Unity.Netcode;
using TMPro;
using Pokiwar.Core;
using Pokiwar.Evolution;
using Pokiwar.Combat;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Networked player component using Unity Netcode for GameObjects.
    /// Syncs position, level, health, shield, boost, and player name.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private EvolutionManager evolutionManager;
        [SerializeField] private HealthController healthController;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Name Tag")]
        [SerializeField] private TextMeshPro nameTagText;
        [SerializeField] private Vector3 nameTagOffset = new Vector3(0f, 0.8f, 0f);

        [Header("Network Settings")]
        [SerializeField] private float positionUpdateRate = 0.05f;

        // Synced variables
        private NetworkVariable<Vector2> netPosition = new NetworkVariable<Vector2>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<int> netLevel = new NetworkVariable<int>(1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<int> netSpriteId = new NetworkVariable<int>(1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netXP = new NetworkVariable<float>(0f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netMaxXP = new NetworkVariable<float>(10f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netHealth = new NetworkVariable<float>(100f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netMaxHealth = new NetworkVariable<float>(100f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> netIsShielded = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> netIsBoosting = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Player name uses a fixed-size string via NetworkVariable<Unity.Collections.FixedString64Bytes>
        private NetworkVariable<Unity.Collections.FixedString64Bytes> netPlayerName =
            new NetworkVariable<Unity.Collections.FixedString64Bytes>(
                "Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private float positionTimer;
        private Camera mainCamera;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (evolutionManager == null) evolutionManager = GetComponent<EvolutionManager>();
            if (healthController == null) healthController = GetComponent<HealthController>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            mainCamera = Camera.main;

            if (IsOwner)
                SetupOwner();
            else
                SetupRemote();

            netLevel.OnValueChanged += OnLevelChanged;
            netSpriteId.OnValueChanged += OnSpriteIdChanged;
            netPlayerName.OnValueChanged += OnPlayerNameChanged;

            UpdateNameTag(netPlayerName.Value.ToString());
        }

        private void SetupOwner()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.SetParent(transform);
                mainCamera.transform.localPosition = new Vector3(0, 0, -10);
            }

            gameObject.tag = "Player";

            // Set player name from saved prefs
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            netPlayerName.Value = savedName;
            evolutionManager?.SetPlayerName(savedName);
        }

        private void SetupRemote()
        {
            gameObject.tag = "Player";

            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 0.8f, 0.8f);
        }

        private void Update()
        {
            if (IsOwner)
                UpdateOwner();
            else
                UpdateRemote();

            UpdateNameTagPosition();
        }

        private void UpdateOwner()
        {
            positionTimer += Time.deltaTime;
            if (positionTimer >= positionUpdateRate)
            {
                positionTimer = 0f;
                netPosition.Value = transform.position;

                if (evolutionManager != null)
                {
                    netLevel.Value = evolutionManager.GetLevel();
                    netSpriteId.Value = evolutionManager.GetSpriteId();
                    netXP.Value = evolutionManager.GetXP();
                    netMaxXP.Value = evolutionManager.GetMaxXP();
                }

                if (healthController != null)
                {
                    netHealth.Value = healthController.CurrentHealth;
                    netMaxHealth.Value = healthController.MaxHealth;
                    netIsShielded.Value = healthController.IsShielded;
                }
            }
        }

        private void UpdateRemote()
        {
            transform.position = Vector3.Lerp(transform.position, netPosition.Value, 10f * Time.deltaTime);
        }

        private void UpdateNameTagPosition()
        {
            if (nameTagText != null)
                nameTagText.transform.position = transform.position + nameTagOffset;
        }

        private void UpdateNameTag(string name)
        {
            if (nameTagText != null)
                nameTagText.text = name;
        }

        private void OnLevelChanged(int oldLevel, int newLevel) { }

        private void OnSpriteIdChanged(int oldId, int newId) { }

        private void OnPlayerNameChanged(Unity.Collections.FixedString64Bytes oldName,
            Unity.Collections.FixedString64Bytes newName)
        {
            UpdateNameTag(newName.ToString());
        }

        public override void OnNetworkDespawn()
        {
            netLevel.OnValueChanged -= OnLevelChanged;
            netSpriteId.OnValueChanged -= OnSpriteIdChanged;
            netPlayerName.OnValueChanged -= OnPlayerNameChanged;
            base.OnNetworkDespawn();
        }

        // Public accessors for UI
        public float GetNetHealth() => netHealth.Value;
        public float GetNetMaxHealth() => netMaxHealth.Value;
        public int GetNetLevel() => netLevel.Value;
        public string GetNetPlayerName() => netPlayerName.Value.ToString();
        public bool GetNetIsShielded() => netIsShielded.Value;
    }
}
