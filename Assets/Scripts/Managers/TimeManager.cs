using UnityEngine;
using VContainer;
using R3;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// 時間倍率を制御するマネージャー
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float _defaultTimeScale = 1.0f;
        [SerializeField] private float _focusModeScale = 0.3f;

        private IFocusEvents _focusEvents;
        private ICameraEvents _cameraEvents;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(IFocusEvents focusEvents, ICameraEvents cameraEvents)
        {
            _focusEvents = focusEvents;
            _cameraEvents = cameraEvents;
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // フォーカスモード
            _focusEvents?.OnFocusModeChanged
                .Subscribe(isFocusing =>
                {
                    if (isFocusing)
                        EnterFocusMode();
                    else
                        ExitFocusMode();
                })
                .AddTo(_disposables);

            // カメラ遷移
            _cameraEvents?.OnCameraTransition
                .Subscribe(evt =>
                {
                    if (evt.IsTransitioning)
                        PauseGame();
                    else
                        ResumeGame();
                })
                .AddTo(_disposables);
        }

        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
        }

        public void EnterFocusMode()
        {
            SetTimeScale(_focusModeScale);
        }

        public void ExitFocusMode()
        {
            SetTimeScale(_defaultTimeScale);
        }

        public void PauseGame()
        {
            SetTimeScale(0f);
        }

        public void ResumeGame()
        {
            SetTimeScale(_defaultTimeScale);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
