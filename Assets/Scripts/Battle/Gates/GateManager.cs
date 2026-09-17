using System.Collections.Generic;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Battle.Gates
{
    /// <summary>Spawns and drives every gate placed in the current level.</summary>
    public class GateManager : MonoBehaviour
    {
        public static GateManager Instance { get; private set; }

        private readonly List<GateController> _gates = new List<GateController>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public void BuildLevel(LevelData level)
        {
            ClearAll();
            var bounds = BattlefieldBounds.Instance;

            foreach (var placement in level.gates)
            {
                if (!GameDatabase.Instance.Gates.TryGetValue(placement.gateDefId, out var def))
                {
                    Debug.LogWarning($"[GateManager] Unknown gate id: {placement.gateDefId}");
                    continue;
                }

                var go = PrimitiveFactory.CreatePlaceholder($"Gate_{def.id}", PlaceholderShape.Cube, ColorForOperation(def.operation), new Vector3(2.6f, 2.2f, 0.3f));
                var controller = go.AddComponent<GateController>();
                float worldX = bounds.LaneToWorldX(placement.laneX);
                controller.Initialize(def, placement, bounds.ClampX(worldX), placement.distanceFromStart);
                _gates.Add(controller);
            }
        }

        private static Color ColorForOperation(GateOperation op)
        {
            return op switch
            {
                GateOperation.Multiply => new Color(0.25f, 0.85f, 0.35f),
                GateOperation.Add => new Color(0.3f, 0.6f, 1f),
                GateOperation.Subtract => new Color(0.9f, 0.3f, 0.3f),
                GateOperation.Divide => new Color(0.9f, 0.55f, 0.2f),
                GateOperation.SetValue => new Color(0.7f, 0.7f, 0.3f),
                _ => new Color(0.7f, 0.3f, 0.9f)
            };
        }

        public void Tick(float deltaTime)
        {
            if (CrowdManager.Instance == null) return;

            foreach (var gate in _gates)
            {
                gate.Tick(deltaTime);
            }

            foreach (var gate in _gates)
            {
                if (gate.HasTriggered) continue;
                foreach (var unit in CrowdManager.Instance.ActiveUnits)
                {
                    if (!unit.IsAlive) continue;
                    if (gate.TryConsume(unit.transform.position)) break;
                }
            }
        }

        public void ClearAll()
        {
            foreach (var gate in _gates)
            {
                if (gate != null) Destroy(gate.gameObject);
            }
            _gates.Clear();
        }
    }
}
