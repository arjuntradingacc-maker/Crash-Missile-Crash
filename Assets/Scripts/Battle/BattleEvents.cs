using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    public readonly struct DamageDealtEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly float Amount;
        public readonly bool IsCrit;
        public readonly TeamSide TargetTeam;
        public DamageDealtEvent(Vector3 position, float amount, bool isCrit, TeamSide targetTeam)
        {
            Position = position; Amount = amount; IsCrit = isCrit; TargetTeam = targetTeam;
        }
    }

    public readonly struct UnitDiedEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly TeamSide Team;
        public readonly bool IsStructure;
        public UnitDiedEvent(Vector3 position, TeamSide team, bool isStructure)
        {
            Position = position; Team = team; IsStructure = isStructure;
        }
    }

    public readonly struct GateTriggeredEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly string Label;
        public readonly int CrowdCountBefore;
        public readonly int CrowdCountAfter;
        public GateTriggeredEvent(Vector3 position, string label, int before, int after)
        {
            Position = position; Label = label; CrowdCountBefore = before; CrowdCountAfter = after;
        }
    }

    public readonly struct CrowdCountChangedEvent : IGameEvent
    {
        public readonly int Count;
        public CrowdCountChangedEvent(int count) { Count = count; }
    }

    public readonly struct BaseCoreDestroyedEvent : IGameEvent { }

    public readonly struct BattleVictoryEvent : IGameEvent
    {
        public readonly int SurvivingUnits;
        public BattleVictoryEvent(int survivingUnits) { SurvivingUnits = survivingUnits; }
    }

    public readonly struct BattleDefeatEvent : IGameEvent { }

    public readonly struct CannonShotFiredEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public CannonShotFiredEvent(Vector3 position) { Position = position; }
    }

    public readonly struct ChampionUltimateReadyEvent : IGameEvent
    {
        public readonly bool IsReady;
        public ChampionUltimateReadyEvent(bool isReady) { IsReady = isReady; }
    }
}
