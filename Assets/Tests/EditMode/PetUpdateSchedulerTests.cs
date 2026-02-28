using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Pokiwar.Evolution;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for PetUpdateScheduler batch management.
    /// </summary>
    public class PetUpdateSchedulerTests
    {
        private PetSpriteMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mapper = ScriptableObject.CreateInstance<PetSpriteMapper>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(mapper);
        }

        [Test]
        public void PetUpdateBatch_IsApplied_DefaultsFalse()
        {
            PetUpdateBatch batch = new PetUpdateBatch
            {
                batchId = "batch_001",
                batchName = "Test Batch"
            };
            Assert.IsFalse(batch.isApplied);
        }

        [Test]
        public void PetUpdateBatch_IsEnabled_DefaultsTrue()
        {
            PetUpdateBatch batch = new PetUpdateBatch
            {
                batchId = "batch_001"
            };
            Assert.IsTrue(batch.isEnabled);
        }

        [Test]
        public void PetSpriteMapper_AddOrUpdatePet_IgnoresNullPetId()
        {
            PetSpriteData data = new PetSpriteData { petId = null };
            mapper.AddOrUpdatePet(data);
            Assert.AreEqual(0, mapper.PetCount);
        }

        [Test]
        public void PetSpriteMapper_AddOrUpdatePet_IgnoresEmptyPetId()
        {
            PetSpriteData data = new PetSpriteData { petId = "" };
            mapper.AddOrUpdatePet(data);
            Assert.AreEqual(0, mapper.PetCount);
        }

        [Test]
        public void PetSpriteMapper_RemovePet_RemovesCorrectPet()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001" });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002" });

            bool removed = mapper.RemovePet("001");

            Assert.IsTrue(removed);
            Assert.AreEqual(1, mapper.PetCount);
            Assert.IsNull(mapper.GetPetById("001"));
            Assert.IsNotNull(mapper.GetPetById("002"));
        }

        [Test]
        public void PetSpriteMapper_RemovePet_ReturnsFalse_WhenNotFound()
        {
            bool removed = mapper.RemovePet("999");
            Assert.IsFalse(removed);
        }

        [Test]
        public void PetSpriteMapper_GetTierCount_ReturnsCorrectCount()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001", tier = EvolutionTier.Baby });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002", tier = EvolutionTier.Baby });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "101", tier = EvolutionTier.Basic });

            Assert.AreEqual(2, mapper.GetTierCount(EvolutionTier.Baby));
            Assert.AreEqual(1, mapper.GetTierCount(EvolutionTier.Basic));
            Assert.AreEqual(0, mapper.GetTierCount(EvolutionTier.Mega));
        }

        [Test]
        public void PetSpriteMapper_GetRandomPet_ReturnsNonNull_WhenPetsExist()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001" });
            PetSpriteData result = mapper.GetRandomPet();
            Assert.IsNotNull(result);
        }

        [Test]
        public void PetSpriteMapper_GetRandomPet_ReturnsNull_WhenEmpty()
        {
            Assert.IsNull(mapper.GetRandomPet());
        }

        [Test]
        public void PetSpriteMapper_CacheDirty_RebuildOnAccess()
        {
            // Add a pet, then add another - cache should rebuild
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001", tier = EvolutionTier.Baby });
            Assert.AreEqual(1, mapper.GetTierCount(EvolutionTier.Baby));

            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002", tier = EvolutionTier.Baby });
            Assert.AreEqual(2, mapper.GetTierCount(EvolutionTier.Baby));
        }

        [Test]
        public void PetSpriteMapper_GetPetsForTier_ReturnsReadOnlyList()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001", tier = EvolutionTier.Baby });

            var list = mapper.GetPetsForTier(EvolutionTier.Baby);
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
        }
    }
}
