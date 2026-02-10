using UnityEngine;
using System;
using R3;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// 時間倍率を制御するマネージャー（POCO）
    /// </summary>
    public class TimeManager : IDisposable
    {
        private readonly IFocusEvents _focusEvents;
        private readonly ICameraEvents _cameraEvents;
        private readonly CompositeDisposable _disposables = new();

        private float _defaultTimeScale = 1.0f;
        private float _focusModeScale = 0.3f;

        public TimeManager(IFocusEvents focusEvents, ICameraEvents cameraEvents)
        {
            _focusEvents = focusEvents;
            _cameraEvents = cameraEvents;

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

        public void SetDefaultTimeScale(float scale)
        {
            _defaultTimeScale = scale;
        }

        public void SetFocusModeScale(float scale)
        {
            _focusModeScale = scale;
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

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
