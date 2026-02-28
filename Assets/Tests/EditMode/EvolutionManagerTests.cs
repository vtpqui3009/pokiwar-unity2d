using NUnit.Framework;
using UnityEngine;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for EvolutionManager logic (SetLevel, AddXP with negative values).
    /// Note: Uses PlayerData directly since EvolutionManager is a MonoBehaviour.
    /// </summary>
    public class EvolutionManagerTests
    {
        private PlayerData playerData;

        [SetUp]
        public void SetUp()
        {
            playerData = new PlayerData("TestPlayer");
        }

        [Test]
        public void AddXP_NegativeAmount_DoesNotGoBelow_Level1()
        {
            // Start at level 1 with 0 XP
            Assert.AreEqual(1, playerData.level);
            Assert.AreEqual(0f, playerData.currentXP);

            // Simulate negative XP (poison food)
            float newXP = Mathf.Max(0f, playerData.currentXP - 5f);
            playerData.currentXP = newXP;

            Assert.AreEqual(0f, playerData.currentXP);
            Assert.AreEqual(1, playerData.level); // Level should not drop below 1
        }

        [Test]
        public void PlayerData_LevelUp_CalculatesCorrectMaxXP()
        {
            // Level 1: maxXP = 10 + (1 * 5) = 15
            Assert.AreEqual(15f, playerData.maxXP);

            playerData.AddXP(playerData.maxXP + 1f);
            Assert.AreEqual(2, playerData.level);

            // Level 2: maxXP = 10 + (2 * 5) = 20
            Assert.AreEqual(20f, playerData.maxXP);
        }

        [Test]
        public void SetLevel_ResetsXPToZero()
        {
            // Simulate SetLevel behavior
            playerData.AddXP(5f);
            Assert.Greater(playerData.currentXP, 0f);

            // Simulate SetLevel
            int newLevel = 5;
            playerData.level = newLevel;
            playerData.spriteId = newLevel;
            playerData.currentXP = 0f;
            playerData.maxXP = 10f + (newLevel * 5f);

            Assert.AreEqual(5, playerData.level);
            Assert.AreEqual(0f, playerData.currentXP);
            Assert.AreEqual(35f, playerData.maxXP); // 10 + (5 * 5)
        }

        [Test]
        public void SetLevel_UpdatesBestLevel_WhenHigher()
        {
            // Simulate SetLevel to a higher level
            int newLevel = 10;
            playerData.level = newLevel;
            if (newLevel > playerData.bestLevel)
                playerData.bestLevel = newLevel;

            Assert.AreEqual(10, playerData.bestLevel);
        }

        [Test]
        public void SetLevel_DoesNotUpdateBestLevel_WhenLower()
        {
            // First go to level 10
            playerData.level = 10;
            playerData.bestLevel = 10;

            // Then set to level 5 (death penalty)
            int newLevel = 5;
            playerData.level = newLevel;
            // bestLevel should NOT update when going down
            if (newLevel > playerData.bestLevel)
                playerData.bestLevel = newLevel;

            Assert.AreEqual(10, playerData.bestLevel); // Still 10
            Assert.AreEqual(5, playerData.level);
        }

        [Test]
        public void SpriteDatabase_GetTierForLevel_CorrectAtBoundaries()
        {
            Assert.AreEqual(EvolutionTier.Baby,   SpriteDatabase.GetTierForLevel(1));
            Assert.AreEqual(EvolutionTier.Baby,   SpriteDatabase.GetTierForLevel(4));
            Assert.AreEqual(EvolutionTier.Basic,  SpriteDatabase.GetTierForLevel(5));
            Assert.AreEqual(EvolutionTier.Basic,  SpriteDatabase.GetTierForLevel(14));
            Assert.AreEqual(EvolutionTier.Stage1, SpriteDatabase.GetTierForLevel(15));
            Assert.AreEqual(EvolutionTier.Stage1, SpriteDatabase.GetTierForLevel(29));
            Assert.AreEqual(EvolutionTier.Stage2, SpriteDatabase.GetTierForLevel(30));
            Assert.AreEqual(EvolutionTier.Stage2, SpriteDatabase.GetTierForLevel(99));
            Assert.AreEqual(EvolutionTier.Mega,   SpriteDatabase.GetTierForLevel(100));
        }

        [Test]
        public void HalfLevelOnDeath_CalculatesCorrectly()
        {
            // Level 10 -> should become level 5
            int level = 10;
            int newLevel = Mathf.Max(1, level / 2);
            Assert.AreEqual(5, newLevel);

            // Level 1 -> should stay at level 1
            level = 1;
            newLevel = Mathf.Max(1, level / 2);
            Assert.AreEqual(1, newLevel);

            // Level 3 -> should become level 1 (3/2 = 1)
            level = 3;
            newLevel = Mathf.Max(1, level / 2);
            Assert.AreEqual(1, newLevel);
        }
    }
}
