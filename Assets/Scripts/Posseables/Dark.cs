using UnityEngine;
using R3;
using Element.Interfaces;
using System;

public class Dark : MonoBehaviour, IPossable
{
    [Header("IPossable Settings")]
    [SerializeField] private int _layer = 0;
    [SerializeField] private Transform _core;

    [Header("Physics Settings")]
    [SerializeField] private bool _useGravity = true;
    [SerializeField] private bool _freezeWhenNotPossessed = false;

    [Header("Eye Settings")]
    [SerializeField] private Vector2 _eyePos;

    private Rigidbody2D _rb;
    private GroundChecker _groundChecker;
    private readonly CompositeDisposable _disposables = new();

    public int Layer => _layer;
    public bool IsPossess { get; set; }
    public Transform Core => _core != null ? _core : transform;
    public bool UseGravity => _useGravity;
    public bool FreezeWhenNotPossessed => _freezeWhenNotPossessed;
    public Vector2 EyePos => _eyePos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }

        _groundChecker = GetComponent<GroundChecker>();

        // 初期物理設定
        UpdatePhysicsSettings();

        // GroundCheckerの接地状態に応じてY速度を制御
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                _groundChecker.ConfigureYVelocity(_rb);
            })
            .AddTo(_disposables);
        _groundChecker.IsGroundedRP.Subscribe(_ =>
        {
            Debug.Log($"IsGrounded: {_}");
        })
            .AddTo(_disposables);
    }

    public void TryPossess()
    {
        Debug.Log($"Dark possessed!");
        UpdatePhysicsSettings();
    }

    public void Death()
    {
        Debug.Log("Dark died!");
    }

    /// <summary>
    /// 物理設定を更新
    /// </summary>
    private void UpdatePhysicsSettings()
    {
        if (_rb == null) return;

        // 重力設定
        _rb.gravityScale = UseGravity ? 1f : 0f;

        // Constraint設定（憑依されていない時はX,Yを固定）
        if (!IsPossess && FreezeWhenNotPossessed)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            _rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}
