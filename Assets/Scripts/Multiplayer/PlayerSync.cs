using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Pokiwar.Combat;
using Pokiwar.Evolution;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// Handles collision and combat synchronization between networked players.
    /// Server-authoritative damage validation with per-pair collision cooldown and knockback.
    ///
    /// Performance: uses Dictionary iteration without per-frame List allocation.
    /// </summary>
    public class PlayerSync : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float collisionCooldown = 0.5f;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private int minLevelDifferenceForDamage = 2;

        private HealthController healthController;
        private EvolutionManager evolutionManager;
        private BattleAnimationController battleAnimController;
        private Rigidbody2D rb;

        // Per-pair cooldown: key = other player's NetworkObjectId
        private readonly Dictionary<ulong, float> pairCooldowns = new Dictionary<ulong, float>();

        // Reuse list to avoid per-frame allocation
        private readonly List<ulong> expiredCooldownKeys = new List<ulong>();

        private void Awake()
        {
            healthController = GetComponent<HealthController>();
            evolutionManager = GetComponent<EvolutionManager>();
            battleAnimController = GetComponent<BattleAnimationController>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Tick down per-pair cooldowns without allocating a new list each frame
            expiredCooldownKeys.Clear();
            foreach (KeyValuePair<ulong, float> kvp in pairCooldowns)
            {
                float newTime = kvp.Value - Time.deltaTime;
                if (newTime <= 0f)
                    expiredCooldownKeys.Add(kvp.Key);
                else
                    pairCooldowns[kvp.Key] = newTime;
            }

            foreach (ulong key in expiredCooldownKeys)
                pairCooldowns.Remove(key);
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

            // Direction from this player to other (normalized)
            Vector2 toOther = (otherPlayer.transform.position - transform.position).normalized;

            if (myLevel > otherLevel + minLevelDifferenceForDamage)
            {
                float damage = CalculateDamage(myLevel, otherLevel);
                otherPlayer.TakeDamageServerRpc(damage, OwnerClientId);

                // Lunge toward target, other player recoils
                PlayLungeClientRpc(toOther);
                otherPlayer.PlayRecoilClientRpc(-toOther);
            }
            else if (otherLevel > myLevel + minLevelDifferenceForDamage)
            {
                float damage = otherPlayer.CalculateDamage(otherLevel, myLevel);
                TakeDamageServerRpc(damage, otherPlayer.OwnerClientId);

                // Other player lunges, this player recoils
                otherPlayer.PlayLungeClientRpc(-toOther);
                PlayRecoilClientRpc(toOther);
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

            // Server-side validation: clamp to reasonable range
            damage = Mathf.Clamp(damage, 0f, 500f);
            healthController?.TakeDamage(damage, null);
        }

        [ClientRpc]
        private void PlayLungeClientRpc(Vector2 direction)
        {
            if (!IsOwner) return;
            battleAnimController?.PlayLunge(direction);
        }

        [ClientRpc]
        private void PlayRecoilClientRpc(Vector2 direction)
        {
            if (!IsOwner) return;

            // Apply physics knockback
            if (rb != null)
                rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

            // Play visual recoil
            battleAnimController?.PlayRecoil(direction);
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
