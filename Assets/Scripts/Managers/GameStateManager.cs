using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Element.Enums;
using Element.Events;

namespace Element.Managers
{
    /// <summary>
    /// ゲーム状態を管理するマネージャー（POCO）
    /// </summary>
    public class GameStateManager : IDisposable
    {
        private readonly GameStateEvents _gameStateEvents;

        public GameStateManager(GameStateEvents gameStateEvents)
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
        /// ゲーム状態を変更（async/await可能、演出待ち付き）
        /// </summary>
        public async UniTask ChangeStateAsync(GameState newState, float waitDuration = 0f)
        {
            _gameStateEvents.ChangeState(newState);
            Debug.Log($"Game state changed to: {newState}");

            // 演出待ち
            if (waitDuration > 0)
            {
                await UniTask.Delay((int)(waitDuration * 1000));
            }
        }

        /// <summary>
        /// 現在の状態を取得
        /// </summary>
        public GameState GetCurrentState()
        {
            return _gameStateEvents.CurrentGameState.CurrentValue;
        }

        public void Dispose()
        {
            // 必要に応じてクリーンアップ
        }
    }
}

