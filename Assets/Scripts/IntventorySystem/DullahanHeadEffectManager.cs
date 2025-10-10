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
        
        // Find player health if not assigned
        if (playerHealthSystem == null)
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();

        // Find head inventory if not assigned
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
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
        Debug.Log("[EffectManager] ═══════════════════════════════════════");
        Debug.Log("[EffectManager] ► APPLYING PLAYER EFFECT");
        
        if (headData == null)
        {
            Debug.LogError("[EffectManager] ✗ headData is null!");
            return;
        }
        
        if (!headData.hasEffect)
        {
            Debug.Log($"[EffectManager] {headData.headName} has no effect to apply");
            return;
        }
        
        Debug.Log($"[EffectManager] Head: {headData.headName}");
        Debug.Log($"[EffectManager] Effect Type: {headData.effectType}");
        Debug.Log($"[EffectManager] Strength: {headData.effectStrength}");
        Debug.Log($"[EffectManager] Duration: {headData.effectDuration}s");
        
        // Create new active effect
        ActiveEffect newEffect = new ActiveEffect
        {
            effectType = headData.effectType,
            strength = headData.effectStrength,
            duration = headData.effectDuration,
            startTime = Time.time
        };
        
        // Apply the effect immediately
        Debug.Log($"[EffectManager] Applying effect to player...");
        ApplyEffect(newEffect);
        
        // Add to active effects list
        activeEffects.Add(newEffect);
        Debug.Log($"[EffectManager] Effect added to active effects list (total: {activeEffects.Count})");
        
        // Show notification
        if (showEffectNotifications)
        {
            ShowEffectNotification(headData);
        }
        
        // Play effect sound
        PlayEffectSound(headData);
        
        Debug.Log($"[EffectManager] ✓ Successfully applied {headData.effectType} effect from {headData.headName}");
        Debug.Log("[EffectManager] ═══════════════════════════════════════");
    }
    
    private void ApplyEffect(ActiveEffect effect)
    {
        Debug.Log($"[Effect Apply] Processing effect type: {effect.effectType}");
        
        // Movement/vision effects require controller; health effects can run without it
        switch (effect.effectType)
        {
            case EffectType.SpeedBoost:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply SpeedBoost");
                    return;
                }
                float newWalkSpeed = originalWalkSpeed * (1f + effect.strength);
                float newSprintSpeed = originalSprintSpeed * (1f + effect.strength);
                playerController.walkSpeed = newWalkSpeed;
                playerController.sprintSpeed = newSprintSpeed;
                Debug.Log($"[Effect Apply] ✓ SpeedBoost: Walk {originalWalkSpeed:F1} → {newWalkSpeed:F1}, Sprint {originalSprintSpeed:F1} → {newSprintSpeed:F1}");
                break;
                
            case EffectType.SpeedDebuff:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply SpeedDebuff");
                    return;
                }
                float newWalkSpeedDebuff = originalWalkSpeed * (1f - effect.strength);
                float newSprintSpeedDebuff = originalSprintSpeed * (1f - effect.strength);
                playerController.walkSpeed = newWalkSpeedDebuff;
                playerController.sprintSpeed = newSprintSpeedDebuff;
                Debug.Log($"[Effect Apply] ✓ SpeedDebuff: Walk {originalWalkSpeed:F1} → {newWalkSpeedDebuff:F1}, Sprint {originalSprintSpeed:F1} → {newSprintSpeedDebuff:F1}");
                break;
                
            case EffectType.VisionBoost:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply VisionBoost");
                    return;
                }
                float newFOVBoost = originalFOV + (effect.strength * 10f);
                playerController.fov = newFOVBoost;
                Debug.Log($"[Effect Apply] ✓ VisionBoost: FOV {originalFOV:F1} → {newFOVBoost:F1}");
                break;
                
            case EffectType.VisionDebuff:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply VisionDebuff");
                    return;
                }
                float newFOVDebuff = originalFOV - (effect.strength * 10f);
                playerController.fov = newFOVDebuff;
                Debug.Log($"[Effect Apply] ✓ VisionDebuff: FOV {originalFOV:F1} → {newFOVDebuff:F1}");
                break;
                
            case EffectType.StaminaBoost:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply StaminaBoost");
                    return;
                }
                float newStaminaBoost = originalSprintDuration * (1f + effect.strength);
                playerController.sprintDuration = newStaminaBoost;
                Debug.Log($"[Effect Apply] ✓ StaminaBoost: Duration {originalSprintDuration:F1} → {newStaminaBoost:F1}");
                break;
                
            case EffectType.StaminaDebuff:
                if (playerController == null)
                {
                    Debug.LogError("[Effect Apply] ✗ playerController is null! Cannot apply StaminaDebuff");
                    return;
                }
                float newStaminaDebuff = originalSprintDuration * (1f - effect.strength);
                playerController.sprintDuration = newStaminaDebuff;
                Debug.Log($"[Effect Apply] ✓ StaminaDebuff: Duration {originalSprintDuration:F1} → {newStaminaDebuff:F1}");
                break;

            case EffectType.HealthBoost:
                if (playerHealthSystem != null)
                {
                    int healAmount = Mathf.Max(1, Mathf.RoundToInt(effect.strength));
                    playerHealthSystem.Heal(healAmount);
                    Debug.Log($"[Effect Apply] ✓ HealthBoost: Healed {healAmount} HP");
                }
                else
                {
                    Debug.LogWarning("[Effect Apply] ⚠ playerHealthSystem is null! Cannot apply HealthBoost");
                }
                break;

            case EffectType.HealthDebuff:
                if (playerHealthSystem != null)
                {
                    int damageAmount = Mathf.Max(1, Mathf.RoundToInt(effect.strength));
                    playerHealthSystem.TakeDamage(damageAmount);
                    Debug.Log($"[Effect Apply] ✓ HealthDebuff: Dealt {damageAmount} damage to player");
                }
                else
                {
                    Debug.LogWarning("[Effect Apply] ⚠ playerHealthSystem is null! Cannot apply HealthDebuff");
                }
                break;
                
            case EffectType.FearEffect:
                if (dullahanChase != null)
                {
                    Debug.Log("[Effect Apply] ✓ Applying FearEffect (increases Dullahan intensity)");
                    // Increase chase intensity
                    StartCoroutine(ApplyFearEffect(effect));
                }
                else
                {
                    Debug.LogWarning("[Effect Apply] ⚠ dullahanChase is null! Cannot apply FearEffect");
                }
                break;
                
            case EffectType.CalmEffect:
                if (dullahanChase != null)
                {
                    Debug.Log("[Effect Apply] ✓ Applying CalmEffect (decreases Dullahan intensity)");
                    // Decrease chase intensity
                    StartCoroutine(ApplyCalmEffect(effect));
                }
                else
                {
                    Debug.LogWarning("[Effect Apply] ⚠ dullahanChase is null! Cannot apply CalmEffect");
                }
                break;
                
            default:
                Debug.LogWarning($"[Effect Apply] ⚠ Unknown effect type: {effect.effectType}");
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
            // Health effects are instantaneous; nothing to revert here
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
            
            if (dullahanChase != null)
            {
                dullahanChase.SetChaseIntensity(Mathf.Lerp(originalIntensity, targetIntensity, t));
            }
            
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
            
            if (dullahanChase != null)
            {
                dullahanChase.SetChaseIntensity(Mathf.Lerp(originalIntensity, targetIntensity, t));
            }
            
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
