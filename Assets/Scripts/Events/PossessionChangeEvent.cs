using Element.Interfaces;

namespace Element.Events
{
    /// <summary>
    /// 乗り移り変更イベントのデータ
    /// </summary>
    public struct PossessionChangeEvent
    {
        /// <summary>乗り移り前のオブジェクト</summary>
        public IPossable OldPossessed;
        
        /// <summary>乗り移り後のオブジェクト</summary>
        public IPossable NewPossessed;
    }
}
