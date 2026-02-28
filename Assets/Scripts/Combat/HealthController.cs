using UnityEngine;
using System.Collections;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Combat
{
    /// <summary>
    /// Manages player health, shield, invincibility frames, poison DoT, and death/respawn logic.
    ///
    /// Fixes:
    /// - CombatManager is cached in Awake (no FindObjectOfType in hot path)
    /// - Level reduction on death is properly implemented via EvolutionManager.SetLevel()
    /// - BattleAnimationController integrated for death/respawn visuals
    /// </summary>
    public class HealthController : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float baseMaxHealth = 100f;
        [SerializeField] private float healthPerLevel = 10f;
        [SerializeField] private float healthRegenRate = 2f;
        [SerializeField] private float regenDelay = 3f;

        [Header("Shield Settings")]
        [SerializeField] private float spawnShieldDuration = 3f;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.5f;

        [Header("Poison")]
        [SerializeField] private float poisonDamagePerSecond = 5f;
        [SerializeField] private float poisonDuration = 5f;

        private float maxHealth;
        private float currentHealth;
        private float lastDamageTime;
        private bool isDead;
        private bool isShielded;
        private bool isInvincible;
        private bool isPoisoned;
        private float poisonTimer;

        // Cached references
        private EvolutionManager evolutionManager;
        private CombatManager combatManager;
        private BattleAnimationController battleAnimController;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsDead => isDead;
        public bool IsShielded => isShielded;
        public bool IsInvincible => isInvincible;
        public bool IsPoisoned => isPoisoned;

        // Events
        public event System.Action<float, GameObject> OnDamageTaken;
        public event System.Action<GameObject> OnDeath;
        public event System.Action OnRespawn;

        private void Awake()
        {
            evolutionManager = GetComponent<EvolutionManager>();
            battleAnimController = GetComponent<BattleAnimationController>();
            maxHealth = baseMaxHealth;
            currentHealth = maxHealth;
            isDead = false;
        }

        private void Start()
        {
            // Cache CombatManager once at start (avoid FindObjectOfType in Die())
            combatManager = FindObjectOfType<CombatManager>();
            ActivateSpawnShield();
        }

        private void Update()
        {
            if (isDead) return;

            HandleRegen();
            HandlePoison();
        }

        private void HandleRegen()
        {
            if (currentHealth < maxHealth && Time.time - lastDamageTime >= regenDelay)
            {
                currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
            }
        }

        private void HandlePoison()
        {
            if (!isPoisoned) return;

            poisonTimer -= Time.deltaTime;
            if (poisonTimer <= 0f)
            {
                isPoisoned = false;
                return;
            }

            currentHealth -= poisonDamagePerSecond * Time.deltaTime;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die(null);
            }
        }

        public void TakeDamage(float damage, GameObject attacker)
        {
            if (isDead || isInvincible || isShielded) return;
            if (damage <= 0f) return;

            currentHealth -= damage;
            lastDamageTime = Time.time;

            OnDamageTaken?.Invoke(damage, attacker);

            StartCoroutine(InvincibilityFrames());

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die(attacker);
            }
        }

        public void ApplyPoison(float duration = -1f)
        {
            isPoisoned = true;
            poisonTimer = duration > 0f ? duration : poisonDuration;
        }

        private IEnumerator InvincibilityFrames()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
        }

        private void ActivateSpawnShield()
        {
            StartCoroutine(SpawnShieldRoutine());
        }

        private IEnumerator SpawnShieldRoutine()
        {
            isShielded = true;
            yield return new WaitForSeconds(spawnShieldDuration);
            isShielded = false;
        }

        private void Die(GameObject killer)
        {
            isDead = true;
            isPoisoned = false;

            OnDeath?.Invoke(killer);

            // Use cached reference (no FindObjectOfType)
            combatManager?.OnPlayerDefeated(gameObject, killer);

            // Play death animation, then respawn
            if (battleAnimController != null)
            {
                battleAnimController.PlayDeath(() => StartCoroutine(RespawnRoutine()));
            }
            else
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            Respawn();
        }

        private void Respawn()
        {
            if (GameManager.Instance != null)
                transform.position = GameManager.Instance.GetRandomPosition();

            // Lose half level on death (FFA mode) - properly implemented
            if (evolutionManager != null)
            {
                int currentLevel = evolutionManager.GetLevel();
                int newLevel = Mathf.Max(1, currentLevel / 2);
                evolutionManager.SetLevel(newLevel);
            }

            UpdateMaxHealthForLevel();
            currentHealth = maxHealth;
            isDead = false;
            isPoisoned = false;

            ActivateSpawnShield();

            // Play respawn animation
            battleAnimController?.PlayRespawn();

            OnRespawn?.Invoke();
        }

        public void UpdateMaxHealthForLevel()
        {
            int level = evolutionManager != null ? evolutionManager.GetLevel() : 1;
            maxHealth = baseMaxHealth + (level * healthPerLevel);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        public void SetMaxHealth(float newMax)
        {
            maxHealth = Mathf.Max(1f, newMax);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public void ForceKill()
        {
            if (!isDead)
                Die(null);
        }
    }
}
