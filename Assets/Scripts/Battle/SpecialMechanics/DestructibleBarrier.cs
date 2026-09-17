using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle.SpecialMechanics
{
    /// <summary>A lane-blocking structure with health; friendly units auto-attack it like any enemy target.</summary>
    public class DestructibleBarrier : MonoBehaviour, ICombatTarget
    {
        public float MaxHealth = 40f;
        public float Health;
        public Transform TargetTransform => transform;
        public TeamSide Team => TeamSide.Enemy;
        public bool IsStructure => true;
        public bool IsAlive => Health > 0f;

        public void Initialize(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            Health = MaxHealth;
        }

        public float TakeDamage(float amount, bool isCrit)
        {
            if (!IsAlive) return 0f;
            float dealt = Mathf.Min(Health, amount);
            Health -= dealt;
            EventBus.Publish(new DamageDealtEvent(transform.position + Vector3.up, dealt, isCrit, Team));
            if (Health <= 0f)
            {
                EventBus.Publish(new UnitDiedEvent(transform.position, Team, true));
            }
            return dealt;
        }
    }
}
