using CrashMissileCrash.Core;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Elevated third-person/top-down battle camera. Follows the crowd's forward progress,
    /// zooms out as the crowd grows, and reacts to big moments with screen shake.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [SerializeField] private float heightOffset = 9f;
        [SerializeField] private float backOffset = 7f;
        [SerializeField] private float followLerpSpeed = 3f;
        [SerializeField] private float baseFov = 45f;
        [SerializeField] private float maxExtraFov = 12f;
        [SerializeField] private int crowdSizeForMaxZoom = 300;

        private Camera _camera;
        private Vector3 _shakeOffset;
        private float _shakeTimer;
        private float _shakeMagnitude;

        private void Awake()
        {
            Instance = this;
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = Camera.main;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Subscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            EventBus.Unsubscribe<BaseCoreDestroyedEvent>(OnCoreDestroyed);
        }

        private void OnUnitDied(UnitDiedEvent e)
        {
            if (e.IsStructure) Shake(0.25f, 0.18f);
        }

        private void OnCoreDestroyed(BaseCoreDestroyedEvent e)
        {
            Shake(0.9f, 0.55f);
        }

        public void Shake(float magnitude, float duration)
        {
            _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
            _shakeTimer = Mathf.Max(_shakeTimer, duration);
        }

        private void LateUpdate()
        {
            if (CrowdManager.Instance == null || BattlefieldBounds.Instance == null) return;

            float followZ = Mathf.Max(CrowdManager.Instance.GetFrontZ() - 2f, BattlefieldBounds.Instance.CannonSpawnPoint.z);
            Vector3 targetPos = new Vector3(0f, heightOffset, Mathf.Max(0f, followZ - backOffset));
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            if (_camera != null)
            {
                float t = Mathf.Clamp01(CrowdManager.Instance.Count / (float)crowdSizeForMaxZoom);
                float targetFov = baseFov + maxExtraFov * t;
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, Time.deltaTime * 2f);
            }

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                _shakeOffset = Random.insideUnitSphere * _shakeMagnitude;
                transform.position += _shakeOffset;
                if (_shakeTimer <= 0f) _shakeMagnitude = 0f;
            }
        }
    }
}
