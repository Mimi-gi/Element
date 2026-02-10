using UnityEngine;

namespace Element.Events
{
    /// <summary>
    /// カメラ遷移イベントのデータ
    /// </summary>
    public struct CameraTransitionEvent
    {
        /// <summary>遷移中かどうか</summary>
        public bool IsTransitioning;
        
        /// <summary>遷移先の位置</summary>
        public Vector3 TargetPosition;
        
        /// <summary>遷移にかかる時間</summary>
        public float Duration;
    }
}
