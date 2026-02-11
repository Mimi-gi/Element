using UnityEngine;
using R3;
using Element.Interfaces;
using System;

public class VerticalMover : MonoBehaviour, IVerticalMove
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody2D _rb;

    private IDisposable _yMoveDisposable;
    private readonly CompositeDisposable _disposables = new();
    void Awake()
    {
        if (_rb == null) _rb = this.GetComponent<Rigidbody2D>();
    }

    public void YMove(float direction)
    {
        _yMoveDisposable?.Dispose();
        _yMoveDisposable = Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                if (direction != 0)
                {
                    Debug.Log("YMove: " + direction);
                }
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, direction * _moveSpeed);
            })
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}