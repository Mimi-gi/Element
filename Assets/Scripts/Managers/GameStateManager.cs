using UnityEngine;
using VContainer;
using Element.Enums;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// ゲーム状態を管理するマネージャー
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private GameStateEvents _gameStateEvents;

        [Inject]
        public void Construct(GameStateEvents gameStateEvents)
        {
            _gameStateEvents = gameStateEvents;
        }

        /// <summary>
        /// ゲーム状態を変更
        /// </summary>
        public void ChangeState(GameState newState)
        {
            _gameStateEvents.ChangeState(newState);
            Debug.Log($"Game state changed to: {newState}");
        }

        /// <summary>
        /// 現在の状態を取得
        /// </summary>
        public GameState GetCurrentState()
        {
            return _gameStateEvents.CurrentGameState.CurrentValue;
        }
    }
}
