using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Battle.Gates
{
    /// <summary>
    /// A single placed gate instance. Handles its own motion (stationary/sliding/rotating/timed)
    /// and, once open, applies its math operation to the whole crowd the first time any friendly
    /// unit's position overlaps its trigger volume. Triggering the whole crowd's count (rather
    /// than per-unit) matches the genre convention and the spec's worked examples
    /// (20 units -> x3 -> ~60) instead of compounding per individual unit.
    /// </summary>
    public class GateController : MonoBehaviour
    {
        public GateDefinitionData Def { get; private set; }
        public GatePlacementData Placement { get; private set; }
        public bool HasTriggered { get; private set; }
        public bool IsOpen { get; private set; } = true;

        private float _baseWorldX;
        private float _worldZ;
        private float _motionTimer;
        private float _timedPhaseTimer;
        private const float TriggerHalfWidth = 1.4f;
        private const float TriggerHalfDepth = 0.9f;

        public void Initialize(GateDefinitionData def, GatePlacementData placement, float worldX, float worldZ)
        {
            Def = def;
            Placement = placement;
            _baseWorldX = worldX;
            _worldZ = worldZ;
            HasTriggered = false;
            IsOpen = true;
            _motionTimer = Random.Range(0f, 10f);
            _timedPhaseTimer = 0f;
            transform.position = new Vector3(worldX, 1f, worldZ);
        }

        public void Tick(float deltaTime)
        {
            if (HasTriggered) return;
            _motionTimer += deltaTime;

            switch (Def.motion)
            {
                case GateMotionType.Sliding:
                    float x = _baseWorldX + Mathf.Sin(_motionTimer * Def.motionSpeed) * Def.motionRange;
                    transform.position = new Vector3(BattlefieldBounds.Instance.ClampX(x), transform.position.y, _worldZ);
                    break;
                case GateMotionType.Rotating:
                    transform.Rotate(Vector3.up, Def.motionSpeed * 60f * deltaTime, Space.World);
                    break;
                case GateMotionType.Timed:
                    _timedPhaseTimer += deltaTime;
                    float cycle = Mathf.Max(0.1f, Def.openDuration + Def.closedDuration);
                    float phase = _timedPhaseTimer % cycle;
                    IsOpen = phase < Def.openDuration;
                    break;
            }
        }

        public bool TryConsume(Vector3 unitPosition)
        {
            if (HasTriggered || !IsOpen) return false;
            if (Mathf.Abs(unitPosition.x - transform.position.x) > TriggerHalfWidth) return false;
            if (Mathf.Abs(unitPosition.z - _worldZ) > TriggerHalfDepth) return false;

            HasTriggered = true;
            CrowdManager.Instance.ApplyGate(Def, transform.position);
            EventBus.Publish(new GateVisualPulseEvent(transform.position, Def.label));
            return true;
        }
    }

    public readonly struct GateVisualPulseEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly string Label;
        public GateVisualPulseEvent(Vector3 position, string label) { Position = position; Label = label; }
    }
}
