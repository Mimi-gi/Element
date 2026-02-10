namespace Element.Enums
{
    /// <summary>
    /// ゲーム全体の状態
    /// </summary>
    public enum GameState
    {
        /// <summary>通常プレイ中</summary>
        Playing,
        
        /// <summary>フォーカスモード中</summary>
        FocusMode,
        
        /// <summary>カメラ遷移中</summary>
        Transition,
        
        /// <summary>ポーズ中</summary>
        Paused,
        
        /// <summary>メニュー表示中</summary>
        Menu,
        
        /// <summary>ゲームオーバー</summary>
        GameOver
    }
}
