using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Champions
{
    /// <summary>
    /// The single deployed champion for the current battle. Fights like a much stronger friendly
    /// unit and additionally banks an ultimate ability the player can trigger from the battle HUD.
    /// </summary>
    public class ChampionUnit : BattleUnit
    {
        public override TeamSide Team => TeamSide.Friendly;

        public ChampionData Data { get; private set; }
        public bool IsUltimateReady { get; private set; }
        private float _cooldownTimer;

        public void InitializeChampion(ChampionData data, int level, Vector3 position)
        {
            Data = data;
            var stats = CombatStatsRuntime.FromData(data.baseStats, data.perLevelGrowth, level);
            ResetForSpawn(stats, position);
            _cooldownTimer = data.ultimateCooldown * 0.5f; // starts partially charged
            IsUltimateReady = false;
        }

        public void TickMovement(float deltaTime, float battlefieldLength)
        {
            if (!IsAlive) return;

            bool inCombat = CurrentTarget != null && CurrentTarget.IsAlive && DistanceToTarget() <= Stats.Range + 0.25f;
            if (!inCombat)
            {
                AnimState = UnitAnimState.Moving;
                var pos = transform.position;
                pos.z = Mathf.Min(battlefieldLength, pos.z + Stats.MoveSpeed * deltaTime);
                transform.position = pos;
            }
            else
            {
                AnimState = UnitAnimState.Fighting;
            }

            if (!IsUltimateReady)
            {
                _cooldownTimer -= deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    IsUltimateReady = true;
                    EventBus.Publish(new ChampionUltimateReadyEvent(true));
                }
            }
        }

        public bool TryActivateUltimate()
        {
            if (!IsUltimateReady || !IsAlive) return false;
            ChampionAbilityExecutor.Execute(this);
            IsUltimateReady = false;
            _cooldownTimer = Data.ultimateCooldown;
            EventBus.Publish(new ChampionUltimateReadyEvent(false));
            return true;
        }
    }
}
