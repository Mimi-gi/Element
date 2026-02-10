using R3;

namespace Element.Events
{
    /// <summary>
    /// カメラ遷移イベントの実装
    /// </summary>
    public class CameraEvents : ICameraEvents
    {
        private readonly Subject<CameraTransitionEvent> _onCameraTransition;

        public Observable<CameraTransitionEvent> OnCameraTransition { get; }

        public CameraEvents()
        {
            _onCameraTransition = new Subject<CameraTransitionEvent>();
            OnCameraTransition = _onCameraTransition.AsObservable();
        }

        /// <summary>
        /// カメラ遷移通知（内部用）
        /// </summary>
        public void NotifyCameraTransition(CameraTransitionEvent evt)
        {
            _onCameraTransition.OnNext(evt);
        }
    }
}
