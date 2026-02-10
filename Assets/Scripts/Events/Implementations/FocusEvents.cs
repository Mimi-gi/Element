using R3;

namespace Element.Events
{
    /// <summary>
    /// フォーカスモードイベントの実装
    /// </summary>
    public class FocusEvents : IFocusEvents
    {
        private readonly Subject<bool> _onFocusModeChanged;

        public Observable<bool> OnFocusModeChanged { get; }

        public FocusEvents()
        {
            _onFocusModeChanged = new Subject<bool>();
            OnFocusModeChanged = _onFocusModeChanged.AsObservable();
        }

        /// <summary>
        /// フォーカスモード変更通知（内部用）
        /// </summary>
        public void NotifyFocusModeChanged(bool isActive)
        {
            _onFocusModeChanged.OnNext(isActive);
        }
    }
}
