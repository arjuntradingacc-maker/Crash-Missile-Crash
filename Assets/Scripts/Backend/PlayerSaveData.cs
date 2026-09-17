using System;
using System.Collections.Generic;
using CrashMissileCrash.Base;
using CrashMissileCrash.Data;

namespace CrashMissileCrash.Backend
{
    // JsonUtility can't serialize Dictionary<K,V> directly, so every dictionary-shaped piece of
    // save state is flattened to a list of key/value pairs for storage and rebuilt into a
    // Dictionary when loaded.
    [Serializable] public class StringIntPair { public string key; public int value; }
    [Serializable] public class CurrencyIntPair { public CurrencyType key; public int value; }
    [Serializable] public class BuildingIntPair { public BuildingType key; public int value; }

    [Serializable]
    public class PlayerSaveData
    {
        public int saveVersion = 1;
        public string lastSavedUtcIso;
        public string currentLevelId = "tutorial_level";

        public List<CurrencyIntPair> currencies = new List<CurrencyIntPair>();
        public List<StringIntPair> cardCounts = new List<StringIntPair>();
        public List<StringIntPair> cardLevels = new List<StringIntPair>();

        public List<string> unlockedCannons = new List<string>();
        public string equippedCannon = "scrapcannon_mk1";

        public List<string> unlockedChampions = new List<string>();
        public string deployedChampion = "ironclad";

        public int xp;
        public int playerLevel = 1;

        public List<StringIntPair> missionProgress = new List<StringIntPair>();
        public List<string> missionClaimed = new List<string>();

        public int trophies;

        public int seasonPoints;
        public bool hasPremiumPass;

        public List<BuildingIntPair> buildingLevels = new List<BuildingIntPair>();
        public string shieldExpiresAtUtcIso;

        public List<StringIntPair> eventScores = new List<StringIntPair>();
        public List<string> ownedCosmetics = new List<string>();
    }
}
