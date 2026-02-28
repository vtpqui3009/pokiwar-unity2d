using UnityEngine;

namespace Pokiwar.Core
{
    /// <summary>
    /// Serializable player data for stats and persistence.
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int level;
        public float currentXP;
        public float maxXP;
        public int spriteId;
        public int foodCollected;
        public int playersDefeated;

        public PlayerData(string name)
        {
            playerName = name;
            level = 1;
            currentXP = 0f;
            maxXP = 10f;
            spriteId = 1;
            foodCollected = 0;
            playersDefeated = 0;
        }

        public void AddXP(float amount)
        {
            currentXP += amount;
            if (currentXP >= maxXP)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentXP -= maxXP;
            level++;
            spriteId = level;
            maxXP = CalculateMaxXP(level);
        }

        private float CalculateMaxXP(int level)
        {
            return 10f + (level * 5f);
        }

        public float GetXPProgress()
        {
            return currentXP / maxXP;
        }
    }
}
