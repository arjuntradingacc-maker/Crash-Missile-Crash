using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>The win-condition objective. Takes damage from any friendly unit that reaches it
    /// and in range; destroying it ends the battle in victory.</summary>
    public class EnemyBaseCore : MonoBehaviour, ICombatTarget
    {
        public float MaxHealth { get; private set; } = 500f;
        public float Health { get; private set; }
        public Transform TargetTransform => transform;
        public TeamSide Team => TeamSide.Enemy;
        public bool IsStructure => true;
        public bool IsAlive => Health > 0f;

        private bool _destroyedEventFired;

        public void Initialize(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            Health = MaxHealth;
            _destroyedEventFired = false;
            gameObject.SetActive(true);
        }

        public float TakeDamage(float amount, bool isCrit)
        {
            if (!IsAlive) return 0f;
            float dealt = Mathf.Min(Health, amount);
            Health -= dealt;
            EventBus.Publish(new DamageDealtEvent(transform.position + Vector3.up * 1.2f, dealt, isCrit, Team));

            if (Health <= 0f && !_destroyedEventFired)
            {
                _destroyedEventFired = true;
                EventBus.Publish(new UnitDiedEvent(transform.position, Team, true));
                EventBus.Publish(new BaseCoreDestroyedEvent());
            }
            return dealt;
        }

        public float HealthFraction01 => MaxHealth > 0f ? Mathf.Clamp01(Health / MaxHealth) : 0f;
    }
}
