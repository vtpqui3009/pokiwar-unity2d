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

        // Enhanced fields
        public int killCount;
        public float totalXPEarned;
        public int deathCount;
        public int bestLevel;
        public float sessionStartTime;
        public int foodCollectedThisSession;

        public PlayerData(string name)
        {
            playerName = name;
            level = 1;
            currentXP = 0f;
            maxXP = 10f;
            spriteId = 1;
            foodCollected = 0;
            playersDefeated = 0;
            killCount = 0;
            totalXPEarned = 0f;
            deathCount = 0;
            bestLevel = 1;
            sessionStartTime = Time.time;
            foodCollectedThisSession = 0;
        }

        public void AddXP(float amount)
        {
            if (amount <= 0f) return;

            currentXP += amount;
            totalXPEarned += amount;

            while (currentXP >= maxXP)
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

            if (level > bestLevel)
                bestLevel = level;
        }

        private float CalculateMaxXP(int lvl)
        {
            return 10f + (lvl * 5f);
        }

        public float GetXPProgress()
        {
            if (maxXP <= 0f) return 0f;
            return Mathf.Clamp01(currentXP / maxXP);
        }

        public void RecordKill()
        {
            killCount++;
            playersDefeated++;
        }

        public void RecordDeath()
        {
            deathCount++;
        }

        public void RecordFoodCollected()
        {
            foodCollected++;
            foodCollectedThisSession++;
        }

        public void ResetSessionStats()
        {
            killCount = 0;
            foodCollectedThisSession = 0;
            sessionStartTime = Time.time;
        }

        public float GetSessionDuration()
        {
            return Time.time - sessionStartTime;
        }
    }
}
