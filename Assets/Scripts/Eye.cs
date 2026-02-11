using UnityEngine;

namespace Element
{
    /// <summary>
    /// プレイヤーの「目」を表すコンポーネント。
    /// PossessionManagerが乗り移り時に親の付け替えを行う。
    /// </summary>
    public class Eye : MonoBehaviour
    {
        /// <summary>
        /// 指定した親の下に移動し、ローカル位置を設定する
        /// </summary>
        public void AttachTo(Transform parent, Vector2 localPos)
        {
            transform.SetParent(parent);
            transform.localPosition = localPos;
        }
    }
}
