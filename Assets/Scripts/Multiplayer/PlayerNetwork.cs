using UnityEngine;
using Unity.Netcode;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Networked player component using Unity Netcode for GameObjects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private EvolutionManager evolutionManager;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Network Settings")]
        [SerializeField] private float positionUpdateRate = 0.05f;

        private NetworkVariable<Vector2> netPosition = new NetworkVariable<Vector2>();
        private NetworkVariable<int> netLevel = new NetworkVariable<int>(1);
        private NetworkVariable<int> netSpriteId = new NetworkVariable<int>(1);
        private NetworkVariable<float> netXP = new NetworkVariable<float>(0f);
        private NetworkVariable<float> netMaxXP = new NetworkVariable<float>(10f);

        private float positionTimer;
        private Camera mainCamera;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            if (evolutionManager == null)
                evolutionManager = GetComponent<EvolutionManager>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            mainCamera = Camera.main;

            if (IsOwner)
            {
                SetupOwner();
            }
            else
            {
                SetupRemote();
            }

            netLevel.OnValueChanged += OnLevelChanged;
            netSpriteId.OnValueChanged += OnSpriteIdChanged;
        }

        private void SetupOwner()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.SetParent(transform);
                mainCamera.transform.localPosition = new Vector3(0, 0, -10);
            }

            gameObject.tag = "Player";
        }

        private void SetupRemote()
        {
            gameObject.tag = "Player";

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.8f, 0.8f);
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                UpdateOwner();
            }
            else
            {
                UpdateRemote();
            }
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
            }
        }

        private void UpdateRemote()
        {
            transform.position = Vector3.Lerp(transform.position, netPosition.Value, 10f * Time.deltaTime);
        }

        private void OnLevelChanged(int oldLevel, int newLevel)
        {
        }

        private void OnSpriteIdChanged(int oldId, int newId)
        {
        }

        public override void OnNetworkDespawn()
        {
            netLevel.OnValueChanged -= OnLevelChanged;
            netSpriteId.OnValueChanged -= OnSpriteIdChanged;
            base.OnNetworkDespawn();
        }
    }
}
