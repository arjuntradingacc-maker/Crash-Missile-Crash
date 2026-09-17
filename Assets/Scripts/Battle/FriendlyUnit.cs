using System.Collections.Generic;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>A single friendly crowd member. Moves forward toward the enemy base unless fighting.</summary>
    public class FriendlyUnit : BattleUnit
    {
        public override TeamSide Team => TeamSide.Friendly;

        public string UnitDataId;
        public int Level = 1;

        /// <summary>Multiplier applied to MoveSpeed this frame by speed zones/moving platforms; reset every tick.</summary>
        public float SpeedMultiplier = 1f;

        /// <summary>Ids of one-shot obstacles (launch pads, teleporters, hazards) already consumed by this unit instance.</summary>
        public readonly HashSet<string> TriggeredOneShotObstacles = new HashSet<string>();

        public override void ResetForSpawn(CombatStatsRuntime stats, Vector3 position)
        {
            base.ResetForSpawn(stats, position);
            SpeedMultiplier = 1f;
            TriggeredOneShotObstacles.Clear();
        }

        /// <summary>Called once per tick by CrowdManager for every active friendly unit.</summary>
        public void TickMovement(float deltaTime, float battlefieldLength)
        {
            if (!IsAlive) return;

            bool inCombat = CurrentTarget != null && CurrentTarget.IsAlive && DistanceToTarget() <= Stats.Range + 0.25f;
            if (inCombat)
            {
                AnimState = UnitAnimState.Fighting;
                return;
            }

            AnimState = UnitAnimState.Moving;
            Vector3 pos = transform.position;
            pos.z = Mathf.Min(battlefieldLength, pos.z + Stats.MoveSpeed * SpeedMultiplier * deltaTime);
            transform.position = pos;
            SpeedMultiplier = 1f;
        }

        protected override void Die()
        {
            base.Die();
        }
    }
}
