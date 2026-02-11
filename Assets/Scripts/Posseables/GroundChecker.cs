using UnityEngine;
using R3;
using Element.Interfaces;
using System;

public class GroundChecker : MonoBehaviour, IGrounded
{
    [Header("Ground Detection")]
    [SerializeField] private Vector2 _leftRayOffset = new(-0.4f, 0f);
    [SerializeField] private Vector2 _rightRayOffset = new(0.4f, 0f);
    [SerializeField] private float _rayLength = 0.6f;

    [Header("Fall Settings")]
    [SerializeField] private float _fallSpeed = 10f;

    private readonly ReactiveProperty<bool> _isGrounded = new(false);
    private readonly CompositeDisposable _disposables = new();

    public bool IsGrounded => _isGrounded.Value;
    public ReadOnlyReactiveProperty<bool> IsGroundedRP => _isGrounded;

    private void Start()
    {
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                _isGrounded.Value = CheckGround();
            })
            .AddTo(_disposables);
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
