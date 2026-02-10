using UnityEngine;
using System;
using System.Linq;
using R3;
using Cysharp.Threading.Tasks;
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
        private readonly CompositeDisposable _disposables = new();

        private bool _isFocusMode;

        public PossessionManager(
            PossessionEvents possessionEvents,
            IStageEvents stageEvents,
            IGameStateEvents gameStateEvents,
            IFocusEvents focusEvents,
            InputProcessor inputProcessor,
            IObjectResolver resolver,
            FocusCircle focusCircle,
            GameObject darkPrefab)
        {
            _possessionEvents = possessionEvents;
            _stageEvents = stageEvents;
            _gameStateEvents = gameStateEvents;
            _focusEvents = focusEvents;
            _inputProcessor = inputProcessor;
            _resolver = resolver;
            _focusCircle = focusCircle;
            _darkPrefab = darkPrefab;

            // 初期状態は非アクティブ
            _focusCircle?.Deactivate();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
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
                PossessTo(nearestTarget);
            }
            else
            {
                Debug.Log("No valid possession target found nearby.");
            }
        }

        /// <summary>
        /// 指定したIPossableに乗り移る
        /// </summary>
        public void PossessTo(IPossable target)
        {
            if (target == null)
            {
                Debug.LogWarning("PossessTo: target is null");
                return;
            }

            var oldPossessed = _possessionEvents.CurrentPossessed.CurrentValue;

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

            PossessTo(dark);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
