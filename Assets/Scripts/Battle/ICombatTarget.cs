using UnityEngine;

namespace CrashMissileCrash.Battle
{
    public enum TeamSide { Friendly, Enemy }

    /// <summary>
    /// Anything that can be targeted and damaged in combat: friendly units, enemy units,
    /// base towers and the enemy base core all implement this so targeting/combat code
    /// (CombatSystem) stays uniform regardless of what kind of object is on the other end.
    /// </summary>
    public interface ICombatTarget
    {
        Transform TargetTransform { get; }
        TeamSide Team { get; }
        bool IsAlive { get; }
        bool IsStructure { get; }
        float TakeDamage(float amount, bool isCrit);
    }
}
