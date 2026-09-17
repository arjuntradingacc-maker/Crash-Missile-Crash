using System.Collections.Generic;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle.SpecialMechanics
{
    /// <summary>Builds and drives every non-gate obstacle placed in the current level.</summary>
    public class ObstacleManager : MonoBehaviour
    {
        public static ObstacleManager Instance { get; private set; }

        private readonly List<ObstacleBehaviour> _behaviours = new List<ObstacleBehaviour>();
        private readonly List<DestructibleBarrier> _barriers = new List<DestructibleBarrier>();
        private readonly List<GameObject> _spawned = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public void BuildLevel(LevelData level, float difficultyScale)
        {
            ClearAll();
            var bounds = BattlefieldBounds.Instance;
            var teleporterLookup = new Dictionary<string, TeleporterObstacle>();

            foreach (var placement in level.obstacles)
            {
                float worldX = bounds.ClampX(bounds.LaneToWorldX(placement.laneX));
                Vector3 pos = new Vector3(worldX, 0.6f, placement.distanceFromStart);
                GameObject go;

                switch (placement.type)
                {
                    case ObstacleType.SpeedZone:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.2f, 0.9f, 0.9f, 0.5f), new Vector3(2.6f, 0.1f, 2f));
                        var speedZone = go.AddComponent<SpeedZoneObstacle>();
                        speedZone.ObstacleId = placement.id;
                        speedZone.SpeedMultiplier = placement.paramA;
                        _behaviours.Add(speedZone);
                        break;

                    case ObstacleType.LaunchPad:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cylinder, new Color(1f, 0.6f, 0.1f), new Vector3(1.8f, 0.3f, 1.8f));
                        var launchPad = go.AddComponent<LaunchPadObstacle>();
                        launchPad.ObstacleId = placement.id;
                        launchPad.LaunchDistance = placement.paramA;
                        _behaviours.Add(launchPad);
                        break;

                    case ObstacleType.MovingPlatform:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.5f, 0.5f, 0.55f), new Vector3(2.4f, 0.3f, 2f));
                        var platform = go.AddComponent<MovingPlatformObstacle>();
                        platform.ObstacleId = placement.id;
                        platform.TravelRange = placement.paramB;
                        platform.Period = placement.paramC;
                        _behaviours.Add(platform);
                        break;

                    case ObstacleType.RotatingObstacle:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.8f, 0.2f, 0.2f), new Vector3(2.2f, 0.3f, 0.4f));
                        var rotator = go.AddComponent<RotatingObstacleHazard>();
                        rotator.ObstacleId = placement.id;
                        rotator.Damage = placement.paramA * difficultyScale;
                        _behaviours.Add(rotator);
                        break;

                    case ObstacleType.Crusher:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.6f, 0.6f, 0.65f), new Vector3(2f, 1.2f, 2f));
                        var crusher = go.AddComponent<CrusherObstacle>();
                        crusher.ObstacleId = placement.id;
                        crusher.Damage = placement.paramA * difficultyScale;
                        crusher.CyclePeriod = Mathf.Max(0.5f, placement.paramC);
                        _behaviours.Add(crusher);
                        break;

                    case ObstacleType.Teleporter:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cylinder, new Color(0.5f, 0.2f, 0.9f), new Vector3(1.5f, 1.5f, 1.5f));
                        var teleporter = go.AddComponent<TeleporterObstacle>();
                        teleporter.ObstacleId = placement.id;
                        teleporterLookup[placement.id] = teleporter;
                        _behaviours.Add(teleporter);
                        break;

                    case ObstacleType.DestructibleBarrier:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.6f, 0.45f, 0.3f), new Vector3(2.4f, 1.6f, 0.6f));
                        var barrier = go.AddComponent<DestructibleBarrier>();
                        barrier.Initialize(Mathf.Max(1f, placement.health) * difficultyScale);
                        _barriers.Add(barrier);
                        break;

                    case ObstacleType.MovingWall:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.4f, 0.4f, 0.5f), new Vector3(2.6f, 2f, 0.4f));
                        var wall = go.AddComponent<MovingWallObstacle>();
                        wall.ObstacleId = placement.id;
                        wall.CyclePeriod = Mathf.Max(0.5f, placement.paramC);
                        _behaviours.Add(wall);
                        break;

                    case ObstacleType.EnemyCannon:
                    case ObstacleType.DefensiveTower:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cylinder, new Color(0.3f, 0.1f, 0.35f), new Vector3(0.9f, 1.4f, 0.9f));
                        var tower = go.AddComponent<BaseTower>();
                        var stats = new CombatStatsRuntime
                        {
                            MaxHealth = Mathf.Max(1f, placement.health) * difficultyScale,
                            Health = Mathf.Max(1f, placement.health) * difficultyScale,
                            Damage = placement.paramA * difficultyScale,
                            Defense = 1f,
                            AttackSpeed = 0.7f,
                            MoveSpeed = 0f,
                            Range = 4.5f,
                            CritChance = 0f,
                            CritMultiplier = 1f
                        };
                        tower.ResetForSpawn(stats, pos);
                        tower.DetectionRange = 6f;
                        EnemyBase.Instance.RegisterExternalTower(tower);
                        break;

                    case ObstacleType.Bridge:
                    default:
                        go = PrimitiveFactory.CreatePlaceholder(placement.id, PlaceholderShape.Cube, new Color(0.55f, 0.4f, 0.25f), new Vector3(2.6f, 0.2f, 3f));
                        break;
                }

                go.transform.position = pos;
                _spawned.Add(go);
            }

            foreach (var placement in level.obstacles)
            {
                if (placement.type != ObstacleType.Teleporter || string.IsNullOrEmpty(placement.linkedTeleporterId)) continue;
                if (teleporterLookup.TryGetValue(placement.id, out var a) && teleporterLookup.TryGetValue(placement.linkedTeleporterId, out var b))
                {
                    a.Linked = b;
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (CrowdManager.Instance == null) return;
            foreach (var behaviour in _behaviours)
            {
                foreach (var unit in CrowdManager.Instance.ActiveUnits)
                {
                    if (unit.IsAlive) behaviour.Tick(deltaTime, unit);
                }
            }
        }

        /// <summary>Extra ICombatTarget entries (destructible barriers) merged into CombatSystem's enemy grid.</summary>
        public IEnumerable<ICombatTarget> GetDamageableTargets()
        {
            foreach (var barrier in _barriers)
            {
                if (barrier != null && barrier.IsAlive) yield return barrier;
            }
        }

        public void ClearAll()
        {
            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();
            _behaviours.Clear();
            _barriers.Clear();
        }
    }
}
