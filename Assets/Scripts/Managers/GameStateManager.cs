using UnityEngine;
using System;
using R3;
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
        private readonly CompositeDisposable _disposables = new();

        public GameStateManager(GameStateEvents gameStateEvents, IFocusEvents focusEvents)
        {
            _gameStateEvents = gameStateEvents;

            // フォーカスモード連動
            focusEvents?.OnFocusModeChanged
                .Subscribe(isFocusing =>
                {
                    if (isFocusing)
                        ChangeState(GameState.FocusMode);
                    else
                        ChangeState(GameState.Playing);
                })
                .AddTo(_disposables);
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
            _disposables?.Dispose();
        }
    }
}

