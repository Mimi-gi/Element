using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// ステージとエリアを管理するマネージャー
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [Header("Dark Sources")]
        [SerializeField] private DarkSource _initialDarkSource;

        private StageEvents _stageEvents;
        private Dictionary<string, DarkSource> _darkSources = new Dictionary<string, DarkSource>();

        [Inject]
        public void Construct(StageEvents stageEvents)
        {
            _stageEvents = stageEvents;
        }

        private void Start()
        {
            if (_initialDarkSource != null)
            {
                SetActiveDarkSource(_initialDarkSource);
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
    }
}
