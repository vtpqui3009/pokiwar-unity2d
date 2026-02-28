using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Pokiwar.Combat;
using Pokiwar.Multiplayer;

namespace Pokiwar.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for combat damage calculations and PlayerSync.
    /// </summary>
    public class CombatTests
    {
        private GameObject combatObj;
        private CombatManager combatManager;

        [SetUp]
        public void SetUp()
        {
            combatObj = new GameObject("CombatManager");
            combatManager = combatObj.AddComponent<CombatManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (combatObj != null)
                Object.DestroyImmediate(combatObj);
        }

        [UnityTest]
        public IEnumerator CalculateDamage_ReturnsPositiveValue()
        {
            yield return null;
            float damage = combatManager.CalculateDamage(5, 3);
            Assert.Greater(damage, 0f);
        }

        [UnityTest]
        public IEnumerator CalculateDamage_HigherLevelDealsMoreDamage()
        {
            yield return null;
            float damageHighLevel = combatManager.CalculateDamage(20, 5);
            float damageLowLevel = combatManager.CalculateDamage(5, 5);
            Assert.Greater(damageHighLevel, damageLowLevel);
        }

        [UnityTest]
        public IEnumerator CalculateDamage_NeverBelowOne()
        {
            yield return null;
            float damage = combatManager.CalculateDamage(1, 100);
            Assert.GreaterOrEqual(damage, 1f);
        }

        [UnityTest]
        public IEnumerator CombatLog_StartsEmpty()
        {
            yield return null;
            int count = 0;
            foreach (string _ in combatManager.GetCombatLog())
                count++;
            Assert.AreEqual(0, count);
        }

        [UnityTest]
        public IEnumerator GetDefeatXPReward_ReturnsPositiveValue()
        {
            yield return null;
            Assert.Greater(combatManager.GetDefeatXPReward(), 0f);
        }

        [UnityTest]
        public IEnumerator PlayerSync_CalculateDamage_ReturnsPositiveValue()
        {
            yield return null;

            GameObject syncObj = new GameObject("PlayerSync");
            syncObj.AddComponent<Rigidbody2D>();
            PlayerSync sync = syncObj.AddComponent<PlayerSync>();

            float damage = sync.CalculateDamage(10, 5);
            Assert.Greater(damage, 0f);

            Object.DestroyImmediate(syncObj);
        }

        [UnityTest]
        public IEnumerator PlayerSync_CalculateDamage_ScalesWithLevel()
        {
            yield return null;

            GameObject syncObj = new GameObject("PlayerSync");
            syncObj.AddComponent<Rigidbody2D>();
            PlayerSync sync = syncObj.AddComponent<PlayerSync>();

            float damageLow = sync.CalculateDamage(1, 1);
            float damageHigh = sync.CalculateDamage(50, 1);
            Assert.Greater(damageHigh, damageLow);

            Object.DestroyImmediate(syncObj);
        }
    }
}
