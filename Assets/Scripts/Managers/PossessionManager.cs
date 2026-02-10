using UnityEngine;
using System;
using System.Linq;
using R3;
using Cysharp.Threading.Tasks;
using Element.Interfaces;
using Element.Events;
using VContainer;

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
        private readonly InputProcessor _inputProcessor;
        private readonly IObjectResolver _resolver;
        private readonly GameObject _darkPrefab;
        private readonly CompositeDisposable _disposables = new();

        private float _maxPossessionDistance = 10f;

        public PossessionManager(
            PossessionEvents possessionEvents,
            IStageEvents stageEvents,
            IGameStateEvents gameStateEvents,
            InputProcessor inputProcessor,
            IObjectResolver resolver,
            GameObject darkPrefab)
        {
            _possessionEvents = possessionEvents;
            _stageEvents = stageEvents;
            _gameStateEvents = gameStateEvents;
            _inputProcessor = inputProcessor;
            _resolver = resolver;
            _darkPrefab = darkPrefab;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // 乗り移り入力購読
            _inputProcessor?.PossessInput
                .Subscribe(_ => OnPossessInput())
                .AddTo(_disposables);
        }

        public void SetMaxPossessionDistance(float distance)
        {
            _maxPossessionDistance = distance;
        }

        /// <summary>
        /// 乗り移り入力処理
        /// </summary>
        private void OnPossessInput()
        {
            var currentPossessed = _possessionEvents?.CurrentPossessed.CurrentValue;
            if (currentPossessed == null) return;

            // 最も近いターゲットを検索
            IPossable nearestTarget = FindNearestPossessable(currentPossessed);

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
        /// 最も近い乗り移り可能オブジェクトを検索
        /// </summary>
        private IPossable FindNearestPossessable(IPossable current)
        {
            if (current?.Core == null) return null;

            // シーン内のすべてのIPossableを検索
            var allPossessables = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IPossable>()
                .Where(p => p != current) // 自分自身を除外
                .Where(p => p.Layer < current.Layer) // 自分より低いレイヤーのみ
                .Where(p => p.Core != null); // Coreが存在するもののみ

            IPossable nearest = null;
            float minDistance = _maxPossessionDistance;

            foreach (var possessable in allPossessables)
            {
                float distance = Vector3.Distance(current.Core.position, possessable.Core.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = possessable;
                }
            }

            if (nearest != null)
            {
                Debug.Log($"Found nearest target at distance: {minDistance:F2}");
            }

            return nearest;
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
