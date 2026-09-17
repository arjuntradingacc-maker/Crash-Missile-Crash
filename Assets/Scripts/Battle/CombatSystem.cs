using System;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Resolves all targeting and attacks for the current battle. Rebuilds two spatial grids
    /// once per tick (friendly targets, enemy+base targets) so nearest-target queries stay cheap
    /// with large crowds, then lets each unit attack if a locked-on target is within its range.
    /// Units keep a target until it dies rather than re-picking every frame, which avoids target
    /// flicker and keeps the query cost to "once per unit per engagement" instead of per-frame.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance { get; private set; }

        private SpatialGrid<ICombatTarget> _friendlyGrid;
        private SpatialGrid<ICombatTarget> _enemyGrid;
        private readonly Random _rng = new Random();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            _friendlyGrid = new SpatialGrid<ICombatTarget>(2.5f);
            _enemyGrid = new SpatialGrid<ICombatTarget>(2.5f);
        }

        public void Tick(float deltaTime)
        {
            if (CrowdManager.Instance == null || EnemyManager.Instance == null || EnemyBase.Instance == null) return;

            _friendlyGrid.Clear();
            _enemyGrid.Clear();

            foreach (var f in CrowdManager.Instance.ActiveUnits)
            {
                if (f.IsAlive) _friendlyGrid.Insert(f, f.transform.position);
            }
            foreach (var e in EnemyManager.Instance.ActiveUnits)
            {
                if (e.IsAlive) _enemyGrid.Insert(e, e.transform.position);
            }
            foreach (var t in EnemyBase.Instance.GetTargets())
            {
                _enemyGrid.Insert(t, t.TargetTransform.position);
            }
            if (SpecialMechanics.ObstacleManager.Instance != null)
            {
                foreach (var t in SpecialMechanics.ObstacleManager.Instance.GetDamageableTargets())
                {
                    _enemyGrid.Insert(t, t.TargetTransform.position);
                }
            }

            var championUnit = ChampionManager.Instance != null ? ChampionManager.Instance.ActiveChampionUnit : null;
            if (championUnit != null && championUnit.IsAlive)
            {
                _friendlyGrid.Insert(championUnit, championUnit.transform.position);
            }

            TickFriendlySide(deltaTime);
            TickEnemySide(deltaTime);

            if (championUnit != null && championUnit.IsAlive) AcquireAndAttack(championUnit, _enemyGrid, deltaTime);
        }

        private void TickFriendlySide(float deltaTime)
        {
            foreach (var f in CrowdManager.Instance.ActiveUnits)
            {
                if (!f.IsAlive) continue;
                AcquireAndAttack(f, _enemyGrid, deltaTime);
            }
        }

        private void AcquireAndAttack(BattleUnit unit, SpatialGrid<ICombatTarget> targetGrid, float deltaTime)
        {
            if (unit.CurrentTarget == null || !unit.CurrentTarget.IsAlive)
            {
                float acquireRadius = Mathf.Max(3.5f, unit.Stats.Range + 1.5f);
                unit.CurrentTarget = FindNearest(targetGrid, unit.transform.position, acquireRadius);
            }

            if (unit.CurrentTarget != null)
            {
                float dist = Vector3.Distance(unit.transform.position, unit.CurrentTarget.TargetTransform.position);
                if (dist <= unit.Stats.Range + 0.25f) unit.TryAttack(deltaTime, _rng);
            }
        }

        private void TickEnemySide(float deltaTime)
        {
            foreach (var e in EnemyManager.Instance.ActiveUnits)
            {
                TickEnemyCombatant(e, deltaTime);
            }
            foreach (var tower in EnemyBase.Instance.Towers)
            {
                if (tower != null) TickEnemyCombatant(tower, deltaTime);
            }
        }

        private void TickEnemyCombatant(EnemyUnit e, float deltaTime)
        {
            if (!e.IsAlive) return;

            if (e.CurrentTarget == null || !e.CurrentTarget.IsAlive)
            {
                e.CurrentTarget = FindNearest(_friendlyGrid, e.transform.position, e.DetectionRange);
            }

            if (e.CurrentTarget != null)
            {
                float dist = Vector3.Distance(e.transform.position, e.CurrentTarget.TargetTransform.position);
                if (dist <= e.Stats.Range + 0.25f) e.TryAttack(deltaTime, _rng);
            }
        }

        private static ICombatTarget FindNearest(SpatialGrid<ICombatTarget> grid, Vector3 position, float radius)
        {
            ICombatTarget best = null;
            float bestSqr = float.MaxValue;
            grid.QueryRadius(position, radius, candidate =>
            {
                if (!candidate.IsAlive) return;
                float sqr = (candidate.TargetTransform.position - position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            });
            return best;
        }
    }
}
