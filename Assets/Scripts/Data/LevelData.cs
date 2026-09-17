using System;
using System.Collections.Generic;

namespace CrashMissileCrash.Data
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public string enemyId;
        public int count = 3;
        public float distanceFromStart = 20f;
        public float laneX;
        public float spawnSpacing = 1f;
    }

    [Serializable]
    public class RewardConfig
    {
        public int coins;
        public int gems;
        public int xp;
        public int stars = 3;
        public string guaranteedCardId;
        public int guaranteedCardCount;
    }

    [Serializable]
    public class LevelData
    {
        public string id;
        public string worldId;
        public int levelNumber = 1;
        public string environmentKey;     // theming key (skybox/props/materials set)
        public int startingUnits = 0;      // extra units the player begins with, beyond cannon fire
        public float battlefieldLength = 60f;
        public float battlefieldWidth = 10f;

        public List<GatePlacementData> gates = new List<GatePlacementData>();
        public List<ObstaclePlacementData> obstacles = new List<ObstaclePlacementData>();
        public List<EnemySpawnEntry> enemyWaves = new List<EnemySpawnEntry>();

        public bool isBossLevel;
        public string bossId;

        public float baseHealth = 500f;
        public int baseTowerCount = 2;
        public float baseTowerHealth = 150f;
        public float baseTowerDamage = 8f;

        public RewardConfig rewards = new RewardConfig();
        public int difficulty = 1;         // coarse difficulty tier used for matchmaking/scaling display
        public float timeLimitSeconds = 90f;
    }

    [Serializable]
    public class WorldData
    {
        public string id;
        public string displayName;
        public string environmentKey;
        public int order;
        public string musicKey;
        public int levelCount;
        public string unlockRequirementLevelId; // previous world's final level id, empty = unlocked by default
    }
}
