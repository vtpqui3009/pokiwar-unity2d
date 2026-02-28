using NUnit.Framework;
using Pokiwar.Evolution;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for SpriteDatabase tier and level calculations.
    /// </summary>
    public class SpriteDatabaseTests
    {
        [Test]
        public void GetTierForLevel_ReturnsBaby_ForLevel1To4()
        {
            Assert.AreEqual(EvolutionTier.Baby, SpriteDatabase.GetTierForLevel(1));
            Assert.AreEqual(EvolutionTier.Baby, SpriteDatabase.GetTierForLevel(4));
        }

        [Test]
        public void GetTierForLevel_ReturnsBasic_ForLevel5To14()
        {
            Assert.AreEqual(EvolutionTier.Basic, SpriteDatabase.GetTierForLevel(5));
            Assert.AreEqual(EvolutionTier.Basic, SpriteDatabase.GetTierForLevel(14));
        }

        [Test]
        public void GetTierForLevel_ReturnsStage1_ForLevel15To29()
        {
            Assert.AreEqual(EvolutionTier.Stage1, SpriteDatabase.GetTierForLevel(15));
            Assert.AreEqual(EvolutionTier.Stage1, SpriteDatabase.GetTierForLevel(29));
        }

        [Test]
        public void GetTierForLevel_ReturnsStage2_ForLevel30To49()
        {
            Assert.AreEqual(EvolutionTier.Stage2, SpriteDatabase.GetTierForLevel(30));
            Assert.AreEqual(EvolutionTier.Stage2, SpriteDatabase.GetTierForLevel(49));
        }

        [Test]
        public void GetTierForLevel_ReturnsMega_ForLevel50Plus()
        {
            Assert.AreEqual(EvolutionTier.Mega, SpriteDatabase.GetTierForLevel(50));
            Assert.AreEqual(EvolutionTier.Mega, SpriteDatabase.GetTierForLevel(100));
        }

        [Test]
        public void GetNextEvolutionLevel_ReturnsCorrectThresholds()
        {
            Assert.AreEqual(5, SpriteDatabase.GetNextEvolutionLevel(1));
            Assert.AreEqual(15, SpriteDatabase.GetNextEvolutionLevel(5));
            Assert.AreEqual(30, SpriteDatabase.GetNextEvolutionLevel(15));
            Assert.AreEqual(50, SpriteDatabase.GetNextEvolutionLevel(30));
            Assert.AreEqual(-1, SpriteDatabase.GetNextEvolutionLevel(50));
        }

        [Test]
        public void GetNextEvolutionLevel_ReturnsNegativeOne_AtMaxTier()
        {
            Assert.AreEqual(-1, SpriteDatabase.GetNextEvolutionLevel(100));
        }

        [Test]
        public void TierValues_AreOrdered()
        {
            Assert.Less((int)EvolutionTier.Baby, (int)EvolutionTier.Basic);
            Assert.Less((int)EvolutionTier.Basic, (int)EvolutionTier.Stage1);
            Assert.Less((int)EvolutionTier.Stage1, (int)EvolutionTier.Stage2);
            Assert.Less((int)EvolutionTier.Stage2, (int)EvolutionTier.Mega);
        }
    }
}
