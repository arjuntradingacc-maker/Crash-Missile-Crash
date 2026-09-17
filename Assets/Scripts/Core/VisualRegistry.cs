using System.Collections.Generic;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Core
{
    [System.Serializable]
    public class VisualOverride
    {
        public string visualKey;
        public GameObject prefab;
    }

    /// <summary>
    /// Resolves a content visualKey (e.g. "unit_recruit") to a template GameObject to instantiate.
    /// Designers can wire real art prefabs into `overrides` in the inspector; any visualKey without
    /// an override falls back to a generated primitive so every system is playable without art.
    /// This is the single seam between data-driven gameplay and art content.
    /// </summary>
    public class VisualRegistry : MonoBehaviour
    {
        public static VisualRegistry Instance { get; private set; }

        [SerializeField] private List<VisualOverride> overrides = new List<VisualOverride>();

        private readonly Dictionary<string, GameObject> _resolved = new Dictionary<string, GameObject>();
        private Transform _templateRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);

            _templateRoot = new GameObject("_VisualTemplates").transform;
            _templateRoot.SetParent(transform, false);
            _templateRoot.gameObject.SetActive(false);

            foreach (var entry in overrides)
            {
                if (entry.prefab != null && !string.IsNullOrEmpty(entry.visualKey))
                {
                    _resolved[entry.visualKey] = entry.prefab;
                }
            }
        }

        /// <summary>Gets (creating if necessary) a template GameObject usable as an ObjectPool source.</summary>
        public GameObject GetOrCreateTemplate(string visualKey, PlaceholderShape fallbackShape, Color fallbackColor, Vector3 fallbackScale)
        {
            if (string.IsNullOrEmpty(visualKey)) visualKey = "unknown";

            if (_resolved.TryGetValue(visualKey, out var existing) && existing != null)
            {
                return existing;
            }

            var placeholder = PrimitiveFactory.CreatePlaceholder(visualKey, fallbackShape, fallbackColor, fallbackScale);
            placeholder.transform.SetParent(_templateRoot, false);
            _resolved[visualKey] = placeholder;
            return placeholder;
        }
    }
}
