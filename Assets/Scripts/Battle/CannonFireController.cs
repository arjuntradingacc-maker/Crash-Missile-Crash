using CrashMissileCrash.Core;
using CrashMissileCrash.Data;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>Fires the equipped cannon at its configured rate, spawning friendly units.</summary>
    public class CannonFireController : MonoBehaviour
    {
        public static CannonFireController Instance { get; private set; }

        public string EquippedCannonId = "scrapcannon_mk1";
        public int CannonLevel = 1;

        private float _fireTimer;
        private CannonData _cannonData;
        private bool _firingEnabled;

        private void Awake()
        {
            Instance = this;
        }

        public void BeginBattle(string cannonId, int cannonLevel)
        {
            EquippedCannonId = cannonId;
            CannonLevel = cannonLevel;
            GameDatabase.Instance.Cannons.TryGetValue(EquippedCannonId, out _cannonData);
            _fireTimer = 0f;
            _firingEnabled = true;

            if (_cannonData != null && CrowdManager.Instance != null)
            {
                CrowdManager.Instance.SetActiveUnitType(_cannonData.spawnUnitId);
            }
        }

        public void StopFiring() => _firingEnabled = false;

        private void Update()
        {
            if (!_firingEnabled || _cannonData == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Battle) return;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            float fireRate = _cannonData.fireRate + _cannonData.perLevelFireRateGrowth * (CannonLevel - 1);
            _fireTimer = 1f / Mathf.Max(0.1f, fireRate);
            Fire();
        }

        private void Fire()
        {
            Vector3 origin = CannonController.Instance != null
                ? CannonController.Instance.transform.position + Vector3.forward * 0.6f
                : BattlefieldBounds.Instance.CannonSpawnPoint;

            int shots = Mathf.Max(1, _cannonData.unitsPerShot);
            for (int i = 0; i < shots; i++)
            {
                float offset = shots > 1 ? (i - (shots - 1) * 0.5f) * 0.5f : 0f;
                var pos = origin + new Vector3(offset, 0f, 0f);
                pos.x = BattlefieldBounds.Instance.ClampX(pos.x);
                CrowdManager.Instance.SpawnUnit(pos);
            }

            EventBus.Publish(new CannonShotFiredEvent(origin));
        }
    }
}
