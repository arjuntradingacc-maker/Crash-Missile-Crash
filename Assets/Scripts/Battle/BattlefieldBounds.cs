using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Defines the playable lane volume for the current level: Z runs from the cannon (0) to the
    /// enemy base (length), X is clamped to +/-halfWidth. All gameplay positioning code reads
    /// through this instead of hardcoding battlefield dimensions.
    /// </summary>
    public class BattlefieldBounds : MonoBehaviour
    {
        public static BattlefieldBounds Instance { get; private set; }

        public float Length { get; private set; } = 60f;
        public float Width { get; private set; } = 10f;
        public float HalfWidth => Width * 0.5f;

        private void Awake()
        {
            Instance = this;
        }

        public void Configure(float length, float width)
        {
            Length = length;
            Width = width;
        }

        public float ClampX(float x) => Mathf.Clamp(x, -HalfWidth + 0.4f, HalfWidth - 0.4f);

        /// <summary>Converts a normalized lane position (-1..1) into a world X coordinate.</summary>
        public float LaneToWorldX(float laneX01) => Mathf.Clamp(laneX01, -1f, 1f) * (HalfWidth - 0.6f);

        public Vector3 CannonSpawnPoint => new Vector3(0f, 0.5f, 0.5f);
        public Vector3 BaseCorePosition => new Vector3(0f, 1f, Length);
    }
}
