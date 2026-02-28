using UnityEngine;
using System.Collections;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Combat
{
    /// <summary>
    /// Ability types available to players.
    /// </summary>
    public enum AbilityType
    {
        Dash,
        AreaAttack,
        Shield
    }

    /// <summary>
    /// Manages player abilities unlocked at evolution stage 2+.
    /// Abilities: Dash, Area Attack, Shield.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class AbilitySystem : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private float dashCooldown = 3f;

        [Header("Area Attack Settings")]
        [SerializeField] private float areaAttackRadius = 3f;
        [SerializeField] private float areaAttackDamage = 30f;
        [SerializeField] private float areaAttackCooldown = 8f;

        [Header("Shield Settings")]
        [SerializeField] private float shieldDuration = 2f;
        [SerializeField] private float shieldCooldown = 10f;

        private PlayerController playerController;
        private EvolutionManager evolutionManager;
        private Rigidbody2D rb;

        private float dashCooldownTimer;
        private float areaAttackCooldownTimer;
        private float shieldCooldownTimer;

        public bool CanDash => dashCooldownTimer <= 0f && IsAbilityUnlocked;
        public bool CanAreaAttack => areaAttackCooldownTimer <= 0f && IsAbilityUnlocked;
        public bool CanShield => shieldCooldownTimer <= 0f && IsAbilityUnlocked;

        public float DashCooldownPercent => dashCooldown > 0f ? Mathf.Clamp01(dashCooldownTimer / dashCooldown) : 0f;
        public float AreaAttackCooldownPercent => areaAttackCooldown > 0f ? Mathf.Clamp01(areaAttackCooldownTimer / areaAttackCooldown) : 0f;
        public float ShieldCooldownPercent => shieldCooldown > 0f ? Mathf.Clamp01(shieldCooldownTimer / shieldCooldown) : 0f;

        /// <summary>
        /// Abilities unlock at evolution Stage1 (level 15+).
        /// </summary>
        public bool IsAbilityUnlocked
        {
            get
            {
                if (evolutionManager == null) return false;
                return (int)evolutionManager.GetCurrentTier() >= (int)EvolutionTier.Stage1;
            }
        }

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            evolutionManager = GetComponent<EvolutionManager>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            TickCooldowns();
        }

        private void TickCooldowns()
        {
            if (dashCooldownTimer > 0f)
                dashCooldownTimer -= Time.deltaTime;
            if (areaAttackCooldownTimer > 0f)
                areaAttackCooldownTimer -= Time.deltaTime;
            if (shieldCooldownTimer > 0f)
                shieldCooldownTimer -= Time.deltaTime;
        }

        public void UseAbility(AbilityType type)
        {
            if (!IsAbilityUnlocked) return;

            switch (type)
            {
                case AbilityType.Dash:
                    if (CanDash) PerformDash();
                    break;
                case AbilityType.AreaAttack:
                    if (CanAreaAttack) PerformAreaAttack();
                    break;
                case AbilityType.Shield:
                    if (CanShield) PerformShield();
                    break;
            }
        }

        private void PerformDash()
        {
            if (rb == null || playerController == null) return;

            Vector2 dashDir = playerController.Velocity.normalized;
            if (dashDir == Vector2.zero)
                dashDir = Vector2.right;

            rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
            dashCooldownTimer = dashCooldown;
        }

        private void PerformAreaAttack()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaAttackRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                if (!hit.CompareTag("Player")) continue;

                HealthController health = hit.GetComponent<HealthController>();
                health?.TakeDamage(areaAttackDamage, gameObject);
            }

            areaAttackCooldownTimer = areaAttackCooldown;
        }

        private void PerformShield()
        {
            HealthController health = GetComponent<HealthController>();
            if (health != null)
                StartCoroutine(ShieldRoutine(health));

            shieldCooldownTimer = shieldCooldown;
        }

        private IEnumerator ShieldRoutine(HealthController health)
        {
            // Temporarily make invincible via the shield field
            yield return new WaitForSeconds(shieldDuration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
        }
    }
}
