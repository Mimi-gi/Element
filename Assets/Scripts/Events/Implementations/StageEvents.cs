using R3;

namespace Element.Events
{
    /// <summary>
    /// ステージ管理イベントの実装
    /// </summary>
    public class StageEvents : IStageEvents
    {
        private readonly ReactiveProperty<DarkSource> _activeDarkSource;
        private readonly Subject<DarkSource> _onActiveAreaChanged;

        public ReadOnlyReactiveProperty<DarkSource> ActiveDarkSource { get; }
        public Observable<DarkSource> OnActiveAreaChanged { get; }

        public StageEvents()
        {
            _activeDarkSource = new ReactiveProperty<DarkSource>(null);
            _onActiveAreaChanged = new Subject<DarkSource>();

            ActiveDarkSource = _activeDarkSource;
            OnActiveAreaChanged = _onActiveAreaChanged.AsObservable();
        }

        /// <summary>
        /// アクティブDarkSource設定（内部用）
        /// </summary>
        public void SetActiveDarkSource(DarkSource source)
        {
            _activeDarkSource.Value = source;
        }

        /// <summary>
        /// エリア変更通知（内部用）
        /// </summary>
        public void NotifyAreaChanged(DarkSource source)
        {
            _onActiveAreaChanged.OnNext(source);
        }
    }
}
