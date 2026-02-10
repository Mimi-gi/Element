using R3;
using Element.Interfaces;

namespace Element.Events
{
    /// <summary>
    /// 乗り移りイベントのインターフェース
    /// </summary>
    public interface IPossessionEvents
    {
        /// <summary>現在Possess中のIPossable（読み取り専用）</summary>
        ReadOnlyReactiveProperty<IPossable> CurrentPossessed { get; }

        /// <summary>乗り移りが発生した時</summary>
        Observable<PossessionChangeEvent> OnPossessionChanged { get; }

        /// <summary>IPossableが死亡した時</summary>
        Observable<IPossable> OnPossessableDeath { get; }
    }
}
