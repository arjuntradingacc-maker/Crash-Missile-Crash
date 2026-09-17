using System.Collections.Generic;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Economy
{
    /// <summary>Owns non-currency, non-card player possessions: cosmetic skins, season trophies, etc.</summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        private readonly HashSet<string> _ownedCosmetics = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public bool OwnsCosmetic(string cosmeticKey) => !string.IsNullOrEmpty(cosmeticKey) && _ownedCosmetics.Contains(cosmeticKey);

        public void GrantCosmetic(string cosmeticKey)
        {
            if (!string.IsNullOrEmpty(cosmeticKey)) _ownedCosmetics.Add(cosmeticKey);
        }

        public IReadOnlyCollection<string> OwnedCosmetics => _ownedCosmetics;

        public void LoadState(IEnumerable<string> cosmetics)
        {
            _ownedCosmetics.Clear();
            foreach (var c in cosmetics) _ownedCosmetics.Add(c);
        }
    }
}
