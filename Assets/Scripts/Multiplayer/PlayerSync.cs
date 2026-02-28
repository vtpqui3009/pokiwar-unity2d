using UnityEngine;
using Unity.Netcode;
using Pokiwar.Combat;
using Pokiwar.Evolution;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Handles collision and combat synchronization between networked players.
    /// Server-authoritative damage validation with per-pair collision cooldown and knockback.
    /// </summary>
    public class PlayerSync : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float collisionCooldown = 0.5f;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private int minLevelDifferenceForDamage = 2;

        private HealthController healthController;
        private EvolutionManager evolutionManager;
        private Rigidbody2D rb;

        // Per-pair cooldown: key = other player's NetworkObjectId
        private System.Collections.Generic.Dictionary<ulong, float> pairCooldowns
            = new System.Collections.Generic.Dictionary<ulong, float>();

        private void Awake()
        {
            healthController = GetComponent<HealthController>();
            evolutionManager = GetComponent<EvolutionManager>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Tick down per-pair cooldowns
            var keys = new System.Collections.Generic.List<ulong>(pairCooldowns.Keys);
            foreach (ulong key in keys)
            {
                pairCooldowns[key] -= Time.deltaTime;
                if (pairCooldowns[key] <= 0f)
                    pairCooldowns.Remove(key);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServer) return;

            PlayerSync otherPlayer = collision.gameObject.GetComponent<PlayerSync>();
            if (otherPlayer == null || otherPlayer == this) return;

            ulong otherId = otherPlayer.NetworkObjectId;
            if (pairCooldowns.ContainsKey(otherId)) return;

            HandlePlayerCollision(otherPlayer, collision);
        }

        private void HandlePlayerCollision(PlayerSync otherPlayer, Collision2D collision)
        {
            if (evolutionManager == null || otherPlayer.evolutionManager == null) return;

            int myLevel = evolutionManager.GetLevel();
            int otherLevel = otherPlayer.evolutionManager.GetLevel();

            // Anti-cheat: validate level difference
            if (myLevel > otherLevel + minLevelDifferenceForDamage)
            {
                float damage = CalculateDamage(myLevel, otherLevel);
                otherPlayer.TakeDamageServerRpc(damage, OwnerClientId);
                ApplyKnockbackClientRpc(otherPlayer.transform.position - transform.position);
            }
            else if (otherLevel > myLevel + minLevelDifferenceForDamage)
            {
                float damage = otherPlayer.CalculateDamage(otherLevel, myLevel);
                TakeDamageServerRpc(damage, otherPlayer.OwnerClientId);
                otherPlayer.ApplyKnockbackClientRpc(transform.position - otherPlayer.transform.position);
            }

            // Set per-pair cooldown
            ulong otherId = otherPlayer.NetworkObjectId;
            pairCooldowns[otherId] = collisionCooldown;
            otherPlayer.pairCooldowns[NetworkObjectId] = collisionCooldown;
        }

        [ServerRpc]
        public void TakeDamageServerRpc(float damage, ulong attackerId)
        {
            if (damage <= 0f) return;

            // Server-side validation: ensure damage is reasonable
            damage = Mathf.Clamp(damage, 0f, 500f);
            healthController?.TakeDamage(damage, null);
        }

        [ClientRpc]
        private void ApplyKnockbackClientRpc(Vector3 direction)
        {
            if (!IsOwner || rb == null) return;

            Vector2 knockback = direction.normalized * knockbackForce;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        public float CalculateDamage(int attackerLevel, int targetLevel)
        {
            float levelDifference = attackerLevel - targetLevel;
            float baseDamage = 10f + (attackerLevel * 2f);

            if (levelDifference > 5)
                baseDamage *= 1.2f;
            else if (levelDifference < -5)
                baseDamage *= 0.8f;

            return Mathf.Max(1f, baseDamage);
        }

        public int GetLevel()
        {
            return evolutionManager != null ? evolutionManager.GetLevel() : 1;
        }
    }
}
