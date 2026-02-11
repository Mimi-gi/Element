using UnityEngine;
using R3;
using Element.Interfaces;
using System;

public class HorizontalMover : MonoBehaviour, IHorizontalMove
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody2D _rb;

    private IDisposable _xMoveDisposable;
    private readonly CompositeDisposable _disposables = new();
    void Awake()
    {
        if (_rb == null) _rb = this.GetComponent<Rigidbody2D>();
    }

    public void XMove(float direction)
    {
        _xMoveDisposable?.Dispose();
        _xMoveDisposable = Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                if (direction != 0)
                {
                    Debug.Log("XMove: " + direction);
                }
                _rb.linearVelocity = new Vector2(direction * _moveSpeed, _rb.linearVelocity.y);
            })
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}
