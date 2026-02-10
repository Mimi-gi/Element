using UnityEngine;
using System;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// カメラを制御するマネージャー（POCO）
    /// Fixedモード・LitMotionベースの遷移のみ
    /// </summary>
    public class CameraController : IDisposable
    {
        private readonly Camera _camera;
        private readonly CameraEvents _cameraEvents;
        private readonly CompositeDisposable _disposables = new();

        private float _defaultTransitionDuration = 0.5f;
        private Ease _transitionEase = Ease.InOutCubic;

        public CameraController(
            Camera camera,
            CameraEvents cameraEvents,
            IPossessionEvents possessionEvents,
            IStageEvents stageEvents)
        {
            _camera = camera;
            _cameraEvents = cameraEvents;

            SubscribeToEvents(possessionEvents, stageEvents);
        }

        private void SubscribeToEvents(IPossessionEvents possessionEvents, IStageEvents stageEvents)
        {
            // Possession変更時、カメラをターゲットに移動
            possessionEvents?.CurrentPossessed
                .Subscribe(possessed =>
                {
                    if (possessed != null && possessed.Core != null)
                    {
                        MoveToPositionAsync(possessed.Core.position, _defaultTransitionDuration).Forget();
                    }
                })
                .AddTo(_disposables);

            // ステージエリア変更時、カメラを移動
            stageEvents?.OnActiveAreaChanged
                .Subscribe(darkSource =>
                {
                    if (darkSource != null)
                    {
                        MoveToPositionAsync(darkSource.transform.position, _defaultTransitionDuration).Forget();
                    }
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// デフォルト遷移時間を設定
        /// </summary>
        public void SetDefaultTransitionDuration(float duration)
        {
            _defaultTransitionDuration = duration;
        }

        /// <summary>
        /// 遷移Easeを設定
        /// </summary>
        public void SetTransitionEase(Ease ease)
        {
            _transitionEase = ease;
        }

        /// <summary>
        /// 指定位置にカメラを移動（LitMotion）
        /// </summary>
        public async UniTask MoveToPositionAsync(Vector3 targetPosition, float duration)
        {
            targetPosition.z = _camera.transform.position.z;

            _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
            {
                IsTransitioning = true,
                TargetPosition = targetPosition,
                Duration = duration
            });

            await LMotion.Create(_camera.transform.position, targetPosition, duration)
                .WithEase(_transitionEase)
                .BindToPosition(_camera.transform)
                .ToUniTask();

            _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
            {
                IsTransitioning = false,
                TargetPosition = targetPosition,
                Duration = 0
            });
        }

        /// <summary>
        /// 指定位置とサイズにカメラを移動（LitMotion）
        /// </summary>
        public async UniTask MoveToPositionWithSizeAsync(Vector3 targetPosition, float targetSize, float duration)
        {
            targetPosition.z = _camera.transform.position.z;

            _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
            {
                IsTransitioning = true,
                TargetPosition = targetPosition,
                Duration = duration
            });

            var positionTask = LMotion.Create(_camera.transform.position, targetPosition, duration)
                .WithEase(_transitionEase)
                .BindToPosition(_camera.transform)
                .ToUniTask();

            UniTask sizeTask = UniTask.CompletedTask;
            if (_camera.orthographic)
            {
                sizeTask = LMotion.Create(_camera.orthographicSize, targetSize, duration)
                    .WithEase(_transitionEase)
                    .Bind(size => _camera.orthographicSize = size)
                    .ToUniTask();
            }

            await UniTask.WhenAll(positionTask, sizeTask);

            _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
            {
                IsTransitioning = false,
                TargetPosition = targetPosition,
                Duration = 0
            });
        }

        /// <summary>
        /// 即座に位置を設定（遷移なし）
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            position.z = _camera.transform.position.z;
            _camera.transform.position = position;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
