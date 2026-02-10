using R3;

namespace Element.Events
{
    /// <summary>
    /// カメラ遷移イベントのインターフェース
    /// </summary>
    public interface ICameraEvents
    {
        /// <summary>カメラ遷移の開始/終了</summary>
        Observable<CameraTransitionEvent> OnCameraTransition { get; }
    }
}
