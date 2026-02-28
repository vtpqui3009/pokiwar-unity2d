using NUnit.Framework;
using Pokiwar.Core;

namespace Pokiwar.Tests
{
    /// <summary>
    /// EditMode tests for GameState static class.
    /// </summary>
    public class GameStateTests
    {
        [SetUp]
        public void SetUp()
        {
            GameState.ResetGame();
        }

        [TearDown]
        public void TearDown()
        {
            GameState.ResetGame();
        }

        [Test]
        public void GameState_InitializesWithCorrectDefaults()
        {
            Assert.IsFalse(GameState.IsGameActive);
            Assert.AreEqual(0, GameState.PlayerCount);
            Assert.AreEqual(0f, GameState.GameTime);
            Assert.AreEqual(GamePhase.Lobby, GameState.CurrentPhase);
        }

        [Test]
        public void StartGame_SetsIsGameActiveTrue()
        {
            GameState.StartGame(GameMode.FFA, "ABC123");
            Assert.IsTrue(GameState.IsGameActive);
        }

        [Test]
        public void StartGame_SetsCorrectGameMode()
        {
            GameState.StartGame(GameMode.Survival, "XYZ789");
            Assert.AreEqual(GameMode.Survival, GameState.CurrentGameMode);
        }

        [Test]
        public void StartGame_SetsRoomCode()
        {
            GameState.StartGame(GameMode.FFA, "ROOM01");
            Assert.AreEqual("ROOM01", GameState.RoomCode);
        }

        [Test]
        public void StartGame_SetsPhaseToPlaying()
        {
            GameState.StartGame(GameMode.FFA, "TEST");
            Assert.AreEqual(GamePhase.Playing, GameState.CurrentPhase);
        }

        [Test]
        public void StartGame_GeneratesSessionId()
        {
            GameState.StartGame(GameMode.FFA, "TEST");
            Assert.IsNotEmpty(GameState.SessionId);
        }

        [Test]
        public void EndGame_SetsIsGameActiveFalse()
        {
            GameState.StartGame(GameMode.FFA, "TEST");
            GameState.EndGame();
            Assert.IsFalse(GameState.IsGameActive);
        }

        [Test]
        public void EndGame_SetsPhaseToGameOver()
        {
            GameState.StartGame(GameMode.FFA, "TEST");
            GameState.EndGame();
            Assert.AreEqual(GamePhase.GameOver, GameState.CurrentPhase);
        }

        [Test]
        public void ResetGame_ClearsAllState()
        {
            GameState.StartGame(GameMode.Survival, "TEST");
            GameState.PlayerCount = 5;
            GameState.GameTime = 100f;
            GameState.ResetGame();

            Assert.IsFalse(GameState.IsGameActive);
            Assert.AreEqual(0, GameState.PlayerCount);
            Assert.AreEqual(0f, GameState.GameTime);
            Assert.AreEqual(GamePhase.Lobby, GameState.CurrentPhase);
        }

        [Test]
        public void GameMode_DefaultIsFFA()
        {
            Assert.AreEqual(GameMode.FFA, GameState.CurrentGameMode);
        }
    }
}
