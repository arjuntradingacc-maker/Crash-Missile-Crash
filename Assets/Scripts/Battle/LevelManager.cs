using CrashMissileCrash.Battle.Gates;
using CrashMissileCrash.Battle.SpecialMechanics;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Procedurally builds a level's environment (ground, side walls) and delegates gate/obstacle/
    /// enemy/base construction to their respective managers. Building everything from LevelData at
    /// runtime means new levels never require hand-authored scenes or prefabs.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public LevelData CurrentLevel { get; private set; }

        private GameObject _environmentRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public void LoadLevel(LevelData level)
        {
            CurrentLevel = level;

            if (_environmentRoot != null) Destroy(_environmentRoot);
            _environmentRoot = new GameObject("Environment");

            BattlefieldBounds.Instance.Configure(level.battlefieldLength, level.battlefieldWidth);
            BuildGround(level);

            float difficultyScale = 1f + (level.difficulty - 1) * 0.12f;

            GateManager.Instance.BuildLevel(level);
            ObstacleManager.Instance.BuildLevel(level, difficultyScale);
            EnemyManager.Instance.SpawnLevelEnemies(level, difficultyScale);
            EnemyBase.Instance.Rebuild(level, difficultyScale);

            if (CannonController.Instance != null)
            {
                var pos = BattlefieldBounds.Instance.CannonSpawnPoint;
                CannonController.Instance.transform.position = pos;
            }
        }

        private void BuildGround(LevelData level)
        {
            var ground = PrimitiveFactory.CreatePlaceholder("Ground", PlaceholderShape.Cube, EnvironmentColor(level.environmentKey),
                new Vector3(level.battlefieldWidth, 0.2f, level.battlefieldLength + 4f));
            ground.transform.SetParent(_environmentRoot.transform, false);
            ground.transform.position = new Vector3(0f, -0.1f, level.battlefieldLength * 0.5f);

            float halfWidth = level.battlefieldWidth * 0.5f;
            foreach (float sign in new[] { -1f, 1f })
            {
                var wall = PrimitiveFactory.CreatePlaceholder("LaneWall", PlaceholderShape.Cube, new Color(0.3f, 0.3f, 0.32f),
                    new Vector3(0.3f, 1.5f, level.battlefieldLength + 4f));
                wall.transform.SetParent(_environmentRoot.transform, false);
                wall.transform.position = new Vector3(sign * (halfWidth + 0.15f), 0.6f, level.battlefieldLength * 0.5f);
            }
        }

        private static Color EnvironmentColor(string environmentKey)
        {
            return environmentKey switch
            {
                "env_grassland" => new Color(0.35f, 0.65f, 0.3f),
                "env_desert" => new Color(0.85f, 0.72f, 0.4f),
                "env_snow" => new Color(0.9f, 0.93f, 0.97f),
                "env_volcano" => new Color(0.35f, 0.15f, 0.12f),
                "env_ruins" => new Color(0.55f, 0.5f, 0.4f),
                "env_cybercity" => new Color(0.12f, 0.1f, 0.22f),
                "env_alien" => new Color(0.25f, 0.5f, 0.35f),
                "env_skyislands" => new Color(0.55f, 0.75f, 0.85f),
                "env_underwater" => new Color(0.1f, 0.3f, 0.5f),
                "env_megacity" => new Color(0.2f, 0.22f, 0.28f),
                _ => new Color(0.4f, 0.55f, 0.35f)
            };
        }
    }
}
