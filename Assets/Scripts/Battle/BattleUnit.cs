using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    public enum UnitAnimState { Moving, Fighting, Dying, Celebrating, Idle }

    /// <summary>
    /// Shared runtime behaviour for any living combatant (friendly or enemy). Movement is driven
    /// externally by CrowdManager/EnemyManager (called once per tick in a plain loop, not through
    /// per-object Unity Update dispatch) so large crowds stay cheap; this class only holds state
    /// and resolves a single attack when instructed to by CombatSystem.
    /// </summary>
    public abstract class BattleUnit : MonoBehaviour, ICombatTarget
    {
        public CombatStatsRuntime Stats { get; protected set; }
        public abstract TeamSide Team { get; }
        public virtual bool IsStructure => false;
        public bool IsAlive => Stats != null && Stats.IsAlive;
        public Transform TargetTransform => transform;

        public ICombatTarget CurrentTarget { get; set; }
        public float AttackTimer;
        public UnitAnimState AnimState { get; protected set; } = UnitAnimState.Moving;

        [System.NonSerialized] public string VisualKey;

        public virtual void ResetForSpawn(CombatStatsRuntime stats, Vector3 position)
        {
            Stats = stats;
            CurrentTarget = null;
            AttackTimer = 0f;
            AnimState = UnitAnimState.Moving;
            transform.position = position;
            transform.rotation = Quaternion.identity;
        }

        public float TakeDamage(float amount, bool isCrit)
        {
            if (!IsAlive) return 0f;
            float dealt = Stats.ApplyDamage(amount);
            EventBus.Publish(new DamageDealtEvent(transform.position + Vector3.up * 0.8f, dealt, isCrit, Team));
            if (!Stats.IsAlive)
            {
                Die();
            }
            return dealt;
        }

        protected virtual void Die()
        {
            AnimState = UnitAnimState.Dying;
            EventBus.Publish(new UnitDiedEvent(transform.position, Team, false));
        }

        public bool TryAttack(float deltaTime, System.Random rng)
        {
            if (CurrentTarget == null || !CurrentTarget.IsAlive) return false;
            AttackTimer -= deltaTime;
            if (AttackTimer > 0f)
            {
                AnimState = UnitAnimState.Fighting;
                return false;
            }
            AttackTimer = 1f / Mathf.Max(0.05f, Stats.AttackSpeed);
            AnimState = UnitAnimState.Fighting;
            bool crit = rng.NextDouble() < Stats.CritChance;
            float dmg = crit ? Stats.Damage * Stats.CritMultiplier : Stats.Damage;
            CurrentTarget.TakeDamage(dmg, crit);
            return true;
        }

        public float DistanceToTarget()
        {
            if (CurrentTarget == null) return float.MaxValue;
            return Vector3.Distance(transform.position, CurrentTarget.TargetTransform.position);
        }
    }
}
