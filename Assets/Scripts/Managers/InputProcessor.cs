using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using R3;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// InputSystemからの入力を受け取り、R3で配信するマネージャー
    /// </summary>
    public class InputProcessor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputAsset;

        private FocusEvents _focusEvents;
        private IGameStateEvents _gameStateEvents;

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
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(FocusEvents focusEvents, IGameStateEvents gameStateEvents)
        {
            _focusEvents = focusEvents;
            _gameStateEvents = gameStateEvents;
        }

        private void Start()
        {
            SetupInputActions();
            SubscribeToGameState();
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

            if (_moveAction != null)
                _moveAction.performed += OnMovePerformed;

            if (_focusModeAction != null)
            {
                _focusModeAction.performed += _ => OnFocusModeChanged(true);
                _focusModeAction.canceled += _ => OnFocusModeChanged(false);
            }

            if (_possessAction != null)
                _possessAction.performed += _ => OnPossess();

            if (_jumpAction != null)
                _jumpAction.performed += _ => OnJump();

            actionMap.Enable();
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

        #region 入力コールバック

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            _move.Value = context.ReadValue<Vector2>();
        }

        private void OnFocusModeChanged(bool isActive)
        {
            if (!_inputEnabled) return;
            _focusEvents?.NotifyFocusModeChanged(isActive);
        }

        private void OnPossess()
        {
            if (!_inputEnabled) return;
            _possessInput.OnNext(Unit.Default);
        }

        private void OnJump()
        {
            if (!_inputEnabled) return;
            _jump.OnNext(Unit.Default);
        }

        #endregion

        public void EnableInput()
        {
            _inputEnabled = true;
        }

        public void DisableInput()
        {
            _inputEnabled = false;
            _move.Value = Vector2.zero;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _possessInput?.Dispose();
            _jump?.Dispose();
            _move?.Dispose();

            if (_moveAction != null)
                _moveAction.performed -= OnMovePerformed;
        }
    }
}
