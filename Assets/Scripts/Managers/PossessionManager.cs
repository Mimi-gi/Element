using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Linq;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Element.Interfaces;
using Element.Events;
using VContainer;
using Element;

namespace Element.Managers
{
    /// <summary>
    /// 乗り移りシステムを管理するマネージャー（POCO）
    /// </summary>
    public class PossessionManager : IDisposable
    {
        private readonly PossessionEvents _possessionEvents;
        private readonly IStageEvents _stageEvents;
        private readonly IGameStateEvents _gameStateEvents;
        private readonly IFocusEvents _focusEvents;
        private readonly InputProcessor _inputProcessor;
        private readonly IObjectResolver _resolver;
        private readonly FocusCircle _focusCircle;
        private readonly GameObject _darkPrefab;
        private readonly Eye _eyePrefab;
        private readonly VisualEffect _possessionVfx;
        private readonly CompositeDisposable _disposables = new();

        private Eye _currentEye;
        private bool _isFocusMode;

        private const float VFX_MOVE_DURATION = 0.3f;
        private static readonly Ease VFX_MOVE_EASE = Ease.InOutCubic;

        public PossessionManager(
            PossessionEvents possessionEvents,
            IStageEvents stageEvents,
            IGameStateEvents gameStateEvents,
            IFocusEvents focusEvents,
            InputProcessor inputProcessor,
            IObjectResolver resolver,
            FocusCircle focusCircle,
            GameObject darkPrefab,
            Eye eye,
            VisualEffect possessionVfx)
        {
            _possessionEvents = possessionEvents;
            _stageEvents = stageEvents;
            _gameStateEvents = gameStateEvents;
            _focusEvents = focusEvents;
            _inputProcessor = inputProcessor;
            _resolver = resolver;
            _focusCircle = focusCircle;
            _darkPrefab = darkPrefab;
            _eyePrefab = eye;
            _possessionVfx = possessionVfx;

            // 初期状態は非アクティブ
            _focusCircle?.Deactivate();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            Debug.Log("SubscribeToEvents");
            // Move入力 → 現在の憑依先にルーティング
            _inputProcessor?.Move
                .Subscribe(moveInput =>
                {
                    var current = _possessionEvents?.CurrentPossessed.CurrentValue;
                    if (current == null || !current.IsPossess) return;

                    if (current is MonoBehaviour mb)
                    {
                        var mover = mb.GetComponent<IHorizontalMove>();
                        if (mover != null)
                        {
                            mover.XMove(moveInput.x);
                            Debug.Log($"XMove called on {mb.name}, direction: {moveInput.x}");
                        }
                        else
                        {
                            Debug.Log($"IHorizontalMove not found on {mb.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Current possessed is not MonoBehaviour: {current}");
                    }
                })
                .AddTo(_disposables);

            // 乗り移り入力購読
            _inputProcessor?.PossessInput
                .Subscribe(_ => OnPossessInput())
                .AddTo(_disposables);

            // フォーカスモード状態追跡 + FocusCircle制御
            _focusEvents?.OnFocusModeChanged
                .Subscribe(isFocusing =>
                {
                    _isFocusMode = isFocusing;

                    if (isFocusing)
                    {
                        // 現在の憑依先の位置にFocusCircleを有効化
                        var current = _possessionEvents?.CurrentPossessed.CurrentValue;
                        if (current?.Core != null)
                        {
                            _focusCircle?.Activate(current.Core.position);
                        }
                    }
                    else
                    {
                        _focusCircle?.Deactivate();
                    }
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// 乗り移り入力処理
        /// </summary>
        private void OnPossessInput()
        {
            if (!_isFocusMode) return; // フォーカス中のみ

            var currentPossessed = _possessionEvents?.CurrentPossessed.CurrentValue;
            if (currentPossessed == null) return;

            // FocusCircleの候補から最も近いターゲットを検索
            IPossable nearestTarget = _focusCircle?.GetNearest(currentPossessed);

            if (nearestTarget != null)
            {
                PossessTo(nearestTarget).Forget();
                Debug.Log("Possessed to: " + nearestTarget);
            }
            else
            {
                Debug.Log("No valid possession target found nearby.");
            }
        }

        /// <summary>
        /// 指定したIPossableに乗り移る
        /// </summary>
        public async UniTask PossessTo(IPossable target)
        {
            if (target == null)
            {
                Debug.LogWarning("PossessTo: target is null");
                return;
            }

            var oldPossessed = _possessionEvents.CurrentPossessed.CurrentValue;

            // 通常の乗り移り時のみVFXを再生（Darkへの乗り移り＝リスポーン時は除く）
            bool shouldPlayVfx = oldPossessed != null && !(target is Dark) && _possessionVfx != null;

            if (shouldPlayVfx)
            {
                // VFX開始位置（前のIPossableのEyePos）
                Vector3 startPos = oldPossessed.Core.position + (Vector3)oldPossessed.EyePos;
                Vector3 endPos = ((MonoBehaviour)target).transform.position + (Vector3)target.EyePos;

                _possessionVfx.transform.position = startPos;
                _possessionVfx.gameObject.SetActive(true);
                _possessionVfx.Play();

                // LitMotionで座標移動
                await LMotion.Create(startPos, endPos, VFX_MOVE_DURATION)
                    .WithEase(VFX_MOVE_EASE)
                    .BindToPosition(_possessionVfx.transform)
                    .ToUniTask();

                _possessionVfx.Stop();
                _possessionVfx.gameObject.SetActive(false);
            }

            // 古いオブジェクトの処理
            if (oldPossessed != null)
            {
                oldPossessed.IsPossess = false;

                // 物理設定を更新（Unpossess時）
                if (oldPossessed is MonoBehaviour oldMb)
                {
                    oldPossessed.TryPossess(); // UpdatePhysicsSettingsを呼ぶため
                }

                // Darkなら破棄
                if (oldPossessed is Dark dark)
                {
                    UnityEngine.Object.Destroy(dark.gameObject);
                }
            }

            // 新しいオブジェクトに乗り移る
            target.IsPossess = true;
            target.TryPossess(); // これでUpdatePhysicsSettingsが呼ばれる

            // Eyeを新しいIPossableに配置
            if (target is MonoBehaviour targetMb)
            {
                if (target is Dark)
                {
                    // Dark（ゲーム開始・リスポーン）: 古いEyeを破棄して新規生成
                    if (_currentEye != null)
                    {
                        UnityEngine.Object.Destroy(_currentEye.gameObject);
                    }

                    if (_eyePrefab != null)
                    {
                        _currentEye = UnityEngine.Object.Instantiate(_eyePrefab, targetMb.transform);
                        _currentEye.transform.localPosition = target.EyePos;
                    }
                }
                else
                {
                    // 通常の乗り移り: 既存Eyeを移動
                    _currentEye?.AttachTo(targetMb.transform, target.EyePos);
                }
            }

            // イベント発行
            _possessionEvents.SetCurrentPossessed(target);
            _possessionEvents.NotifyPossessionChanged(new PossessionChangeEvent
            {
                OldPossessed = oldPossessed,
                NewPossessed = target
            });

            Debug.Log($"Possessed: {target.GetType().Name} (Layer: {target.Layer})");
        }

        /// <summary>
        /// IPossableが死亡した時の処理
        /// </summary>
        public void OnPossessableDeath(IPossable deadPossessable)
        {
            if (deadPossessable == null) return;

            var currentPossessed = _possessionEvents.CurrentPossessed.CurrentValue;

            if (deadPossessable == currentPossessed)
            {
                Debug.Log("Current possessed object died. Respawning as Dark.");
                RespawnAsDark().Forget();
            }

            _possessionEvents.NotifyDeath(deadPossessable);
        }

        /// <summary>
        /// Darkとしてリスポーン
        /// </summary>
        private async UniTaskVoid RespawnAsDark()
        {
            var darkSource = _stageEvents?.ActiveDarkSource.CurrentValue;

            if (darkSource == null)
            {
                Debug.LogError("No active DarkSource! Cannot respawn.");
                return;
            }

            if (_darkPrefab == null)
            {
                Debug.LogError("Dark prefab is not assigned!");
                return;
            }

            await UniTask.Delay(100);

            Vector3 spawnPos = darkSource.SpawnPosition;
            GameObject darkObj = UnityEngine.Object.Instantiate(_darkPrefab, spawnPos, Quaternion.identity);
            Dark dark = darkObj.GetComponent<Dark>();

            if (dark == null)
            {
                Debug.LogError("Dark component not found on prefab!");
                UnityEngine.Object.Destroy(darkObj);
                return;
            }

            // VContainer注入
            _resolver.Inject(dark);

            await PossessTo(dark);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
