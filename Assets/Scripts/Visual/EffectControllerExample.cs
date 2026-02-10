using UnityEngine;
using Element.Visual;
using R3;

namespace Element.Examples
{
    /// <summary>
    /// エフェクトコントローラーの使用例
    /// </summary>
    public class EffectControllerExample : MonoBehaviour
    {
        [Header("Burst Effects")]
        [SerializeField] private BurstEffectController _hitEffect;
        [SerializeField] private BurstEffectController _deathEffect;

        [Header("Continuous Effects")]
        [SerializeField] private ContinuousEffectController _auraEffect;
        [SerializeField] private ContinuousEffectController _shieldEffect;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            // BurstエフェクトのObservableを購読
            if (_hitEffect != null)
            {
                _hitEffect.OnComplete
                    .Subscribe(_ => Debug.Log("Hit effect completed!"))
                    .AddTo(_disposables);
            }

            if (_deathEffect != null)
            {
                _deathEffect.OnComplete
                    .Subscribe(_ => OnDeathEffectComplete())
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// ヒットエフェクト再生例
        /// </summary>
        public void PlayHitEffect()
        {
            _hitEffect?.PlayAll();
        }

        /// <summary>
        /// 死亡エフェクト再生例
        /// </summary>
        public void PlayDeathEffect()
        {
            _deathEffect?.PlayAll();
        }

        /// <summary>
        /// オーラエフェクト切り替え例
        /// </summary>
        public void ToggleAura()
        {
            _auraEffect?.Toggle();
        }

        /// <summary>
        /// シールドエフェクト制御例
        /// </summary>
        public void SetShieldActive(bool active)
        {
            _shieldEffect?.SetActive(active);
        }

        /// <summary>
        /// シールド強度調整例
        /// </summary>
        public void UpdateShieldStrength(float strength)
        {
            _shieldEffect?.SetIntensity(strength);
        }

        /// <summary>
        /// 死亡エフェクト完了コールバック
        /// </summary>
        private void OnDeathEffectComplete()
        {
            Debug.Log("Death effect completed!");
            // オブジェクト破棄などの処理
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
