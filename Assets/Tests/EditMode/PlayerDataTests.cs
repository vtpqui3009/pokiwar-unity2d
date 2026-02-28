using NUnit.Framework;
using Pokiwar.Core;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for PlayerData XP, leveling, and stat tracking.
    /// </summary>
    public class PlayerDataTests
    {
        private PlayerData playerData;

        [SetUp]
        public void SetUp()
        {
            playerData = new PlayerData("TestPlayer");
        }

        [Test]
        public void PlayerData_InitializesWithCorrectDefaults()
        {
            Assert.AreEqual("TestPlayer", playerData.playerName);
            Assert.AreEqual(1, playerData.level);
            Assert.AreEqual(0f, playerData.currentXP);
            Assert.AreEqual(0, playerData.killCount);
            Assert.AreEqual(0, playerData.deathCount);
            Assert.AreEqual(1, playerData.bestLevel);
        }

        [Test]
        public void AddXP_IncreasesCurrentXP()
        {
            playerData.AddXP(5f);
            Assert.AreEqual(5f, playerData.currentXP);
        }

        [Test]
        public void AddXP_TriggersLevelUp_WhenXPExceedsMax()
        {
            float maxXP = playerData.maxXP;
            playerData.AddXP(maxXP + 1f);
            Assert.AreEqual(2, playerData.level);
        }

        [Test]
        public void AddXP_DoesNothing_WhenAmountIsZero()
        {
            playerData.AddXP(0f);
            Assert.AreEqual(0f, playerData.currentXP);
            Assert.AreEqual(1, playerData.level);
        }

        [Test]
        public void AddXP_DoesNothing_WhenAmountIsNegative()
        {
            playerData.AddXP(-5f);
            Assert.AreEqual(0f, playerData.currentXP);
        }

        [Test]
        public void GetXPProgress_ReturnsZero_WhenNoXP()
        {
            Assert.AreEqual(0f, playerData.GetXPProgress());
        }

        [Test]
        public void GetXPProgress_ReturnsOne_WhenAtMaxXP()
        {
            playerData.AddXP(playerData.maxXP - 0.001f);
            Assert.That(playerData.GetXPProgress(), Is.GreaterThan(0.99f));
        }

        [Test]
        public void RecordKill_IncrementsKillCount()
        {
            playerData.RecordKill();
            playerData.RecordKill();
            Assert.AreEqual(2, playerData.killCount);
            Assert.AreEqual(2, playerData.playersDefeated);
        }

        [Test]
        public void RecordDeath_IncrementsDeathCount()
        {
            playerData.RecordDeath();
            Assert.AreEqual(1, playerData.deathCount);
        }

        [Test]
        public void RecordFoodCollected_IncrementsBothCounters()
        {
            playerData.RecordFoodCollected();
            playerData.RecordFoodCollected();
            Assert.AreEqual(2, playerData.foodCollected);
            Assert.AreEqual(2, playerData.foodCollectedThisSession);
        }

        [Test]
        public void ResetSessionStats_ClearsSessionCounters()
        {
            playerData.RecordKill();
            playerData.RecordFoodCollected();
            playerData.ResetSessionStats();

            Assert.AreEqual(0, playerData.killCount);
            Assert.AreEqual(0, playerData.foodCollectedThisSession);
        }

        [Test]
        public void BestLevel_UpdatesOnLevelUp()
        {
            playerData.AddXP(playerData.maxXP + 1f);
            Assert.AreEqual(2, playerData.bestLevel);
        }

        [Test]
        public void MultiLevelUp_WorksCorrectly()
        {
            // Add enough XP for multiple level-ups
            playerData.AddXP(1000f);
            Assert.Greater(playerData.level, 5);
        }

        [Test]
        public void TotalXPEarned_AccumulatesAcrossAdditions()
        {
            playerData.AddXP(10f);
            playerData.AddXP(20f);
            Assert.AreEqual(30f, playerData.totalXPEarned);
        }
    }
}
