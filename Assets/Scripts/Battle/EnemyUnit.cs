using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>An enemy guard. Stays at its post until a friendly unit enters detection range.</summary>
    public class EnemyUnit : BattleUnit
    {
        public override TeamSide Team => TeamSide.Enemy;

        public string EnemyDataId;
        public Vector3 GuardPosition;
        public float DetectionRange = 6f;
        public bool IsEngaged;

        public override void ResetForSpawn(CombatStatsRuntime stats, Vector3 position)
        {
            base.ResetForSpawn(stats, position);
            GuardPosition = position;
            IsEngaged = false;
        }

        /// <summary>Called once per tick by EnemyManager for every active enemy unit.</summary>
        public virtual void TickMovement(float deltaTime)
        {
            if (!IsAlive) return;

            if (CurrentTarget == null || !CurrentTarget.IsAlive)
            {
                IsEngaged = false;
                AnimState = UnitAnimState.Idle;
                return;
            }

            IsEngaged = true;
            float dist = DistanceToTarget();
            if (dist <= Stats.Range + 0.25f)
            {
                AnimState = UnitAnimState.Fighting;
                return;
            }

            AnimState = UnitAnimState.Moving;
            Vector3 dir = (CurrentTarget.TargetTransform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                transform.position += dir * (Stats.MoveSpeed * deltaTime);
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}
