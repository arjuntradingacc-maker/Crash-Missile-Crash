using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle.SpecialMechanics
{
    /// <summary>Shared helpers for zone-based obstacles (AABB overlap test against a friendly unit).</summary>
    public abstract class ObstacleBehaviour : MonoBehaviour
    {
        public string ObstacleId;
        protected float HalfWidth = 1.3f;
        protected float HalfDepth = 1.0f;

        protected bool Overlaps(Vector3 unitPosition)
        {
            return Mathf.Abs(unitPosition.x - transform.position.x) <= HalfWidth
                && Mathf.Abs(unitPosition.z - transform.position.z) <= HalfDepth;
        }

        public abstract void Tick(float deltaTime, FriendlyUnit unit);
    }

    /// <summary>Persistent zone: multiplies MoveSpeed while a unit is inside (paramA = multiplier).</summary>
    public class SpeedZoneObstacle : ObstacleBehaviour
    {
        public float SpeedMultiplier = 1.5f;

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            if (Overlaps(unit.transform.position)) unit.SpeedMultiplier = SpeedMultiplier;
        }
    }

    /// <summary>One-shot: launches a unit forward instantly the first time it enters (paramA = launch distance).</summary>
    public class LaunchPadObstacle : ObstacleBehaviour
    {
        public float LaunchDistance = 8f;

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            if (unit.TriggeredOneShotObstacles.Contains(ObstacleId)) return;
            if (!Overlaps(unit.transform.position)) return;

            unit.TriggeredOneShotObstacles.Add(ObstacleId);
            var pos = unit.transform.position;
            pos.z = Mathf.Min(BattlefieldBounds.Instance.Length, pos.z + LaunchDistance);
            unit.transform.position = pos;
        }
    }

    /// <summary>Moves back and forth laterally; units inside drift toward its current X (paramB = range, paramC = period).</summary>
    public class MovingPlatformObstacle : ObstacleBehaviour
    {
        public float TravelRange = 3f;
        public float Period = 4f;
        private float _timer;
        private float _baseX;

        private void Start() { _baseX = transform.position.x; _timer = Random.Range(0f, Period); }

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            _timer += deltaTime;
            float x = _baseX + Mathf.Sin((_timer / Mathf.Max(0.1f, Period)) * Mathf.PI * 2f) * TravelRange;
            transform.position = new Vector3(BattlefieldBounds.Instance.ClampX(x), transform.position.y, transform.position.z);

            if (Overlaps(unit.transform.position))
            {
                var pos = unit.transform.position;
                pos.x = Mathf.Lerp(pos.x, transform.position.x, deltaTime * 2f);
                unit.transform.position = pos;
            }
        }
    }

    /// <summary>Spins continuously; deals one-shot damage to any unit that ever touches it (paramA = damage).</summary>
    public class RotatingObstacleHazard : ObstacleBehaviour
    {
        public float Damage = 4f;

        private void Update() => transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            if (unit.TriggeredOneShotObstacles.Contains(ObstacleId)) return;
            if (!Overlaps(unit.transform.position)) return;
            unit.TriggeredOneShotObstacles.Add(ObstacleId);
            unit.TakeDamage(Damage, false);
        }
    }

    /// <summary>Periodically slams down; only dangerous during its "down" phase (paramA = damage, paramC = cycle seconds).</summary>
    public class CrusherObstacle : ObstacleBehaviour
    {
        public float Damage = 8f;
        public float CyclePeriod = 2.5f;
        private float _timer;

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            _timer += deltaTime;
            float phase = (_timer % Mathf.Max(0.2f, CyclePeriod)) / Mathf.Max(0.2f, CyclePeriod);
            bool isDown = phase < 0.3f;
            transform.localScale = new Vector3(transform.localScale.x, isDown ? 0.4f : 1.2f, transform.localScale.z);

            if (!isDown) return;
            if (unit.TriggeredOneShotObstacles.Contains(ObstacleId + "_" + Mathf.FloorToInt(_timer / CyclePeriod))) return;
            if (!Overlaps(unit.transform.position)) return;
            unit.TriggeredOneShotObstacles.Add(ObstacleId + "_" + Mathf.FloorToInt(_timer / CyclePeriod));
            unit.TakeDamage(Damage, false);
        }
    }

    /// <summary>Paired one-shot teleporter; moves the unit to its linked partner's position.</summary>
    public class TeleporterObstacle : ObstacleBehaviour
    {
        public TeleporterObstacle Linked;

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            if (Linked == null) return;
            if (unit.TriggeredOneShotObstacles.Contains(ObstacleId)) return;
            if (!Overlaps(unit.transform.position)) return;

            unit.TriggeredOneShotObstacles.Add(ObstacleId);
            unit.TriggeredOneShotObstacles.Add(Linked.ObstacleId);
            var pos = unit.transform.position;
            pos.x = Linked.transform.position.x;
            pos.z = Linked.transform.position.z;
            unit.transform.position = pos;
        }
    }

    /// <summary>Blocks/slows units while active (paramC = open/close cycle seconds); acts like a timed soft wall.</summary>
    public class MovingWallObstacle : ObstacleBehaviour
    {
        public float CyclePeriod = 4f;
        private float _timer;
        public bool IsBlocking { get; private set; } = true;

        public override void Tick(float deltaTime, FriendlyUnit unit)
        {
            _timer += deltaTime;
            IsBlocking = (_timer % Mathf.Max(0.2f, CyclePeriod)) < CyclePeriod * 0.5f;
            gameObject.transform.localScale = new Vector3(transform.localScale.x, IsBlocking ? 2f : 0.15f, transform.localScale.z);

            if (IsBlocking && Overlaps(unit.transform.position))
            {
                unit.SpeedMultiplier = 0f;
            }
        }
    }
}
