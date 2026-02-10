using UnityEngine;
using System.Linq;
using VContainer;
using R3;
using Cysharp.Threading.Tasks;
using Element.Interfaces;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// 乗り移りシステムを管理するマネージャー
    /// </summary>
    public class PossessionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _darkPrefab;

        private PossessionEvents _possessionEvents;
        private IStageEvents _stageEvents;
        private IGameStateEvents _gameStateEvents;
        private InputProcessor _inputProcessor;

        private IPossable _optimalTarget;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(
            PossessionEvents possessionEvents,
            IStageEvents stageEvents,
            IGameStateEvents gameStateEvents,
            InputProcessor inputProcessor)
        {
            _possessionEvents = possessionEvents;
            _stageEvents = stageEvents;
            _gameStateEvents = gameStateEvents;
            _inputProcessor = inputProcessor;
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // 乗り移り入力購読
            _inputProcessor?.PossessInput
                .Subscribe(_ => ExecutePossession())
                .AddTo(_disposables);

            // TODO: フォーカスモード中の最適ターゲット更新
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

                // Darkなら破棄
                if (oldPossessed is Dark dark)
                {
                    Destroy(dark.gameObject);
                }
            }

            // 新しいオブジェクトに乗り移る
            target.IsPossess = true;
            target.TryPossess();

            // イベント発行
            _possessionEvents.SetCurrentPossessed(target);
            _possessionEvents.NotifyPossessionChanged(new PossessionChangeEvent
            {
                OldPossessed = oldPossessed,
                NewPossessed = target
            });

            Debug.Log($"Possessed: {target.GetType().Name}");
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
            GameObject darkObj = Instantiate(_darkPrefab, spawnPos, Quaternion.identity);
            Dark dark = darkObj.GetComponent<Dark>();

            if (dark == null)
            {
                Debug.LogError("Dark component not found on prefab!");
                Destroy(darkObj);
                return;
            }

            PossessTo(dark);
        }

        private void ExecutePossession()
        {
            if (_optimalTarget != null)
            {
                PossessTo(_optimalTarget);
                _optimalTarget = null;
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
