using UnityEngine;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Combat
{
    /// <summary>
    /// Manages combat between players - attack calculations and defeat rewards.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float levelBonusDamage = 2f;
        [SerializeField] private float defeatXPReward = 25f;
        [SerializeField] private float invincibilityDuration = 1f;

        [Header("Effects")]
        [SerializeField] private GameObject defeatEffectPrefab;
        [SerializeField] private GameObject damageNumberPrefab;

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

            if (killer != null)
            {
                EvolutionManager killerEvolution = killer.GetComponent<EvolutionManager>();
                if (killerEvolution != null)
                {
                    killerEvolution.AddXP(defeatXPReward);
                }

                ShowDamageNumber(defeated.transform.position, defeatXPReward.ToString() + " XP", Color.green);
            }
        }

        public float CalculateDamage(int attackerLevel, int targetLevel)
        {
            float levelDifference = attackerLevel - targetLevel;
            float damage = baseDamage + (attackerLevel * levelBonusDamage);

            if (levelDifference > 5)
                damage *= 1.2f;
            else if (levelDifference < -5)
                damage *= 0.8f;

            return damage;
        }

        public void ShowDamageNumber(Vector3 position, string text, Color color)
        {
            if (damageNumberPrefab != null)
            {
                GameObject dmgNum = Instantiate(damageNumberPrefab, position, Quaternion.identity);
                TextMesh textMesh = dmgNum.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    textMesh.text = text;
                    textMesh.color = color;
                }
                Destroy(dmgNum, 1f);
            }
        }

        public float GetDefeatXPReward()
        {
            return defeatXPReward;
        }
    }
}
