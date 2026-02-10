using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using R3;

namespace Element.Visual
{
    /// <summary>
    /// Burst型エフェクト（投げっぱなし発動型）
    /// アニメーション、VFX、シェーダーをコールバック付きで制御
    /// </summary>
    public class BurstEffectController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _burstStateName;

        [Header("VFX Settings")]
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private UnityEngine.VFX.VisualEffect _visualEffect;

        [Header("Shader Settings")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private string _progressPropertyName = "_Progress";
        [SerializeField] private float _shaderDuration = 1f;

        private Material _material;
        private int _progressPropertyID;

        private readonly Subject<Unit> _onComplete = new();

        /// <summary>エフェクト完了時のObservable</summary>
        public Observable<Unit> OnComplete => _onComplete;

        private void Awake()
        {
            if (_renderer != null)
            {
                _material = _renderer.material;
                _progressPropertyID = Shader.PropertyToID(_progressPropertyName);
            }
        }

        /// <summary>
        /// Burst発動（アニメーション）
        /// </summary>
        public void PlayAnimation()
        {
            if (_animator == null) return;

            _animator.Play(_burstStateName);
            MonitorAnimationCompletion().Forget();
        }

        /// <summary>
        /// Burst発動（パーティクル）
        /// </summary>
        public void PlayParticle()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
                MonitorParticleCompletion(_particleSystem).Forget();
            }
            else if (_visualEffect != null)
            {
                _visualEffect.Play();
                MonitorVFXCompletion().Forget();
            }
        }

        /// <summary>
        /// Burst発動（シェーダープログレス）
        /// </summary>
        public void PlayShaderProgress()
        {
            if (_material == null) return;
            AnimateShaderProgress().Forget();
        }

        /// <summary>
        /// すべてのエフェクトを一括発動
        /// </summary>
        public void PlayAll()
        {
            PlayAnimation();
            PlayParticle();
            PlayShaderProgress();
        }

        /// <summary>
        /// アニメーション完了監視
        /// </summary>
        private async UniTaskVoid MonitorAnimationCompletion()
        {
            if (_animator == null) return;

            // ステート開始まで待機
            await UniTask.Yield();

            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.normalizedTime < 1f && _animator.IsInTransition(0) == false)
            {
                await UniTask.Yield();
                stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            }

            _onComplete.OnNext(Unit.Default);
        }

        /// <summary>
        /// ParticleSystem完了監視
        /// </summary>
        private async UniTaskVoid MonitorParticleCompletion(ParticleSystem ps)
        {
            // パーティクルが存在する間待機
            while (ps.particleCount > 0)
            {
                await UniTask.Yield();
            }

            _onComplete.OnNext(Unit.Default);
        }

        /// <summary>
        /// VFXGraph完了監視
        /// </summary>
        private async UniTaskVoid MonitorVFXCompletion()
        {
            if (_visualEffect == null) return;

            // VFXのalive particle数が0になるまで待機
            await UniTask.WaitUntil(() => _visualEffect.aliveParticleCount == 0);

            _onComplete.OnNext(Unit.Default);
        }

        /// <summary>
        /// シェーダープログレスアニメーション
        /// </summary>
        private async UniTaskVoid AnimateShaderProgress()
        {
            float elapsed = 0f;

            while (elapsed < _shaderDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _shaderDuration;
                _material.SetFloat(_progressPropertyID, progress);
                await UniTask.Yield();
            }

            _material.SetFloat(_progressPropertyID, 1f);
            _onComplete.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onComplete?.Dispose();
        }
    }
}
