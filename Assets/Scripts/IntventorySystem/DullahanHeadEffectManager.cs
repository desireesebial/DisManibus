using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveEffect
{
    public EffectType effectType;
    public float strength;
    public float duration;
    public float startTime;
    public bool isActive = true;
}

public class DullahanHeadEffectManager : MonoBehaviour
{
    [Header("Player References")]
    public FirstPersonController playerController;
    public PlayerInventory playerInventory;
    public PlayerHealthSystem playerHealthSystem;
    public DullahanHeadInventory headInventory;
    
    [Header("Dullahan References")]
    public DullahanChaseSystem dullahanChase;
    public DullahanAudioManager audioManager;
    
    [Header("Effect Settings")]
    public float effectFadeTime = 1f;
    public bool showEffectNotifications = true;
    
    [Header("Visual Effects")]
    public ParticleSystem effectParticles;
    public Light effectLight;
    public Material effectMaterial;
    
    [Header("Audio Effects")]
    public AudioClip effectSound;
    public AudioClip effectEndSound;
    
    // Private variables
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    private AudioSource audioSource;
    private Renderer playerRenderer;
    private Material originalPlayerMaterial;
    
    void Start()
    {
        InitializeComponents();
        SetupAudio();
    }
    
    void Update()
    {
        UpdateActiveEffects();
    }
    
    void InitializeComponents()
    {
        // Find player components
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<FirstPersonController>();
            playerInventory = playerObj.GetComponent<PlayerInventory>();
            playerHealthSystem = playerObj.GetComponent<PlayerHealthSystem>();
            playerRenderer = playerObj.GetComponent<Renderer>();
            
            if (playerRenderer != null)
                originalPlayerMaterial = playerRenderer.material;
        }
        
        // Find Dullahan components
        dullahanChase = FindObjectOfType<DullahanChaseSystem>();
        audioManager = FindObjectOfType<DullahanAudioManager>();
        
