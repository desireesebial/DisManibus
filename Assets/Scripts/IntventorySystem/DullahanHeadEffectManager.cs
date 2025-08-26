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
        FindPlayerComponents();
        
        // Store original values
        StoreOriginalValues();
        
        // Setup UI
        if (effectNotificationUI != null)
            effectNotificationUI.SetActive(false);
    }
    
    void Update()
    {
        // Update active effects
        UpdateActiveEffects();
    }
    
    private void FindPlayerComponents()
    {
        // Find player controller if not assigned
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
            
        // Find player inventory if not assigned
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
            
        // Find Dullahan chase if not assigned
        if (dullahanChase == null)
            dullahanChase = FindObjectOfType<DullahanChaseSystem>();
            
        // Find audio manager if not assigned
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
    }
    
    private void StoreOriginalValues()
    {
        if (playerController != null)
        {
            originalWalkSpeed = playerController.walkSpeed;
            originalSprintSpeed = playerController.sprintSpeed;
            originalSprintDuration = playerController.sprintDuration;
            originalFOV = playerController.fov;
        }
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
        PlayEffectSound(headData);
        
        Debug.Log($"Applied {headData.effectType} effect from {headData.headName}");
    }
    
    private void ApplyEffect(ActiveEffect effect)
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
    
    private void RemoveEffect(ActiveEffect effect)
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
    
    private void UpdateActiveEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];
            
            if (!effect.isActive) continue;
            
            // Check if effect has expired
            if (Time.time - effect.startTime >= effect.duration)
            {
                // Remove the effect
                RemoveEffect(effect);
                activeEffects.RemoveAt(i);
                
                Debug.Log($"Removed {effect.effectType} effect");
            }
        }
    }
    
    private IEnumerator ApplyFearEffect(ActiveEffect effect)
    {
        float originalIntensity = dullahanChase.GetCurrentIntensity();
        float targetIntensity = Mathf.Clamp01(originalIntensity + effect.strength);
        
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
    
    private IEnumerator ApplyCalmEffect(ActiveEffect effect)
    {
        float originalIntensity = dullahanChase.GetCurrentIntensity();
        float targetIntensity = Mathf.Clamp01(originalIntensity - effect.strength);
        
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
    
    private void ShowEffectNotification(DullahanHeadSO headData)
    {
        if (effectNotificationUI == null || effectNotificationText == null) return;
        
        string effectText = GetEffectText(headData.effectType, headData.effectStrength);
        effectNotificationText.text = $"{headData.headName}: {effectText}";
        
        effectNotificationUI.SetActive(true);
        
        // Hide notification after 3 seconds
        StartCoroutine(HideNotificationAfterDelay(3f));
    }
    
    private string GetEffectText(EffectType effectType, float strength)
    {
        switch (effectType)
        {
            case EffectType.SpeedBoost:
                return $"Speed +{(strength * 100):0}%";
            case EffectType.SpeedDebuff:
                return $"Speed -{(strength * 100):0}%";
            case EffectType.VisionBoost:
                return $"Vision +{(strength * 10):0}°";
            case EffectType.VisionDebuff:
                return $"Vision -{(strength * 10):0}°";
            case EffectType.StaminaBoost:
                return $"Stamina +{(strength * 100):0}%";
            case EffectType.StaminaDebuff:
                return $"Stamina -{(strength * 100):0}%";
            case EffectType.FearEffect:
                return "Fear Increased";
            case EffectType.CalmEffect:
                return "Fear Decreased";
            default:
                return "Unknown Effect";
        }
    }
    
    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effectNotificationUI != null)
            effectNotificationUI.SetActive(false);
    }
    
    private void PlayEffectSound(DullahanHeadSO headData)
    {
        // Try audio manager first
        if (audioManager != null)
        {
            audioManager.PlayHeadEffectSound(headData.headType);
        }
        // Fallback to head data audio
        else if (headData.effectSound != null)
        {
            AudioSource.PlayClipAtPoint(headData.effectSound, Camera.main.transform.position);
        }
    }
    
    // Public methods for other scripts
    public void ClearAllEffects()
    {
        foreach (ActiveEffect effect in activeEffects)
        {
            RemoveEffect(effect);
        }
        activeEffects.Clear();
    }
    
    public bool HasActiveEffect(EffectType effectType)
    {
        foreach (ActiveEffect effect in activeEffects)
        {
            if (effect.effectType == effectType && effect.isActive)
                return true;
        }
        return false;
    }
    
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }
}
