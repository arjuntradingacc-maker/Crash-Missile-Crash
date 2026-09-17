using UnityEngine;

namespace CrashMissileCrash.Data
{
    /// <summary>
    /// Loads JSON content from Resources/Data. All game content (units, enemies, gates,
    /// levels, worlds, cannons, champions, cards, missions, events, seasons, shop offers)
    /// ships as plain JSON so designers can add hundreds of levels/items without touching code.
    /// Files placed in Resources are bundled at build time and load synchronously offline,
    /// which keeps campaign gameplay fully playable without network access.
    /// </summary>
    public static class DataLoader
    {
        public static T LoadJson<T>(string resourcePathNoExtension) where T : class, new()
        {
            var textAsset = Resources.Load<TextAsset>(resourcePathNoExtension);
            if (textAsset == null)
            {
                Debug.LogWarning($"[DataLoader] Missing data file: Resources/{resourcePathNoExtension}.json");
                return new T();
            }

            try
            {
                return JsonUtility.FromJson<T>(textAsset.text);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataLoader] Failed parsing {resourcePathNoExtension}: {e.Message}");
                return new T();
            }
        }

        /// <summary>Loads a single LevelData asset by its resource path, e.g. "Data/Levels/world1_level1".</summary>
        public static LevelData LoadLevel(string levelResourcePath)
        {
            var textAsset = Resources.Load<TextAsset>(levelResourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"[DataLoader] Missing level file: Resources/{levelResourcePath}.json");
                return null;
            }
            return JsonUtility.FromJson<LevelData>(textAsset.text);
        }
    }
}
