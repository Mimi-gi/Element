using R3;
using Element.Interfaces;

namespace Element.Events
{
    /// <summary>
    /// 乗り移りイベントの実装
    /// </summary>
    public class PossessionEvents : IPossessionEvents
    {
        private readonly ReactiveProperty<IPossable> _currentPossessed;
        private readonly Subject<PossessionChangeEvent> _onPossessionChanged;
        private readonly Subject<IPossable> _onPossessableDeath;

        public ReadOnlyReactiveProperty<IPossable> CurrentPossessed { get; }
        public Observable<PossessionChangeEvent> OnPossessionChanged { get; }
        public Observable<IPossable> OnPossessableDeath { get; }

        public PossessionEvents()
        {
            _currentPossessed = new ReactiveProperty<IPossable>(null);
            _onPossessionChanged = new Subject<PossessionChangeEvent>();
            _onPossessableDeath = new Subject<IPossable>();

            CurrentPossessed = _currentPossessed;
            OnPossessionChanged = _onPossessionChanged.AsObservable();
            OnPossessableDeath = _onPossessableDeath.AsObservable();
        }

        /// <summary>
        /// 現在Possessed設定（内部用）
        /// </summary>
        public void SetCurrentPossessed(IPossable target)
        {
            _currentPossessed.Value = target;
        }

        /// <summary>
        /// 乗り移り変更通知（内部用）
        /// </summary>
        public void NotifyPossessionChanged(PossessionChangeEvent evt)
        {
            _onPossessionChanged.OnNext(evt);
        }

        /// <summary>
        /// 死亡通知（内部用）
        /// </summary>
        public void NotifyDeath(IPossable deadPossessable)
        {
            _onPossessableDeath.OnNext(deadPossessable);
        }
    }
}
