using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Horizontal drag control for the player's cannon. Reads touch (mobile) or mouse (editor/
    /// desktop testing) input and smoothly follows the finger while staying inside the lane.
    /// </summary>
    public class CannonController : MonoBehaviour
    {
        public static CannonController Instance { get; private set; }

        [SerializeField] private float followLerpSpeed = 14f;
        [SerializeField] private Camera targetCamera;

        private float _targetX;
        private bool _dragging;
        public bool HasEverDragged { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void Start()
        {
            _targetX = transform.position.x;
        }

        private void Update()
        {
            if (BattlefieldBounds.Instance == null) return;

            if (TryGetPointerWorldX(out float worldX))
            {
                _targetX = BattlefieldBounds.Instance.ClampX(worldX);
            }

            var pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, _targetX, 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime));
            transform.position = pos;
        }

        private bool TryGetPointerWorldX(out float worldX)
        {
            worldX = 0f;
            Vector2 screenPos;
            bool held;

#if UNITY_EDITOR || UNITY_STANDALONE
            held = Input.GetMouseButton(0);
            screenPos = Input.mousePosition;
#else
            held = Input.touchCount > 0;
            screenPos = held ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
#endif
            _dragging = held;
            if (held) HasEverDragged = true;
            if (!held || targetCamera == null) return false;

            Ray ray = targetCamera.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
            if (groundPlane.Raycast(ray, out float enter))
            {
                worldX = ray.GetPoint(enter).x;
                return true;
            }
            return false;
        }

        public float CurrentX => transform.position.x;
    }
}
