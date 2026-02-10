using UnityEngine;
using VContainer;
using R3;
using Element.Interfaces;
using Element.Managers;
using System;

public class Dark : MonoBehaviour, IPossable, IHorizontalMove, IGrounded
{
    [Header("IPossable Settings")]
    [SerializeField] private int _layer = 0;
    [SerializeField] private Transform _core;

    [Header("Physics Settings")]
    [SerializeField] private bool _useGravity = true;
    [SerializeField] private bool _isKinematicWhenNotPossessed = false;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _fallSpeed = 10f;

    [Header("Ground Detection")]
    [SerializeField] private Vector2 _leftRayOffset = new(-0.4f, 0f);
    [SerializeField] private Vector2 _rightRayOffset = new(0.4f, 0f);
    [SerializeField] private float _rayLength = 0.6f;

    private Rigidbody2D _rb;
    private InputProcessor _inputProcessor;
    private readonly CompositeDisposable _disposables = new();

    public int Layer => _layer;
    public bool IsPossess { get; set; }
    public Transform Core => _core != null ? _core : transform;
    public bool UseGravity => _useGravity;
    public bool IsKinematicWhenNotPossessed => _isKinematicWhenNotPossessed;
    private IDisposable _xMoveDisposables;

    private readonly ReactiveProperty<bool> _isGrounded = new(false);
    public bool IsGrounded => _isGrounded.Value;

    [Inject]
    public void Construct(InputProcessor inputProcessor)
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            //_rb.gravityScale = 0; // 2Dトップダウンの場合
        }

        // 初期物理設定
        UpdatePhysicsSettings();
        Debug.Log("DarkConstruct");
        _inputProcessor = inputProcessor;
        if (_inputProcessor == null) { Debug.Log("InputProcessor is null"); return; }
        SubscribeToInput();

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                _isGrounded.Value = CheckGround();
                ConfigureYVelocity(_rb);
            })
            .AddTo(_disposables);
        _isGrounded.Subscribe(_ =>
        {
            Debug.Log($"IsGrounded: {_}");
        })
            .AddTo(_disposables);
    }


    private void SubscribeToInput()
    {
        if (_inputProcessor == null) { Debug.Log("InputProcessor is null"); return; }
        Debug.Log("Subscribing to input");
        // Move入力を購読
        _inputProcessor.Move
            .Subscribe(moveInput =>
            {
                Debug.Log($"Move input: {moveInput}");
                if (IsPossess)
                {
                    XMove(moveInput.x);
                }
            })
            .AddTo(_disposables);
    }

    public void XMove(float direction)
    {
        _xMoveDisposables?.Dispose();
        _xMoveDisposables = Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                _rb.linearVelocity = new Vector2(direction * _moveSpeed, _rb.linearVelocity.y);
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

    public void ConfigureYVelocity(Rigidbody2D rb)
    {
        if (_isGrounded.Value)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -_fallSpeed);
        }
    }

    private bool CheckGround()
    {
        Vector2 origin = (Vector2)transform.position;

        // 左端レイ（すべてのヒットを判定）
        var hitsLeft = Physics2D.RaycastAll(origin + _leftRayOffset, Vector2.down, _rayLength);
        foreach (var hit in hitsLeft)
        {
            if (hit.collider != null && hit.collider.GetComponent<IGround>() != null)
                return true;
        }

        // 右端レイ（すべてのヒットを判定）
        var hitsRight = Physics2D.RaycastAll(origin + _rightRayOffset, Vector2.down, _rayLength);
        foreach (var hit in hitsRight)
        {
            if (hit.collider != null && hit.collider.GetComponent<IGround>() != null)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position;
        Gizmos.color = _isGrounded.Value ? Color.green : Color.red;
        Gizmos.DrawLine(origin + _leftRayOffset, origin + _leftRayOffset + Vector2.down * _rayLength);
        Gizmos.DrawLine(origin + _rightRayOffset, origin + _rightRayOffset + Vector2.down * _rayLength);
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}
