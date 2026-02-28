using NUnit.Framework;
using Pokiwar.Evolution;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for PetSpriteMapper and animation data structures.
    /// </summary>
    public class PetAnimationTests
    {
        private PetSpriteMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mapper = UnityEngine.ScriptableObject.CreateInstance<PetSpriteMapper>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(mapper);
        }

        [Test]
        public void PetSpriteMapper_StartsEmpty()
        {
            Assert.AreEqual(0, mapper.PetCount);
        }

        [Test]
        public void AddOrUpdatePet_IncreasesCount()
        {
            PetSpriteData data = new PetSpriteData { petId = "001" };
            mapper.AddOrUpdatePet(data);
            Assert.AreEqual(1, mapper.PetCount);
        }

        [Test]
        public void AddOrUpdatePet_UpdatesExistingPet()
        {
            PetSpriteData data1 = new PetSpriteData { petId = "001", tier = EvolutionTier.Baby };
            PetSpriteData data2 = new PetSpriteData { petId = "001", tier = EvolutionTier.Basic };

            mapper.AddOrUpdatePet(data1);
            mapper.AddOrUpdatePet(data2);

            Assert.AreEqual(1, mapper.PetCount);
            Assert.AreEqual(EvolutionTier.Basic, mapper.GetPetById("001").tier);
        }

        [Test]
        public void GetPetById_ReturnsCorrectPet()
        {
            PetSpriteData data = new PetSpriteData { petId = "042", tier = EvolutionTier.Baby };
            mapper.AddOrUpdatePet(data);

            PetSpriteData result = mapper.GetPetById("042");
            Assert.IsNotNull(result);
            Assert.AreEqual("042", result.petId);
        }

        [Test]
        public void GetPetById_ReturnsNull_ForUnknownId()
        {
            Assert.IsNull(mapper.GetPetById("999"));
        }

        [Test]
        public void GetPetById_IsCaseInsensitive()
        {
            PetSpriteData data = new PetSpriteData { petId = "ABC" };
            mapper.AddOrUpdatePet(data);

            Assert.IsNotNull(mapper.GetPetById("abc"));
            Assert.IsNotNull(mapper.GetPetById("ABC"));
            Assert.IsNotNull(mapper.GetPetById("Abc"));
        }

        [Test]
        public void Clear_RemovesAllPets()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001" });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002" });
            mapper.Clear();

            Assert.AreEqual(0, mapper.PetCount);
        }

        [Test]
        public void GetPetsForTier_ReturnsCorrectPets()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001", tier = EvolutionTier.Baby });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002", tier = EvolutionTier.Baby });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "101", tier = EvolutionTier.Basic });

            var babyPets = mapper.GetPetsForTier(EvolutionTier.Baby);
            var basicPets = mapper.GetPetsForTier(EvolutionTier.Basic);

            Assert.AreEqual(2, babyPets.Count);
            Assert.AreEqual(1, basicPets.Count);
        }

        [Test]
        public void GetTierCount_ReturnsCorrectCount()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001", tier = EvolutionTier.Baby });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002", tier = EvolutionTier.Baby });

            Assert.AreEqual(2, mapper.GetTierCount(EvolutionTier.Baby));
            Assert.AreEqual(0, mapper.GetTierCount(EvolutionTier.Mega));
        }

        [Test]
        public void PetSpriteData_HasBreathAnimation_FalseWhenEmpty()
        {
            PetSpriteData data = new PetSpriteData { petId = "001", breathFrames = null };
            Assert.IsFalse(data.HasBreathAnimation);
        }

        [Test]
        public void PetSpriteData_HasBreathAnimation_TrueWhenFramesExist()
        {
            PetSpriteData data = new PetSpriteData
            {
                petId = "001",
                breathFrames = new UnityEngine.Sprite[1]
            };
            // Note: sprites are null but array exists - HasBreathAnimation checks Length > 0
            Assert.IsTrue(data.HasBreathAnimation);
        }

        [Test]
        public void PetSpriteData_HasLargeSprite_FalseWhenNull()
        {
            PetSpriteData data = new PetSpriteData { petId = "001", largeSprite = null };
            Assert.IsFalse(data.HasLargeSprite);
        }

        [Test]
        public void GetPetByIndex_ReturnsFirstPet_ForIndex0()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001" });
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "002" });

            PetSpriteData result = mapper.GetPetByIndex(0);
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetPetByIndex_ClampsToValidRange()
        {
            mapper.AddOrUpdatePet(new PetSpriteData { petId = "001" });

            // Index out of range should clamp
            PetSpriteData result = mapper.GetPetByIndex(999);
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetPetByIndex_ReturnsNull_WhenEmpty()
        {
            Assert.IsNull(mapper.GetPetByIndex(0));
        }
    }
}
