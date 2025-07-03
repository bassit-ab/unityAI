using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace RunnerGame.Enhancements
{
    public class EnhancedVFXManager : MonoBehaviour
    {
        [Header("Screen Effects")]
        public Camera mainCamera;
        public Material screenShakeMaterial;
        public AnimationCurve shakeCurve;
        
        [Header("Particle Systems")]
        public ParticleSystem[] hitEffects;
        public ParticleSystem[] collectEffects;
        public ParticleSystem[] explosionEffects;
        public ParticleSystem trailEffect;
        
        [Header("Dynamic Lighting")]
        public Light dynamicLight;
        public Gradient lightColorGradient;
        public AnimationCurve lightIntensityCurve;
        
        [Header("Post-Processing")]
        public bool enableDynamicPostProcessing = true;
        public float chromaticAberrationMax = 0.5f;
        public float vignetteMax = 0.3f;
        
        private Dictionary<string, ParticleSystem> effectPool = new Dictionary<string, ParticleSystem>();
        private Vector3 originalCameraPosition;
        private float shakeIntensity = 0f;
        private Sequence currentShakeSequence;
        
        public static EnhancedVFXManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeVFX();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            originalCameraPosition = mainCamera.transform.position;
            InitializeEffectPool();
        }
        
        private void InitializeVFX()
        {
            // Initialize dynamic lighting
            if (dynamicLight == null)
            {
                GameObject lightObj = new GameObject("Dynamic Light");
                dynamicLight = lightObj.AddComponent<Light>();
                dynamicLight.type = LightType.Point;
                dynamicLight.intensity = 1f;
                dynamicLight.range = 10f;
            }
        }
        
        private void InitializeEffectPool()
        {
            // Pre-instantiate effect pools for better performance
            for (int i = 0; i < hitEffects.Length; i++)
            {
                effectPool[$"hit_{i}"] = hitEffects[i];
            }
            
            for (int i = 0; i < collectEffects.Length; i++)
            {
                effectPool[$"collect_{i}"] = collectEffects[i];
            }
            
            for (int i = 0; i < explosionEffects.Length; i++)
            {
                effectPool[$"explosion_{i}"] = explosionEffects[i];
            }
        }
        
        public void PlayHitEffect(Vector3 position, HitType hitType = HitType.Normal)
        {
            ParticleSystem effect = GetHitEffect(hitType);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.Play();
            }
            
            // Add screen shake
            TriggerScreenShake(GetShakeIntensity(hitType), 0.3f);
            
            // Dynamic lighting flash
            TriggerLightFlash(GetFlashColor(hitType), 0.2f);
            
            // Camera effects
            if (enableDynamicPostProcessing)
            {
                TriggerCameraEffect(hitType);
            }
        }
        
        public void PlayCollectEffect(Vector3 position, CollectibleType collectibleType)
        {
            ParticleSystem effect = GetCollectEffect(collectibleType);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.Play();
            }
            
            // Positive feedback effects
            TriggerPositiveFeedback(collectibleType);
        }
        
        public void PlayExplosionEffect(Vector3 position, ExplosionType explosionType)
        {
            ParticleSystem effect = GetExplosionEffect(explosionType);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.Play();
            }
            
            // Intense screen shake
            TriggerScreenShake(1.5f, 0.8f);
            
            // Bright flash
            TriggerLightFlash(Color.white, 0.5f);
        }
        
        private ParticleSystem GetHitEffect(HitType hitType)
        {
            switch (hitType)
            {
                case HitType.Critical:
                    return effectPool.ContainsKey("hit_2") ? effectPool["hit_2"] : null;
                case HitType.Heavy:
                    return effectPool.ContainsKey("hit_1") ? effectPool["hit_1"] : null;
                default:
                    return effectPool.ContainsKey("hit_0") ? effectPool["hit_0"] : null;
            }
        }
        
        private ParticleSystem GetCollectEffect(CollectibleType collectibleType)
        {
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                    return effectPool.ContainsKey("collect_0") ? effectPool["collect_0"] : null;
                case CollectibleType.PowerUp:
                    return effectPool.ContainsKey("collect_1") ? effectPool["collect_1"] : null;
                case CollectibleType.Rare:
                    return effectPool.ContainsKey("collect_2") ? effectPool["collect_2"] : null;
                default:
                    return effectPool.ContainsKey("collect_0") ? effectPool["collect_0"] : null;
            }
        }
        
        private ParticleSystem GetExplosionEffect(ExplosionType explosionType)
        {
            switch (explosionType)
            {
                case ExplosionType.Large:
                    return effectPool.ContainsKey("explosion_1") ? effectPool["explosion_1"] : null;
                case ExplosionType.Massive:
                    return effectPool.ContainsKey("explosion_2") ? effectPool["explosion_2"] : null;
                default:
                    return effectPool.ContainsKey("explosion_0") ? effectPool["explosion_0"] : null;
            }
        }
        
        private void TriggerScreenShake(float intensity, float duration)
        {
            if (currentShakeSequence != null)
            {
                currentShakeSequence.Kill();
            }
            
            shakeIntensity = intensity;
            
            currentShakeSequence = DOTween.Sequence()
                .Append(DOTween.To(() => shakeIntensity, x => shakeIntensity = x, 0f, duration)
                    .SetEase(shakeCurve))
                .OnComplete(() => {
                    mainCamera.transform.position = originalCameraPosition;
                    shakeIntensity = 0f;
                });
        }
        
        private void TriggerLightFlash(Color flashColor, float duration)
        {
            if (dynamicLight != null)
            {
                dynamicLight.color = flashColor;
                
                DOTween.Sequence()
                    .Append(DOTween.To(() => dynamicLight.intensity, x => dynamicLight.intensity = x, 3f, duration * 0.2f))
                    .Append(DOTween.To(() => dynamicLight.intensity, x => dynamicLight.intensity = x, 1f, duration * 0.8f));
            }
        }
        
        private void TriggerCameraEffect(HitType hitType)
        {
            // Implement post-processing effects here
            // This would integrate with Unity's Post Processing Stack or URP Volume system
            
            float chromaticIntensity = 0f;
            float vignetteIntensity = 0f;
            
            switch (hitType)
            {
                case HitType.Critical:
                    chromaticIntensity = chromaticAberrationMax;
                    vignetteIntensity = vignetteMax;
                    break;
                case HitType.Heavy:
                    chromaticIntensity = chromaticAberrationMax * 0.6f;
                    vignetteIntensity = vignetteMax * 0.6f;
                    break;
                default:
                    chromaticIntensity = chromaticAberrationMax * 0.3f;
                    vignetteIntensity = vignetteMax * 0.3f;
                    break;
            }
            
            // Apply and fade out effects
            ApplyPostProcessingEffect(chromaticIntensity, vignetteIntensity, 0.5f);
        }
        
        private void TriggerPositiveFeedback(CollectibleType collectibleType)
        {
            // Gentle positive visual feedback
            Color feedbackColor = GetCollectibleColor(collectibleType);
            
            // Subtle light pulse
            TriggerLightFlash(feedbackColor, 0.3f);
            
            // UI feedback could be triggered here
        }
        
        private Color GetFlashColor(HitType hitType)
        {
            switch (hitType)
            {
                case HitType.Critical:
                    return Color.red;
                case HitType.Heavy:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
        
        private Color GetCollectibleColor(CollectibleType collectibleType)
        {
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                    return Color.yellow;
                case CollectibleType.PowerUp:
                    return Color.blue;
                case CollectibleType.Rare:
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }
        
        private float GetShakeIntensity(HitType hitType)
        {
            switch (hitType)
            {
                case HitType.Critical:
                    return 1.2f;
                case HitType.Heavy:
                    return 0.8f;
                default:
                    return 0.4f;
            }
        }
        
        private void ApplyPostProcessingEffect(float chromaticIntensity, float vignetteIntensity, float duration)
        {
            // This would integrate with your post-processing solution
            // Example implementation for demonstration
            
            DOTween.Sequence()
                .Append(DOTween.To(() => 0f, x => {
                    // Apply chromatic aberration
                    // Apply vignette
                }, 1f, duration * 0.2f))
                .Append(DOTween.To(() => 1f, x => {
                    // Fade out effects
                }, 0f, duration * 0.8f));
        }
        
        private void Update()
        {
            // Apply screen shake
            if (shakeIntensity > 0f)
            {
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-shakeIntensity, shakeIntensity),
                    Random.Range(-shakeIntensity, shakeIntensity),
                    0f
                );
                
                mainCamera.transform.position = originalCameraPosition + shakeOffset;
            }
        }
        
        public void SetTrailEffect(Transform target, bool enabled)
        {
            if (trailEffect != null)
            {
                trailEffect.transform.SetParent(target);
                trailEffect.transform.localPosition = Vector3.zero;
                
                if (enabled)
                {
                    trailEffect.Play();
                }
                else
                {
                    trailEffect.Stop();
                }
            }
        }
        
        public enum HitType
        {
            Normal,
            Heavy,
            Critical
        }
        
        public enum CollectibleType
        {
            Coin,
            PowerUp,
            Rare
        }
        
        public enum ExplosionType
        {
            Small,
            Large,
            Massive
        }
    }
} 