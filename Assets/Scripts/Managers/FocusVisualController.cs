using UnityEngine;
using UnityEngine.Rendering;
using System;
using R3;
using LitMotion;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// フォーカスモードのVolume演出を制御するマネージャー（POCO）
    /// </summary>
    public class FocusVisualController : IDisposable
    {
        private readonly Volume _focusVolume;
        private readonly CompositeDisposable _disposables = new();

        private float _transitionDuration = 0.3f;
        private MotionHandle _currentMotion;

        public FocusVisualController(Volume focusVolume, IFocusEvents focusEvents)
        {
            _focusVolume = focusVolume;

            if (_focusVolume != null)
            {
                _focusVolume.weight = 0f;
            }

            focusEvents?.OnFocusModeChanged
                .Subscribe(isFocusing =>
                {
                    TransitionVolume(isFocusing ? 1f : 0f);
                })
                .AddTo(_disposables);
        }

        private void TransitionVolume(float targetWeight)
        {
            if (_focusVolume == null) return;

            // 実行中のモーションをキャンセル
            if (_currentMotion.IsActive())
            {
                _currentMotion.Cancel();
            }

            _currentMotion = LMotion.Create(_focusVolume.weight, targetWeight, _transitionDuration)
                .WithEase(Ease.InOutCubic)
                .Bind(w => _focusVolume.weight = w);
        }

        public void SetTransitionDuration(float duration)
        {
            _transitionDuration = duration;
        }

        public void Dispose()
        {
            if (_currentMotion.IsActive())
            {
                _currentMotion.Cancel();
            }
            _disposables?.Dispose();
        }
    }
}
