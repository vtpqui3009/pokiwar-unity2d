using UnityEngine;
using System.Collections.Generic;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Combat
{
    /// <summary>
    /// Damage type for combat calculations.
    /// </summary>
    public enum DamageType
    {
        Normal,
        Critical,
        Poison
    }

    /// <summary>
    /// Manages combat between players - attack calculations, defeat rewards, kill streaks, and combat log.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float levelBonusDamage = 2f;
        [SerializeField] private float defeatXPReward = 25f;
        [SerializeField] private float criticalChance = 0.15f;
        [SerializeField] private float criticalMultiplier = 2f;

        [Header("Effects")]
        [SerializeField] private GameObject defeatEffectPrefab;
        [SerializeField] private GameObject damageNumberPrefab;

        // Combat log (last 10 events)
        private readonly Queue<string> combatLog = new Queue<string>();
        private const int MaxLogEntries = 10;

        // Kill streak tracking per player (clientId -> streak)
        private readonly Dictionary<ulong, int> killStreaks = new Dictionary<ulong, int>();

        public event System.Action<string, string, int> OnPlayerKilled; // killer, victim, streak

        private void OnEnable()
        {
            Physics2D.IgnoreLayerCollision(6, 6, false);
        }

        public void OnPlayerDefeated(GameObject defeated, GameObject killer)
        {
            if (defeatEffectPrefab != null)
            {
                GameObject effect = Instantiate(defeatEffectPrefab, defeated.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            string killerName = "Unknown";
            string victimName = defeated.name;
            int streak = 0;

            if (killer != null)
            {
                EvolutionManager killerEvolution = killer.GetComponent<EvolutionManager>();
                if (killerEvolution != null)
                {
                    killerName = killerEvolution.GetPlayerName();
                    float xpReward = CalculateDefeatXPReward(killerEvolution.GetLevel(),
                        defeated.GetComponent<EvolutionManager>()?.GetLevel() ?? 1);
                    killerEvolution.AddXP(xpReward);
                    killerEvolution.RecordKill();

                    ShowDamageNumber(defeated.transform.position, $"+{xpReward:F0} XP", Color.green);
                }

                EvolutionManager victimEvolution = defeated.GetComponent<EvolutionManager>();
                if (victimEvolution != null)
                {
                    victimName = victimEvolution.GetPlayerName();
                    victimEvolution.RecordDeath();
                }
            }

            AddCombatLogEntry($"{killerName} defeated {victimName}");
            OnPlayerKilled?.Invoke(killerName, victimName, streak);
        }

        public float CalculateDamage(int attackerLevel, int targetLevel, DamageType damageType = DamageType.Normal)
        {
            float levelDifference = attackerLevel - targetLevel;
            float damage = baseDamage + (attackerLevel * levelBonusDamage);

            if (levelDifference > 5)
                damage *= 1.2f;
            else if (levelDifference < -5)
                damage *= 0.8f;

            if (damageType == DamageType.Critical || (damageType == DamageType.Normal && Random.value < criticalChance))
            {
                damage *= criticalMultiplier;
                ShowDamageNumber(Vector3.zero, "CRIT!", Color.red);
            }

            if (damageType == DamageType.Poison)
                damage *= 0.5f;

            return Mathf.Max(1f, damage);
        }

        private float CalculateDefeatXPReward(int killerLevel, int victimLevel)
        {
            float reward = defeatXPReward;
            int levelDiff = victimLevel - killerLevel;

            // Bonus XP for defeating higher-level players
            if (levelDiff > 0)
                reward *= 1f + (levelDiff * 0.1f);

            return Mathf.Max(defeatXPReward * 0.5f, reward);
        }

        public void ShowDamageNumber(Vector3 position, string text, Color color)
        {
            if (damageNumberPrefab == null) return;

            GameObject dmgNum = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            TextMesh textMesh = dmgNum.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
            }
            Destroy(dmgNum, 1f);
        }

        private void AddCombatLogEntry(string entry)
        {
            combatLog.Enqueue(entry);
            if (combatLog.Count > MaxLogEntries)
                combatLog.Dequeue();
        }

        public IEnumerable<string> GetCombatLog() => combatLog;

        public float GetDefeatXPReward() => defeatXPReward;
    }
}
