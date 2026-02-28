using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Combat
{
    /// <summary>
    /// Manages player health and death/respawn logic.
    /// </summary>
    public class HealthController : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float healthRegenRate = 1f;
        [SerializeField] private float regenDelay = 3f;

        private float currentHealth;
        private float lastDamageTime;
        private bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
            isDead = false;
        }

        private void Update()
        {
            if (!isDead && Time.time - lastDamageTime >= regenDelay)
            {
                RegenerateHealth();
            }
        }

        public void TakeDamage(float damage, GameObject attacker)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            lastDamageTime = Time.time;

            if (currentHealth <= 0)
            {
                Die(attacker);
            }
        }

        private void RegenerateHealth()
        {
            if (currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
            }
        }

        private void Die(GameObject killer)
        {
            isDead = true;
            Debug.Log($"{gameObject.name} was defeated by {killer?.name}");

            CombatManager combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
            {
                combatManager.OnPlayerDefeated(gameObject, killer);
            }

            Respawn();
        }

        private void Respawn()
        {
            if (GameManager.Instance != null)
            {
                transform.position = GameManager.Instance.GetRandomPosition();
            }

            currentHealth = maxHealth;
            isDead = false;

            EvolutionManager evolution = GetComponent<EvolutionManager>();
            if (evolution != null)
            {
                int newLevel = Mathf.Max(1, evolution.GetLevel() / 2);
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        public void SetMaxHealth(float newMax)
        {
            maxHealth = newMax;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }
}
