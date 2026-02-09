using UnityEngine;

namespace Element.Interfaces
{
    /// <summary>
    /// 二つに分かれることができるオブジェクトのインターフェース
    /// IPossableを継承し、分離機能を追加する
    /// </summary>
    public interface IDoublable : IPossable
    {
        /// <summary>
        /// オブジェクトが現在分離状態かどうか
        /// </summary>
        bool IsSplit { get; }

        /// <summary>
        /// オブジェクトを二つに分離する
        /// </summary>
        void Split();

        /// <summary>
        /// 分離されたオブジェクトを統合する
        /// </summary>
        void Merge();

        /// <summary>
        /// 分離された場合のもう一方のIPossableを取得
        /// 分離していない場合はnullを返す
        /// </summary>
        IPossable GetOtherHalf();
    }
}
