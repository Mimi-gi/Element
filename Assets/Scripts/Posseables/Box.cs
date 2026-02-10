using UnityEngine;
using Element.Interfaces;

namespace Element.Posseables
{
    /// <summary>
    /// テスト用のBox（乗り移り可能）
    /// </summary>
    public class Box : MonoBehaviour, IPossable,IGround
    {
        [Header("IPossable Settings")]
        [SerializeField] private int _layer = 1;
        [SerializeField] private Transform _core;

        [Header("Physics Settings")]
        [SerializeField] private bool _useGravity = true;
        [SerializeField] private bool _isKinematicWhenNotPossessed = true;

        private Rigidbody2D _rb;

        public int Layer => _layer;
        public bool IsPossess { get; set; }
        public Transform Core => _core != null ? _core : transform;
        public bool UseGravity => _useGravity;
        public bool IsKinematicWhenNotPossessed => _isKinematicWhenNotPossessed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }

            UpdatePhysicsSettings();
        }

        public void TryPossess()
        {
            Debug.Log($"Box possessed! Layer: {_layer}");
            UpdatePhysicsSettings();
        }

        public void Death()
        {
            Debug.Log("Box died!");
            Destroy(gameObject);
        }

        /// <summary>
        /// 物理設定を更新
        /// </summary>
        private void UpdatePhysicsSettings()
        {
            if (_rb == null) return;

            // 重力設定
            _rb.gravityScale = UseGravity ? 1f : 0f;

            // Kinematic設定（憑依されていない時）
            if (!IsPossess && IsKinematicWhenNotPossessed)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        private void OnDrawGizmos()
        {
            // コアを視覚化
            Gizmos.color = IsPossess ? Color.green : Color.yellow;
            Vector3 corePos = Core != null ? Core.position : transform.position;
            Gizmos.DrawWireSphere(corePos, 0.3f);
        }
    }
}
