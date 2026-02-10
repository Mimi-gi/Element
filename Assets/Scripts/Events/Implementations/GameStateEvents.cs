using R3;
using Element.Enums;

namespace Element.Events
{
    /// <summary>
    /// ゲーム状態イベントの実装
    /// </summary>
    public class GameStateEvents : IGameStateEvents
    {
        private readonly ReactiveProperty<GameState> _currentGameState;

        public ReadOnlyReactiveProperty<GameState> CurrentGameState { get; }

        public GameStateEvents()
        {
            _currentGameState = new ReactiveProperty<GameState>(GameState.Playing);
            CurrentGameState = _currentGameState;
        }

        /// <summary>
        /// 状態を変更（内部用）
        /// </summary>
        public void ChangeState(GameState newState)
        {
            _currentGameState.Value = newState;
        }
    }
}
