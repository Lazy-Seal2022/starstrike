using System;

namespace StarStrike.Core
{
    [Serializable]
    public struct RunSummary
    {
        public bool isVictory;
        public float timeSurvived;
        public int gemsCollected;
        public int asteroidsMined;
        public int enemiesDestroyed;
        public bool bossDefeated;
        public string finalShipId;
        public int finalScore;

        public static int CalculateScore(int gems, int asteroids, int enemies, bool bossKilled, float time)
        {
            int score = (gems * 15) + (asteroids * 10) + (enemies * 50);
            if (bossKilled) score += 1000;
            score += Mathf.Max(0, (int)(time * 2f));
            return score;
        }

        private static class Mathf
        {
            public static int Max(int a, int b) => a > b ? a : b;
        }
    }
}
