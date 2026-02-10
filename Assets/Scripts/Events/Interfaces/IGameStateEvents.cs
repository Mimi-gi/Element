using R3;
using Element.Enums;

namespace Element.Events
{
    /// <summary>
    /// ゲーム状態イベントのインターフェース
    /// </summary>
    public interface IGameStateEvents
    {
        /// <summary>現在のゲーム状態（読み取り専用）</summary>
        ReadOnlyReactiveProperty<GameState> CurrentGameState { get; }
    }
}
