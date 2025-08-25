using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DullahanHeadEffectManager : MonoBehaviour
{
    [Header("Player References")]
    public FirstPersonController playerController;
    public PlayerInventory playerInventory;
    
    [Header("Dullahan References")]
    public DullahanChaseSystem dullahanChase;
    public DullahanAudioManager audioManager;
    
    [Header("Effect Settings")]
    public float effectFadeTime = 1f;
    public bool showEffectNotifications = true;
    
    [Header("UI")]
    public GameObject effectNotificationUI;
    public TMPro.TextMeshProUGUI effectNotificationText;
    
    // Active effects
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    
    // Original player values
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private float originalSprintDuration;
    private float originalFOV;
    
    void Start()
    {
        // Find player components
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
            
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
            
        if (dullahanChase == null)
            dullahanChase = FindObjectOfType<DullahanChaseSystem>();
            
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
            
        // Store original values
        if (playerController != null)
        {
            originalWalkSpeed = playerController.walkSpeed;
            originalSprintSpeed = playerController.sprintSpeed;
            originalSprintDuration = playerController.sprintDuration;
            originalFOV = playerController.fov;
        }
        
        // Setup UI
        if (effectNotificationUI != null)
            effectNotificationUI.SetActive(false);
    }
    
    void Update()
    {
        // Update active effects
        UpdateActiveEffects();
    }
    
    public void ApplyHeadEffect(DullahanHeadSO headData)
    {
        if (headData == null || !headData.hasEffect) return;
        
        // Create new active effect
        ActiveEffect newEffect = new ActiveEffect
        {
            effectType = headData.effectType,
            strength = headData.effectStrength,
            duration = headData.effectDuration,
            startTime = Time.time
        };
        
        // Apply the effect immediately
        ApplyEffect(newEffect);
        
        // Add to active effects list
        activeEffects.Add(newEffect);
        
        // Show notification
        if (showEffectNotifications)
        {
            ShowEffectNotification(headData);
        }
        
        // Play effect sound
        if (audioManager != null)
        {
            PlayEffectSound(headData.effectType);
        }
        else if (headData.effectSound != null)
        {
            AudioSource.PlayClipAtPoint(headData.effectSound, playerController.transform.position);
        }
        
        Debug.Log($"Applied {headData.effectType} effect from {headData.headName}");
    }
    
    void ApplyEffect(ActiveEffect effect)
    {
        if (playerController == null) return;
        
        switch (effect.effectType)
        {
            case EffectType.SpeedBoost:
                playerController.walkSpeed = originalWalkSpeed * (1f + effect.strength);
                playerController.sprintSpeed = originalSprintSpeed * (1f + effect.strength);
                break;
                
            case EffectType.SpeedDebuff:
                playerController.walkSpeed = originalWalkSpeed * (1f - effect.strength);
                playerController.sprintSpeed = originalSprintSpeed * (1f - effect.strength);
                break;
                
            case EffectType.VisionBoost:
                playerController.fov = originalFOV + (effect.strength * 10f);
                break;
                
            case EffectType.VisionDebuff:
                playerController.fov = originalFOV - (effect.strength * 10f);
                break;
                
            case EffectType.StaminaBoost:
                playerController.sprintDuration = originalSprintDuration * (1f + effect.strength);
                break;
                
            case EffectType.StaminaDebuff:
                playerController.sprintDuration = originalSprintDuration * (1f - effect.strength);
                break;
                
            case EffectType.FearEffect:
                if (dullahanChase != null)
                {
                    // Increase chase intensity
                    StartCoroutine(ApplyFearEffect(effect));
                }
                break;
                
            case EffectType.CalmEffect:
                if (dullahanChase != null)
                {
                    // Decrease chase intensity
                    StartCoroutine(ApplyCalmEffect(effect));
                }
                break;
        }
    }
    
    void RemoveEffect(ActiveEffect effect)
    {
        if (playerController == null) return;
        
        switch (effect.effectType)
        {
            case EffectType.SpeedBoost:
            case EffectType.SpeedDebuff:
                playerController.walkSpeed = originalWalkSpeed;
                playerController.sprintSpeed = originalSprintSpeed;
                break;
                
            case EffectType.VisionBoost:
            case EffectType.VisionDebuff:
                playerController.fov = originalFOV;
                break;
                
            case EffectType.StaminaBoost:
            case EffectType.StaminaDebuff:
                playerController.sprintDuration = originalSprintDuration;
                break;
        }
    }
    
    void UpdateActiveEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];
            
            // Check if effect has expired
            if (Time.time - effect.startTime >= effect.duration)
            {
                // Remove the effect
                RemoveEffect(effect);
                activeEffects.RemoveAt(i);
                
                Debug.Log($"Effect {effect.effectType} has expired");
            }
        }
    }
    
    IEnumerator ApplyFearEffect(ActiveEffect effect)
    {
        float originalIntensity = dullahanChase.GetCurrentIntensity();
        float targetIntensity = Mathf.Min(1f, originalIntensity + effect.strength);
        
        float elapsed = 0f;
        while (elapsed < effectFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectFadeTime;
            
            // This would need to be implemented in DullahanChaseSystem
            // dullahanChase.SetChaseIntensity(Mathf.Lerp(originalIntensity, targetIntensity, t));
            
            yield return null;
        }
    }
    
    IEnumerator ApplyCalmEffect(ActiveEffect effect)
    {
        float originalIntensity = dullahanChase.GetCurrentIntensity();
        float targetIntensity = Mathf.Max(0f, originalIntensity - effect.strength);
        
        float elapsed = 0f;
        while (elapsed < effectFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectFadeTime;
            
            // This would need to be implemented in DullahanChaseSystem
            // dullahanChase.SetChaseIntensity(Mathf.Lerp(originalIntensity, targetIntensity, t));
            
            yield return null;
        }
    }
    
    void ShowEffectNotification(DullahanHeadSO headData)
    {
        if (effectNotificationUI == null || effectNotificationText == null) return;
        
        string effectText = GetEffectDescription(headData);
        effectNotificationText.text = effectText;
        
        effectNotificationUI.SetActive(true);
        
        // Hide after 3 seconds
        StartCoroutine(HideNotificationAfterDelay(3f));
    }
    
    string GetEffectDescription(DullahanHeadSO headData)
    {
        string effectName = headData.effectType.ToString();
        string duration = $"{headData.effectDuration}s";
        
        switch (headData.effectType)
        {
            case EffectType.SpeedBoost:
                return $"Speed Boost! (+{headData.effectStrength * 100}% for {duration})";
            case EffectType.SpeedDebuff:
                return $"Speed Debuff! (-{headData.effectStrength * 100}% for {duration})";
            case EffectType.VisionBoost:
                return $"Vision Boost! (+{headData.effectStrength * 10} FOV for {duration})";
            case EffectType.VisionDebuff:
                return $"Vision Debuff! (-{headData.effectStrength * 10} FOV for {duration})";
            case EffectType.StaminaBoost:
                return $"Stamina Boost! (+{headData.effectStrength * 100}% for {duration})";
            case EffectType.StaminaDebuff:
                return $"Stamina Debuff! (-{headData.effectStrength * 100}% for {duration})";
            case EffectType.FearEffect:
                return $"Fear Effect! (Increased chase intensity for {duration})";
            case EffectType.CalmEffect:
                return $"Calm Effect! (Decreased chase intensity for {duration})";
            default:
                return $"Effect: {effectName} for {duration}";
        }
    }
    
    IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effectNotificationUI != null)
            effectNotificationUI.SetActive(false);
    }
    
    public void ClearAllEffects()
    {
        foreach (ActiveEffect effect in activeEffects)
        {
            RemoveEffect(effect);
        }
        activeEffects.Clear();
    }
    
    public List<ActiveEffect> GetActiveEffects()
    {
        return activeEffects;
    }
    
    void PlayEffectSound(EffectType effectType)
    {
        if (audioManager == null) return;
        
        switch (effectType)
        {
            case EffectType.SpeedBoost:
                audioManager.PlaySpeedBoostSound();
                break;
            case EffectType.SpeedDebuff:
                audioManager.PlaySpeedDebuffSound();
                break;
            case EffectType.VisionBoost:
                audioManager.PlayVisionBoostSound();
                break;
            case EffectType.VisionDebuff:
                audioManager.PlayVisionDebuffSound();
                break;
            case EffectType.FearEffect:
                audioManager.PlayFearEffectSound();
                break;
            case EffectType.CalmEffect:
                audioManager.PlayCalmEffectSound();
                break;
        }
    }
}

[System.Serializable]
public class ActiveEffect
{
    public EffectType effectType;
    public float strength;
    public float duration;
    public float startTime;
}
