using R3;

namespace Element.Events
{
    /// <summary>
    /// フォーカスモードイベントのインターフェース
    /// </summary>
    public interface IFocusEvents
    {
        /// <summary>フォーカスモードの開始/終了</summary>
        Observable<bool> OnFocusModeChanged { get; }
    }
}
