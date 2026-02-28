using UnityEngine;
using Unity.Netcode;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Handles collision and combat synchronization between networked players.
    /// </summary>
    public class PlayerSync : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float collisionCooldown = 0.5f;

        private float lastCollisionTime;
        private HealthController healthController;
        private EvolutionManager evolutionManager;

        private void Awake()
        {
            healthController = GetComponent<HealthController>();
            evolutionManager = GetComponent<EvolutionManager>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer)
                return;

            if (Time.time - lastCollisionTime < collisionCooldown)
                return;

            PlayerSync otherPlayer = collision.gameObject.GetComponent<PlayerSync>();
            if (otherPlayer != null && otherPlayer != this)
            {
                HandlePlayerCollision(otherPlayer);
            }
        }

        private void HandlePlayerCollision(PlayerSync otherPlayer)
        {
            if (evolutionManager == null || otherPlayer.evolutionManager == null)
                return;

            int myLevel = evolutionManager.GetLevel();
            int otherLevel = otherPlayer.evolutionManager.GetLevel();

            if (myLevel > otherLevel + 2)
            {
                float damage = CalculateDamage(myLevel, otherLevel);
                otherPlayer.TakeDamageServerRpc(damage, OwnerClientId);
            }
            else if (otherLevel > myLevel + 2)
            {
                float damage = otherPlayer.CalculateDamage(otherLevel, myLevel);
                TakeDamageServerRpc(damage, otherPlayer.OwnerClientId);
            }

            lastCollisionTime = Time.time;
            otherPlayer.lastCollisionTime = Time.time;
        }

        [ServerRpc]
        public void TakeDamageServerRpc(float damage, ulong attackerId)
        {
            if (healthController != null)
            {
                healthController.TakeDamage(damage, null);
            }
        }

        public float CalculateDamage(int attackerLevel, int targetLevel)
        {
            float levelDifference = attackerLevel - targetLevel;
            float baseDamage = 10f + (attackerLevel * 2f);

            if (levelDifference > 5)
                baseDamage *= 1.2f;
            else if (levelDifference < -5)
                baseDamage *= 0.8f;

            return baseDamage;
        }

        public int GetLevel()
        {
            return evolutionManager != null ? evolutionManager.GetLevel() : 1;
        }
    }
}