        // Find head inventory
        headInventory = FindObjectOfType<DullahanHeadInventory>();
    }
    
    void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void ApplyHeadEffect(DullahanHeadSO headData)
    {
        if (headData == null || !headData.hasEffect) return;
        
        Debug.Log($"[DullahanHeadEffectManager] Applying effect: {headData.effectType} from {headData.headName}");
        
        // Create new active effect
        ActiveEffect newEffect = new ActiveEffect
        {
            effectType = headData.effectType,
            strength = headData.effectStrength,
            duration = headData.effectDuration,
            startTime = Time.time,
            isActive = true
        };
        
        activeEffects.Add(newEffect);
        
        // Apply immediate effect
        ApplyEffectImmediate(newEffect);
        
        // Play effect sound
        if (audioSource != null && effectSound != null)
            audioSource.PlayOneShot(effectSound);
        
        // Start visual effects
        StartEffectVisuals(newEffect);
        
        // Show notification
        if (showEffectNotifications)
        {
            ShowEffectNotification(headData);
        }
    }
    
    void ApplyEffectImmediate(ActiveEffect effect)
    {
        switch (effect.effectType)
        {
            case EffectType.SpeedBoost:
                ApplySpeedBoost(effect.strength);
                break;
            case EffectType.SpeedDebuff:
                ApplySlowDown(effect.strength);
                break;
            case EffectType.HealthBoost:
                ApplyHealthBoost(effect.strength);
                break;
            case EffectType.HealthDebuff:
                ApplyHealthDrain(effect.strength);
                break;
            case EffectType.VisionBoost:
                ApplyInvisibility(effect.strength);
                break;
            case EffectType.VisionDebuff:
                ApplyDullahanCalm(effect.strength);
                break;
            case EffectType.StaminaBoost:
                ApplyStaminaBoost(effect.strength);
                break;
            case EffectType.StaminaDebuff:
                ApplyStaminaDebuff(effect.strength);
                break;
            case EffectType.FearEffect:
                ApplyDullahanRage(effect.strength);
                break;
            case EffectType.CalmEffect:
                ApplyDullahanCalm(effect.strength);
                break;
            case EffectType.None:
            default:
                // No effect
                break;
        }
    }
    
    void ApplySpeedBoost(float strength)
    {
        if (playerController != null)
        {
            // Increase player speed (assuming FirstPersonController has a speed property)
            // Note: This will need to be adjusted based on actual FirstPersonController implementation
            Debug.Log($"[DullahanHeadEffectManager] Speed boost applied: {strength * 100f}%");
            // TODO: Implement actual speed boost when FirstPersonController structure is known
        }
    }
    
    void ApplySlowDown(float strength)
    {
        if (playerController != null)
        {
            // Decrease player speed (assuming FirstPersonController has a speed property)
            // Note: This will need to be adjusted based on actual FirstPersonController implementation
            Debug.Log($"[DullahanHeadEffectManager] Slow down applied: {strength * 100f}%");
            // TODO: Implement actual slow down when FirstPersonController structure is known
        }
    }
    
    void ApplyHealthBoost(float strength)
    {
        if (playerHealthSystem != null)
        {
            // Restore health (assuming PlayerHealthSystem has a health restoration method)
            // Note: This will need to be adjusted based on actual PlayerHealthSystem implementation
            Debug.Log($"[DullahanHeadEffectManager] Health boost applied: {strength * 100f}%");
            // TODO: Implement actual health boost when PlayerHealthSystem structure is known
        }
    }
    
    void ApplyHealthDrain(float strength)
    {
        if (playerHealthSystem != null)
        {
            // Drain health (assuming PlayerHealthSystem has a TakeDamage method)
            // Note: This will need to be adjusted based on actual PlayerHealthSystem implementation
            Debug.Log($"[DullahanHeadEffectManager] Health drain applied: {strength * 100f}%");
            // TODO: Implement actual health drain when PlayerHealthSystem structure is known
        }
    }
    
    void ApplyInvisibility(float strength)
    {
        if (playerRenderer != null)
        {
            // Make player semi-transparent
            Color color = playerRenderer.material.color;
            color.a = 1f - strength;
            playerRenderer.material.color = color;
            Debug.Log($"[DullahanHeadEffectManager] Invisibility applied: {strength * 100f}%");
        }
    }
    
    void ApplyDullahanCalm(float strength)
    {
        if (dullahanChase != null)
        {
            // Reduce Dullahan chase intensity
            float currentIntensity = dullahanChase.GetChaseIntensity();
            float newIntensity = currentIntensity * (1f - strength);
            dullahanChase.SetChaseIntensity(newIntensity);
            Debug.Log($"[DullahanHeadEffectManager] Dullahan calm applied: {strength * 100f}%");
        }
    }
    
    void ApplyDullahanRage(float strength)
    {
        if (dullahanChase != null)
        {
            // Increase Dullahan chase intensity
            float currentIntensity = dullahanChase.GetChaseIntensity();
            float newIntensity = Mathf.Min(1f, currentIntensity + strength);
            dullahanChase.SetChaseIntensity(newIntensity);
            Debug.Log($"[DullahanHeadEffectManager] Dullahan rage applied: {strength * 100f}%");
        }
    }
    
    void ApplyStaminaBoost(float strength)
    {
        if (playerController != null)
        {
            // Increase stamina (if player controller has stamina system)
            Debug.Log($"[DullahanHeadEffectManager] Stamina boost applied: {strength * 100f}%");
        }
    }
    
    void ApplyStaminaDebuff(float strength)
    {
        if (playerController != null)
        {
            // Decrease stamina (if player controller has stamina system)
            Debug.Log($"[DullahanHeadEffectManager] Stamina debuff applied: {strength * 100f}%");
        }
    }
    
    void UpdateActiveEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];
            
            if (!effect.isActive) continue;
            
            // Check if effect has expired
            if (Time.time - effect.startTime >= effect.duration)
            {
                RemoveEffect(effect);
                activeEffects.RemoveAt(i);
            }
        }
    }
    
    void RemoveEffect(ActiveEffect effect)
    {
        Debug.Log($"[DullahanHeadEffectManager] Removing effect: {effect.effectType}");
        
        // Reverse the effect
        switch (effect.effectType)
        {
            case EffectType.SpeedBoost:
                // TODO: Implement actual speed boost reversal when FirstPersonController structure is known
                break;
            case EffectType.SpeedDebuff:
                // TODO: Implement actual speed debuff reversal when FirstPersonController structure is known
                break;
            case EffectType.VisionBoost:
                if (playerRenderer != null)
                {
                    Color color = playerRenderer.material.color;
                    color.a = 1f;
                    playerRenderer.material.color = color;
                }
                break;
            case EffectType.VisionDebuff:
                if (dullahanChase != null)
                {
                    float currentIntensity = dullahanChase.GetChaseIntensity();
                    float newIntensity = currentIntensity / (1f - effect.strength);
                    dullahanChase.SetChaseIntensity(newIntensity);
                }
                break;
            case EffectType.StaminaBoost:
            case EffectType.StaminaDebuff:
                // Stamina effects are typically temporary and don't need reversal
                break;
            case EffectType.FearEffect:
                if (dullahanChase != null)
                {
                    float currentIntensity = dullahanChase.GetChaseIntensity();
                    float newIntensity = Mathf.Max(0f, currentIntensity - effect.strength);
                    dullahanChase.SetChaseIntensity(newIntensity);
                }
                break;
            case EffectType.CalmEffect:
                if (dullahanChase != null)
                {
                    float currentIntensity = dullahanChase.GetChaseIntensity();
                    float newIntensity = currentIntensity / (1f - effect.strength);
                    dullahanChase.SetChaseIntensity(newIntensity);
                }
                break;
            case EffectType.None:
            default:
                // No effect to reverse
                break;
        }
        
        // Stop visual effects
        StopEffectVisuals(effect);
        
        // Play effect end sound
        if (audioSource != null && effectEndSound != null)
            audioSource.PlayOneShot(effectEndSound);
        
        effect.isActive = false;
    }
    
    void StartEffectVisuals(ActiveEffect effect)
    {
        // Start particle effects
        if (effectParticles != null)
        {
            effectParticles.Play();
        }
        
        // Start light effects
        if (effectLight != null)
        {
            effectLight.enabled = true;
            effectLight.color = GetEffectColor(effect.effectType);
            effectLight.intensity = effect.strength * 2f;
        }
        
        // Apply effect material
        if (effectMaterial != null && playerRenderer != null)
        {
            playerRenderer.material = effectMaterial;
        }
    }
    
    void StopEffectVisuals(ActiveEffect effect)
    {
        // Stop particle effects
        if (effectParticles != null)
        {
            effectParticles.Stop();
        }
        
        // Stop light effects
        if (effectLight != null)
        {
            effectLight.enabled = false;
        }
        
        // Restore original material
        if (originalPlayerMaterial != null && playerRenderer != null)
        {
            playerRenderer.material = originalPlayerMaterial;
        }
    }
    
    Color GetEffectColor(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.SpeedBoost:
                return Color.green;
            case EffectType.SpeedDebuff:
                return Color.red;
            case EffectType.HealthBoost:
                return Color.blue;
            case EffectType.HealthDebuff:
                return Color.magenta;
            case EffectType.VisionBoost:
                return Color.cyan;
            case EffectType.VisionDebuff:
                return Color.yellow;
            case EffectType.StaminaBoost:
                return Color.green;
            case EffectType.StaminaDebuff:
                return Color.red;
            case EffectType.FearEffect:
                return Color.red;
            case EffectType.CalmEffect:
                return Color.blue;
            case EffectType.None:
            default:
                return Color.white;
        }
    }
    
    void ShowEffectNotification(DullahanHeadSO headData)
    {
        // This would typically show a UI notification
        Debug.Log($"[DullahanHeadEffectManager] Effect notification: {headData.headName} - {headData.effectType}");
    }
    
    public void ClearAllEffects()
    {
        Debug.Log("[DullahanHeadEffectManager] Clearing all active effects");
        
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            RemoveEffect(activeEffects[i]);
        }
        
        activeEffects.Clear();
    }
    
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }
    
    public bool HasActiveEffect(EffectType effectType)
    {
        foreach (ActiveEffect effect in activeEffects)
        {
            if (effect.isActive && effect.effectType == effectType)
                return true;
        }
        return false;
    }
    
    public List<ActiveEffect> GetActiveEffects()
    {
        return new List<ActiveEffect>(activeEffects);
    }
    
    // Debug methods
    public void TestEffect(EffectType effectType, float strength = 0.5f, float duration = 5f)
    {
        DullahanHeadSO testHead = ScriptableObject.CreateInstance<DullahanHeadSO>();
        testHead.headName = "Test Head";
        testHead.effectType = effectType;
        testHead.effectStrength = strength;
        testHead.effectDuration = duration;
        testHead.hasEffect = true;
        
        ApplyHeadEffect(testHead);
    }
    
    void OnDestroy()
    {
        // Clean up effects when destroyed
        ClearAllEffects();
    }
}
