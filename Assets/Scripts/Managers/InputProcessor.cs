using UnityEngine;
using UnityEngine.InputSystem;
using System;
using R3;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// InputSystemからの入力を受け取り、R3で配信するマネージャー（POCO）
    /// Observable.EveryUpdateでポーリング
    /// </summary>
    public class InputProcessor : IDisposable
    {
        private readonly InputActionAsset _inputAsset;
        private readonly FocusEvents _focusEvents;
        private readonly IGameStateEvents _gameStateEvents;
        private readonly CompositeDisposable _disposables = new();

        #region 入力ストリーム

        private readonly ReactiveProperty<Vector2> _move = new(Vector2.zero);
        private readonly Subject<Unit> _possessInput = new();
        private readonly Subject<Unit> _jump = new();

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public Observable<Unit> PossessInput => _possessInput;
        public Observable<Unit> Jump => _jump;

        #endregion

        private InputAction _moveAction;
        private InputAction _focusModeAction;
        private InputAction _possessAction;
        private InputAction _jumpAction;

        private bool _inputEnabled = true;
        private bool _wasFocusPressed;

        public InputProcessor(
            InputActionAsset inputAsset,
            FocusEvents focusEvents,
            IGameStateEvents gameStateEvents)
        {
            _inputAsset = inputAsset;
            _focusEvents = focusEvents;
            _gameStateEvents = gameStateEvents;

            Initialize();
        }

        private void Initialize()
        {
            SetupInputActions();
            SubscribeToGameState();
            StartPolling();
        }

        private void SetupInputActions()
        {
            if (_inputAsset == null)
            {
                Debug.LogError("InputActionAsset is not assigned!");
                return;
            }

            var actionMap = _inputAsset.FindActionMap("Player");
            if (actionMap == null)
            {
                Debug.LogError("Player action map not found!");
                return;
            }

            _moveAction = actionMap.FindAction("Move");
            _focusModeAction = actionMap.FindAction("Focus");
            _possessAction = actionMap.FindAction("Possess");
            _jumpAction = actionMap.FindAction("Jump");

            actionMap.Enable();
            Debug.Log("InputProcessor: actions set up");
        }

        private void StartPolling()
        {
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (!_inputEnabled) return;

                    // Move（連続値）— フォーカス中は移動不可
                    if (_moveAction != null)
                    {
                        _move.Value = _wasFocusPressed ? Vector2.zero : _moveAction.ReadValue<Vector2>();
                    }

                    // Possess（トリガー）
                    if (_possessAction != null && _possessAction.WasPressedThisFrame())
                    {
                        _possessInput.OnNext(Unit.Default);
                    }

                    // Jump（トリガー）
                    if (_jumpAction != null && _jumpAction.WasPressedThisFrame())
                    {
                        _jump.OnNext(Unit.Default);
                    }

                    // Focus（ホールド）
                    if (_focusModeAction != null)
                    {
                        bool isPressed = _focusModeAction.IsPressed();
                        if (isPressed != _wasFocusPressed)
                        {
                            _wasFocusPressed = isPressed;
                            _focusEvents?.NotifyFocusModeChanged(isPressed);
                        }
                    }
                })
                .AddTo(_disposables);
        }

        private void SubscribeToGameState()
        {
            if (_gameStateEvents == null) return;

            _gameStateEvents.CurrentGameState
                .Subscribe(state =>
                {
                    bool shouldAcceptInput = state == Element.Enums.GameState.Playing
                                           || state == Element.Enums.GameState.FocusMode;

                    if (shouldAcceptInput && !_inputEnabled)
                        EnableInput();
                    else if (!shouldAcceptInput && _inputEnabled)
                        DisableInput();
                })
                .AddTo(_disposables);
        }

        public void EnableInput()
        {
            _inputEnabled = true;
        }

        public void DisableInput()
        {
            _inputEnabled = false;
            _move.Value = Vector2.zero;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _possessInput?.Dispose();
            _jump?.Dispose();
            _move?.Dispose();
        }
    }
}
