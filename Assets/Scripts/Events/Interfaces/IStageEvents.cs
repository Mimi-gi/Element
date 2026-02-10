using R3;

namespace Element.Events
{
    /// <summary>
    /// ステージ管理イベントのインターフェース
    /// </summary>
    public interface IStageEvents
    {
        /// <summary>現在アクティブなDarkSource（読み取り専用）</summary>
        ReadOnlyReactiveProperty<DarkSource> ActiveDarkSource { get; }

        /// <summary>アクティブなエリアが変更された時</summary>
        Observable<DarkSource> OnActiveAreaChanged { get; }
    }
}
