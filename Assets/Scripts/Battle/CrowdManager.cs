using System.Collections.Generic;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Owns the friendly crowd: pooling, spawning, gate-driven multiplication, and per-tick
    /// movement. All friendly units in a battle share one active unit type at a time (whatever
    /// the equipped cannon currently fires), matching the "crowd multiplication" genre convention
    /// and keeping pooling/rendering simple - GateManager just tells this class how many units to
    /// add or remove.
    /// </summary>
    public class CrowdManager : MonoBehaviour
    {
        public static CrowdManager Instance { get; private set; }

        private readonly Dictionary<string, ObjectPool<FriendlyUnit>> _poolsByUnitId = new Dictionary<string, ObjectPool<FriendlyUnit>>();
        private readonly List<FriendlyUnit> _active = new List<FriendlyUnit>();
        private readonly List<(FriendlyUnit unit, float releaseAt)> _pendingRelease = new List<(FriendlyUnit, float)>();

        public string ActiveUnitDataId { get; private set; }
        public int UnitLevel { get; private set; } = 1;
        public int Count => _active.Count;
        public IReadOnlyList<FriendlyUnit> ActiveUnits => _active;

        private float _battlefieldLength = 60f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public void ConfigureForLevel(float battlefieldLength, string startingUnitDataId, int unitLevel)
        {
            _battlefieldLength = battlefieldLength;
            ActiveUnitDataId = startingUnitDataId;
            UnitLevel = unitLevel;
            ReleaseAll();
        }

        /// <summary>Called by CannonFireController when a cannon is equipped/switched.</summary>
        public void SetActiveUnitType(string unitDataId)
        {
            if (!string.IsNullOrEmpty(unitDataId)) ActiveUnitDataId = unitDataId;
        }

        private ObjectPool<FriendlyUnit> GetOrCreatePool(UnitData data)
        {
            if (_poolsByUnitId.TryGetValue(data.id, out var pool)) return pool;

            var template = VisualRegistry.Instance.GetOrCreateTemplate(
                data.visualKey, PlaceholderShape.Capsule, PrimitiveFactory.ColorForRarity((int)data.rarity), new Vector3(0.5f, 0.5f, 0.5f));
            var prefabUnit = template.GetComponent<FriendlyUnit>();
            if (prefabUnit == null) prefabUnit = template.AddComponent<FriendlyUnit>();

            pool = new ObjectPool<FriendlyUnit>(prefabUnit, transform, 32);
            _poolsByUnitId[data.id] = pool;
            return pool;
        }

        public FriendlyUnit SpawnUnit(Vector3 position)
        {
            if (string.IsNullOrEmpty(ActiveUnitDataId)) return null;
            if (!GameDatabase.Instance.Units.TryGetValue(ActiveUnitDataId, out var data)) return null;

            var pool = GetOrCreatePool(data);
            var unit = pool.Get(position, Quaternion.identity);
            var stats = CombatStatsRuntime.FromData(data.baseStats, data.perLevelGrowth, UnitLevel);
            unit.ResetForSpawn(stats, position);
            unit.UnitDataId = data.id;
            unit.Level = UnitLevel;
            unit.VisualKey = data.visualKey;
            _active.Add(unit);
            EventBus.Publish(new CrowdCountChangedEvent(Count));
            return unit;
        }

        public void SpawnMany(int count, Vector3 originPosition, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                float offsetX = Random.Range(-spread, spread);
                var pos = originPosition + new Vector3(BattlefieldBounds.Instance.ClampX(offsetX), 0f, 0f);
                SpawnUnit(pos);
            }
        }

        /// <summary>Removes N units from the back of the crowd (used by subtract/divide gates).</summary>
        public void RemoveUnits(int count)
        {
            for (int i = 0; i < count && _active.Count > 0; i++)
            {
                var unit = _active[_active.Count - 1];
                _active.RemoveAt(_active.Count - 1);
                ReleaseUnit(unit);
            }
            EventBus.Publish(new CrowdCountChangedEvent(Count));
        }

        /// <summary>Applies a gate's math to the current crowd count and spawns/removes the delta.</summary>
        public void ApplyGate(GateDefinitionData gate, Vector3 gatePosition)
        {
            int before = Count;
            int after = before;
            switch (gate.operation)
            {
                case GateOperation.Multiply: after = Mathf.RoundToInt(before * gate.value); break;
                case GateOperation.Add: after = before + Mathf.RoundToInt(gate.value); break;
                case GateOperation.Subtract: after = Mathf.Max(0, before - Mathf.RoundToInt(gate.value)); break;
                case GateOperation.Divide: after = Mathf.RoundToInt(before / Mathf.Max(1f, gate.value)); break;
                case GateOperation.SetValue: after = Mathf.RoundToInt(gate.value); break;
                case GateOperation.RandomRange: after = Mathf.Max(0, before + Random.Range(gate.randomMin, gate.randomMax + 1)); break;
            }
            after = Mathf.Max(0, after);

            if (after > before) SpawnMany(after - before, gatePosition, BattlefieldBounds.Instance.HalfWidth * 0.8f);
            else if (after < before) RemoveUnits(before - after);

            EventBus.Publish(new GateTriggeredEvent(gatePosition, gate.label, before, Count));
        }

        private void ReleaseUnit(FriendlyUnit unit)
        {
            _poolsByUnitId[unit.UnitDataId].Release(unit);
        }

        private void ReleaseAll()
        {
            foreach (var pool in _poolsByUnitId.Values) pool.ReleaseAll();
            _active.Clear();
            _pendingRelease.Clear();
        }

        /// <summary>Called once per frame by BattleManager. Moves units and reclaims dead ones.</summary>
        public void Tick(float deltaTime)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var unit = _active[i];
                if (!unit.IsAlive)
                {
                    _active.RemoveAt(i);
                    _pendingRelease.Add((unit, Time.time + 0.6f));
                    continue;
                }
                unit.TickMovement(deltaTime, _battlefieldLength);
            }

            for (int i = _pendingRelease.Count - 1; i >= 0; i--)
            {
                if (Time.time >= _pendingRelease[i].releaseAt)
                {
                    ReleaseUnit(_pendingRelease[i].unit);
                    _pendingRelease.RemoveAt(i);
                }
            }
        }

        public Vector3 GetCentroid()
        {
            if (_active.Count == 0) return BattlefieldBounds.Instance != null ? BattlefieldBounds.Instance.CannonSpawnPoint : Vector3.zero;
            Vector3 sum = Vector3.zero;
            foreach (var u in _active) sum += u.transform.position;
            return sum / _active.Count;
        }

        public float GetFrontZ()
        {
            float maxZ = 0f;
            foreach (var u in _active) if (u.transform.position.z > maxZ) maxZ = u.transform.position.z;
            return maxZ;
        }
    }
}
