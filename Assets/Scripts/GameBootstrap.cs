using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using VContainer;
using Element.Managers;

namespace Element
{
    /// <summary>
    /// ゲーム起動時の初期化を担当（POCO）
    /// </summary>
    public class GameBootstrap : IDisposable
    {
        private readonly StageManager _stageManager;
        private readonly PossessionManager _possessionManager;
        private readonly CameraController _cameraController;
        private readonly GameStateManager _gameStateManager;
        private readonly IObjectResolver _resolver;
        private readonly GameObject _darkPrefab;

        public GameBootstrap(
            StageManager stageManager,
            PossessionManager possessionManager,
            CameraController cameraController,
            GameStateManager gameStateManager,
            IObjectResolver resolver,
            DarkSource[] darkSources,
            GameObject darkPrefab)
        {
            _stageManager = stageManager;
            _possessionManager = possessionManager;
            _cameraController = cameraController;
            _gameStateManager = gameStateManager;
            _resolver = resolver;
            _darkPrefab = darkPrefab;

            // DarkSourceを登録
            if (darkSources != null && darkSources.Length > 0)
            {
                _stageManager.RegisterDarkSources(darkSources, darkSources[0]);
            }
            else
            {
                Debug.LogWarning("No DarkSources provided to GameBootstrap!");
            }
        }

        /// <summary>
        /// ゲーム初期化（非同期）
        /// </summary>
        public async UniTask InitializeGameAsync()
        {
            // StageManagerの初期化を確認
            if (_stageManager == null)
            {
                Debug.LogError("StageManager is null! Check VContainer setup.");
                return;
            }

            // 1. アクティブなDarkSourceを取得
            var activeDarkSource = _stageManager.GetCurrentDarkSource();
            if (activeDarkSource == null)
            {
                Debug.LogError("No active DarkSource found! Make sure:");
                Debug.LogError("- DarkSource exists in the scene");
                Debug.LogError("- DarkSource has a valid AreaId");
                return;
            }

            Debug.Log($"Using DarkSource: {activeDarkSource.name} at {activeDarkSource.SpawnPosition}");

            // 2. 初期Darkをスポーン
            Vector3 spawnPos = activeDarkSource.SpawnPosition;
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

            // 少し待機（オブジェクト初期化のため）
            await UniTask.Delay(100);

            // 3. 初期Possessionを設定
            if (_possessionManager != null)
            {
                await _possessionManager.PossessTo(dark);
            }

            // 4. カメラ初期位置を設定
            if (dark.Core != null)
            {
                _cameraController?.SetPosition(dark.Core.position);
            }

            // 5. ゲーム状態をPlayingに
            _gameStateManager?.ChangeState(Element.Enums.GameState.Playing);

            Debug.Log("Game initialized successfully!");
        }

        public void Dispose()
        {
            // 必要に応じてクリーンアップ
        }
    }
}
