using UnityEngine;

namespace Element.Interfaces
{
    /// <summary>
    /// 乗り移り可能なオブジェクトが実装するインターフェース
    /// </summary>
    public interface IPossable
    {
        /// <summary>
        /// 乗り移り入力が発生した時に呼ばれる
        /// </summary>
        void TryPossess();

        /// <summary>
        /// オブジェクトの描画順序を示すレイヤー値
        /// 値が大きいほど手前に描画される
        /// </summary>
        int Layer { get; }

        /// <summary>
        /// 現在プレイヤーに乗り移られているかどうか
        /// </summary>
        bool IsPossess { get; set; }

        /// <summary>
        /// オブジェクトのコア（乗り移りの中心点）
        /// 視認性判定や距離計算に使用される
        /// </summary>
        Transform Core { get; }

        /// <summary>
        /// オブジェクトの死亡処理
        /// 乗り移り中でなくても呼ばれる可能性がある
        /// </summary>
        void Death();
    }
}
