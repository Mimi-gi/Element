using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// ステージとエリアを管理するマネージャー（POCO）
    /// </summary>
    public class StageManager : IDisposable
    {
        private readonly StageEvents _stageEvents;
        private readonly Dictionary<string, DarkSource> _darkSources = new();
        private DarkSource _initialDarkSource;

        public StageManager(StageEvents stageEvents)
        {
            _stageEvents = stageEvents;
        }

        /// <summary>
        /// DarkSourceの配列を登録
        /// </summary>
        public void RegisterDarkSources(DarkSource[] sources, DarkSource initialSource = null)
        {
            foreach (var source in sources)
            {
                RegisterDarkSource(source.AreaId, source);
                Debug.Log($"Registered DarkSource: {source.name} with ID: {source.AreaId}");
            }

            // 初期DarkSourceを設定
            if (initialSource != null)
            {
                _initialDarkSource = initialSource;
                SetActiveDarkSource(initialSource);
            }
            else if (_darkSources.Count > 0)
            {
                // 初期DarkSourceが指定されていない場合、最初に見つかったものを使用
                var firstSource = new List<DarkSource>(_darkSources.Values)[0];
                _initialDarkSource = firstSource;
                SetActiveDarkSource(firstSource);
                Debug.LogWarning($"No initial DarkSource set, using first found: {firstSource.name}");
            }
        }

        /// <summary>
        /// アクティブなDarkSourceを設定
        /// </summary>
        public void SetActiveDarkSource(DarkSource source)
        {
            if (source == null)
            {
                Debug.LogWarning("Trying to set null DarkSource");
                return;
            }

            _stageEvents.SetActiveDarkSource(source);
            _stageEvents.NotifyAreaChanged(source);

            Debug.Log($"Active DarkSource set to: {source.name}");
        }

        /// <summary>
        /// DarkSourceを登録
        /// </summary>
        public void RegisterDarkSource(string id, DarkSource source)
        {
            if (_darkSources.ContainsKey(id))
            {
                Debug.LogWarning($"DarkSource with id '{id}' already registered");
                return;
            }

            _darkSources[id] = source;
        }

        /// <summary>
        /// IDからDarkSourceを取得
        /// </summary>
        public DarkSource GetDarkSource(string id)
        {
            if (_darkSources.TryGetValue(id, out DarkSource source))
            {
                return source;
            }

            Debug.LogWarning($"DarkSource with id '{id}' not found");
            return null;
        }

        /// <summary>
        /// エリアに遷移
        /// </summary>
        public void TransitionToArea(string areaId)
        {
            var darkSource = GetDarkSource(areaId);
            if (darkSource != null)
            {
                SetActiveDarkSource(darkSource);
            }
        }

        /// <summary>
        /// 現在アクティブなDarkSourceを取得
        /// </summary>
        public DarkSource GetCurrentDarkSource()
        {
            return _stageEvents?.ActiveDarkSource.CurrentValue;
        }

        /// <summary>
        /// エリアに遷移（async/await可能）
        /// </summary>
        public async UniTask TransitionToAreaAsync(string areaId, float waitDuration = 0f)
        {
            var darkSource = GetDarkSource(areaId);
            if (darkSource != null)
            {
                SetActiveDarkSource(darkSource);

                // 演出待ち
                if (waitDuration > 0)
                {
                    await UniTask.Delay((int)(waitDuration * 1000));
                }
            }
        }

        public void Dispose()
        {
            // 必要に応じてクリーンアップ
        }
    }
}

