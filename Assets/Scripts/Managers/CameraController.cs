using UnityEngine;
using VContainer;
using R3;
using LitMotion;
using LitMotion.Extensions;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// カメラを制御するマネージャー
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;

        [Header("Follow Settings")]
        [SerializeField] private bool _followTarget = true;
        [SerializeField] private float _followSpeed = 5f;

        private CameraEvents _cameraEvents;
        private IPossessionEvents _possessionEvents;
        private Transform _targetTransform;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(CameraEvents cameraEvents, IPossessionEvents possessionEvents)
        {
            _cameraEvents = cameraEvents;
            _possessionEvents = possessionEvents;
        }

        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            // Current Possessedを追従
            _possessionEvents?.CurrentPossessed
                .Subscribe(possessed =>
                {
                    if (possessed != null && possessed.Core != null)
                    {
                        FollowTarget(possessed.Core);
                    }
                })
                .AddTo(_disposables);
        }

        private void LateUpdate()
        {
            if (_followTarget && _targetTransform != null)
            {
                Vector3 targetPos = _targetTransform.position;
                targetPos.z = _camera.transform.position.z;

                _camera.transform.position = Vector3.Lerp(
                    _camera.transform.position,
                    targetPos,
                    Time.deltaTime * _followSpeed
                );
            }
        }

        /// <summary>
        /// 指定エリアにカメラを移動
        /// </summary>
        public void MoveToArea(Transform targetTransform, float targetSize, float duration)
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("Target transform is null");
                return;
            }

            // 遷移開始イベント
            _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
            {
                IsTransitioning = true,
                TargetPosition = targetTransform.position,
                Duration = duration
            });

            Vector3 targetPos = targetTransform.position;
            targetPos.z = _camera.transform.position.z;

            LMotion.Create(_camera.transform.position, targetPos, duration)
                .WithEase(Ease.InOutCubic)
                .WithOnComplete(() =>
                {
                    _cameraEvents?.NotifyCameraTransition(new CameraTransitionEvent
                    {
                        IsTransitioning = false,
                        TargetPosition = targetPos,
                        Duration = 0
                    });
                })
                .BindToPosition(_camera.transform);

            if (_camera.orthographic)
            {
                LMotion.Create(_camera.orthographicSize, targetSize, duration)
                    .WithEase(Ease.InOutCubic)
                    .Bind(size => _camera.orthographicSize = size);
            }
        }

        /// <summary>
        /// カメラがターゲットを追従するように設定
        /// </summary>
        public void FollowTarget(Transform target)
        {
            _targetTransform = target;
            _followTarget = true;
        }

        /// <summary>
        /// カメラの追従を停止
        /// </summary>
        public void StopFollowing()
        {
            _followTarget = false;
            _targetTransform = null;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
