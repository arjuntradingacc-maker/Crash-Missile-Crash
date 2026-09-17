using System.Collections.Generic;
using CrashMissileCrash.Battle.Gates;
using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Central home for all "juice": floating damage numbers, gate pulse labels, spawn/death/
    /// impact particle bursts. Subscribes to gameplay events instead of being called directly,
    /// so any new event source (abilities, obstacles, bosses) gets VFX for free.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private readonly Queue<TextMesh> _textPool = new Queue<TextMesh>();
        private readonly Queue<ParticleSystem> _burstPool = new Queue<ParticleSystem>();
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            _poolRoot = new GameObject("_VFXPool").transform;
            _poolRoot.SetParent(transform, false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Subscribe<GateVisualPulseEvent>(OnGatePulse);
            EventBus.Subscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Unsubscribe<GateVisualPulseEvent>(OnGatePulse);
            EventBus.Unsubscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);
        }

        private void OnDamageDealt(DamageDealtEvent e)
        {
            var color = e.TargetTeam == TeamSide.Enemy ? (e.IsCrit ? new Color(1f, 0.85f, 0.1f) : Color.white) : new Color(1f, 0.35f, 0.3f);
            SpawnFloatingText(e.Position, Mathf.RoundToInt(e.Amount).ToString(), color, e.IsCrit ? 1.4f : 1f);
        }

        private void OnUnitDied(UnitDiedEvent e)
        {
            SpawnBurst(e.Position, e.Team == TeamSide.Enemy ? new Color(1f, 0.5f, 0.15f) : new Color(0.5f, 0.6f, 1f), e.IsStructure ? 2.2f : 1f);
        }

        private void OnGatePulse(GateVisualPulseEvent e)
        {
            SpawnFloatingText(e.Position + Vector3.up * 1.2f, e.Label, new Color(1f, 1f, 0.4f), 1.8f);
            SpawnBurst(e.Position, new Color(1f, 1f, 0.5f), 1.3f);
        }

        private void OnCoreDestroyed(BaseCoreDestroyedEvent e)
        {
            SpawnBurst(BattlefieldBounds.Instance != null ? BattlefieldBounds.Instance.BaseCorePosition : Vector3.zero, new Color(1f, 0.6f, 0.1f), 4f);
        }

        public void SpawnFloatingText(Vector3 worldPosition, string text, Color color, float scale)
        {
            var tm = _textPool.Count > 0 ? _textPool.Dequeue() : CreateTextMesh();
            tm.transform.position = worldPosition;
            tm.transform.localScale = Vector3.one * scale * 0.3f;
            tm.text = text;
            tm.color = color;
            tm.gameObject.SetActive(true);
            StartCoroutine(FloatAndRelease(tm));
        }

        private System.Collections.IEnumerator FloatAndRelease(TextMesh tm)
        {
            float duration = 0.8f;
            float t = 0f;
            Vector3 start = tm.transform.position;
            while (t < duration)
            {
                t += Time.deltaTime;
                tm.transform.position = start + Vector3.up * (t / duration) * 1.2f;
                var c = tm.color;
                c.a = 1f - (t / duration);
                tm.color = c;
                if (Camera.main != null) tm.transform.rotation = Camera.main.transform.rotation;
                yield return null;
            }
            tm.gameObject.SetActive(false);
            tm.transform.SetParent(_poolRoot, false);
            _textPool.Enqueue(tm);
        }

        private TextMesh CreateTextMesh()
        {
            var go = new GameObject("FloatingText");
            go.transform.SetParent(_poolRoot, false);
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 48;
            tm.characterSize = 0.3f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            return tm;
        }

        public void SpawnBurst(Vector3 worldPosition, Color color, float scale)
        {
            var ps = _burstPool.Count > 0 ? _burstPool.Dequeue() : CreateBurst();
            ps.transform.position = worldPosition;
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.2f * scale;
            ps.Clear();
            ps.Emit(Mathf.RoundToInt(12 * scale));
            StartCoroutine(ReleaseBurstAfter(ps, 1.2f));
        }

        private System.Collections.IEnumerator ReleaseBurstAfter(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.transform.SetParent(_poolRoot, false);
            _burstPool.Enqueue(ps);
        }

        private ParticleSystem CreateBurst()
        {
            var go = new GameObject("ImpactBurst");
            go.transform.SetParent(_poolRoot, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.maxParticles = 64;
            main.playOnAwake = false;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            renderer.material = new Material(shader);
            return ps;
        }
    }
}
