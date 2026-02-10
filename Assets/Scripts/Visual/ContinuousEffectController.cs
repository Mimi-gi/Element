using UnityEngine;

namespace Element.Visual
{
    /// <summary>
    /// 持続型エフェクト（Continuous）
    /// オン/オフ切り替えと状態管理
    /// </summary>
    public class ContinuousEffectController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _onStateName;
        [SerializeField] private string _offStateName;
        [SerializeField] private string _boolParameterName;

        [Header("VFX Settings")]
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private UnityEngine.VFX.VisualEffect _visualEffect;

        [Header("Shader Settings")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private string _activePropertyName = "_IsActive";
        [SerializeField] private string _intensityPropertyName = "_Intensity";

        private Material _material;
        private int _activePropertyID;
        private int _intensityPropertyID;

        /// <summary>現在発動中かどうか</summary>
        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (_renderer != null)
            {
                _material = _renderer.material;
                _activePropertyID = Shader.PropertyToID(_activePropertyName);
                _intensityPropertyID = Shader.PropertyToID(_intensityPropertyName);
            }
        }

        /// <summary>
        /// エフェクトをオンにする
        /// </summary>
        public void SetActive(bool active)
        {
            if (IsActive == active) return;

            IsActive = active;

            UpdateAnimation(active);
            UpdateVFX(active);
            UpdateShader(active);
        }

        /// <summary>
        /// エフェクトのオン/オフを切り替える
        /// </summary>
        public void Toggle()
        {
            SetActive(!IsActive);
        }

        /// <summary>
        /// エフェクトの強度を設定（0~1）
        /// </summary>
        public void SetIntensity(float intensity)
        {
            if (_material != null)
            {
                _material.SetFloat(_intensityPropertyID, Mathf.Clamp01(intensity));
            }

            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.rateOverTime = intensity * 100f; // 基準値を100として調整
            }
        }

        /// <summary>
        /// アニメーション更新
        /// </summary>
        private void UpdateAnimation(bool active)
        {
            if (_animator == null) return;

            if (!string.IsNullOrEmpty(_boolParameterName))
            {
                _animator.SetBool(_boolParameterName, active);
            }
            else if (!string.IsNullOrEmpty(_onStateName) && !string.IsNullOrEmpty(_offStateName))
            {
                _animator.Play(active ? _onStateName : _offStateName);
            }
        }

        /// <summary>
        /// VFX更新
        /// </summary>
        private void UpdateVFX(bool active)
        {
            if (_particleSystem != null)
            {
                if (active)
                {
                    if (!_particleSystem.isPlaying)
                        _particleSystem.Play();
                }
                else
                {
                    _particleSystem.Stop();
                }
            }

            if (_visualEffect != null)
            {
                if (active)
                {
                    if (!_visualEffect.isActiveAndEnabled)
                        _visualEffect.Play();
                }
                else
                {
                    _visualEffect.Stop();
                }
            }
        }

        /// <summary>
        /// シェーダー更新
        /// </summary>
        private void UpdateShader(bool active)
        {
            if (_material == null) return;
            _material.SetFloat(_activePropertyID, active ? 1f : 0f);
        }
    }
}
