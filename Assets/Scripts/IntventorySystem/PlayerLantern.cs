using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerLantern : MonoBehaviour
{
    [Header("Lantern Settings")]
    public bool hasLantern = false;
    public bool isLanternOn = false;
    public KeyCode toggleKey = KeyCode.L;
    
    [Header("Light Components")]
    public Light lanternLight;
    public GameObject lanternModel;
    public Transform lanternHolder; // Where the lantern is held
    
    [Header("Battery System")]
    public bool hasBattery = true;
    public float maxBatteryLife = 300f; // 5 minutes
    public float currentBatteryLife = 300f;
    public float batteryDrainRate = 1f; // Per second
    public float batteryRechargeRate = 0.5f; // Per second when off
    public bool infiniteBattery = false;
    
    [Header("Battery UI")]
    public GameObject batteryUI;
    public Image batteryFillImage;
    public TextMeshProUGUI batteryText;
    public Color fullBatteryColor = Color.green;
    public Color lowBatteryColor = Color.yellow;
    public Color criticalBatteryColor = Color.red;
    public float lowBatteryThreshold = 0.3f;
    public float criticalBatteryThreshold = 0.1f;
    
    [Header("Light Settings")]
    public float maxLightIntensity = 2f;
    public float minLightIntensity = 0.1f;
    public float lightFlickerIntensity = 0.1f;
    public float flickerSpeed = 5f;
    public bool enableFlicker = true;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lanternOnSound;
    public AudioClip lanternOffSound;
    public AudioClip batteryLowSound;
    public AudioClip batteryDeadSound;
    public AudioClip flickerSound;
    
    [Header("Visual Effects")]
    public ParticleSystem lanternParticles;
    public Material lanternMaterial;
    public Color onColor = Color.yellow;
    public Color offColor = Color.gray;
    
    [Header("Integration")]
    public FirstPersonController playerController;
    public DullahanHeadInventory headInventory; // For flashlight integration
    
    // Private variables
    private bool isFlickering = false;
    private bool batteryLowPlayed = false;
    private bool batteryDeadPlayed = false;
    private float originalLightIntensity;
    private Color originalMaterialColor;
    
    // Events
    public System.Action<bool> OnLanternToggled;
    public System.Action<float> OnBatteryChanged;
    public System.Action OnBatteryDead;
    
    void Start()
    {
        InitializeLantern();
    }
    
    void Update()
    {
        if (!hasLantern) return;
        
        HandleInput();
        HandleBattery();
        HandleLightEffects();
        HandleDebugInput();
    }
    
    private void InitializeLantern()
    {
        // Find references if not assigned
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
            
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Store original values
        if (lanternLight != null)
            originalLightIntensity = lanternLight.intensity;
            
        if (lanternMaterial != null)
            originalMaterialColor = lanternMaterial.color;
        
        // Setup initial state
        if (hasLantern)
        {
            currentBatteryLife = maxBatteryLife;
            SetLanternState(false);
            UpdateBatteryUI();
        }
        else
        {
            SetLanternState(false);
            if (batteryUI != null)
                batteryUI.SetActive(false);
        }
        
        Debug.Log("Player Lantern initialized");
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleLantern();
        }
    }
    
    public void ToggleLantern()
    {
        if (!hasLantern || (currentBatteryLife <= 0 && !infiniteBattery)) return;
        
        SetLanternState(!isLanternOn);
        
        // Play sound
        if (audioSource != null)
        {
            if (isLanternOn && lanternOnSound != null)
                audioSource.PlayOneShot(lanternOnSound);
            else if (!isLanternOn && lanternOffSound != null)
                audioSource.PlayOneShot(lanternOffSound);
        }
        
        // Trigger event
        OnLanternToggled?.Invoke(isLanternOn);
        
        Debug.Log($"Lantern toggled: {(isLanternOn ? "ON" : "OFF")}");
    }
    
    private void SetLanternState(bool state)
    {
        isLanternOn = state;
        
        // Update light
        if (lanternLight != null)
        {
            lanternLight.enabled = state;
            if (state)
                lanternLight.intensity = maxLightIntensity;
        }
        
        // Update model
        if (lanternModel != null)
        {
            lanternModel.SetActive(state);
        }
        
        // Update material
        if (lanternMaterial != null)
        {
            lanternMaterial.color = state ? onColor : offColor;
        }
        
        // Update particles
        if (lanternParticles != null)
        {
            if (state)
                lanternParticles.Play();
            else
                lanternParticles.Stop();
        }
        
        // Update UI
        if (batteryUI != null)
            batteryUI.SetActive(hasLantern);
    }
    
    private void HandleBattery()
    {
        if (!hasBattery || infiniteBattery) return;
        
        if (isLanternOn)
        {
            // Drain battery
            currentBatteryLife -= batteryDrainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(0f, currentBatteryLife);
            
            // Check for low battery
            float batteryPercentage = currentBatteryLife / maxBatteryLife;
            
            if (batteryPercentage <= criticalBatteryThreshold && !batteryDeadPlayed)
            {
                batteryDeadPlayed = true;
                OnBatteryDead?.Invoke();
                
                if (audioSource != null && batteryDeadSound != null)
                    audioSource.PlayOneShot(batteryDeadSound);
                
                // Turn off lantern when battery dies
                SetLanternState(false);
                Debug.Log("Lantern battery dead!");
            }
            else if (batteryPercentage <= lowBatteryThreshold && !batteryLowPlayed)
            {
                batteryLowPlayed = true;
                
                if (audioSource != null && batteryLowSound != null)
                    audioSource.PlayOneShot(batteryLowSound);
                
                Debug.Log("Lantern battery low!");
            }
        }
        else
        {
            // Recharge battery when off
            if (currentBatteryLife < maxBatteryLife)
            {
                currentBatteryLife += batteryRechargeRate * Time.deltaTime;
                currentBatteryLife = Mathf.Min(maxBatteryLife, currentBatteryLife);
                
                // Reset battery warnings when recharging
                if (currentBatteryLife > lowBatteryThreshold)
                    batteryLowPlayed = false;
                if (currentBatteryLife > criticalBatteryThreshold)
                    batteryDeadPlayed = false;
            }
        }
        
        UpdateBatteryUI();
        OnBatteryChanged?.Invoke(currentBatteryLife);
    }
    
    private void HandleLightEffects()
    {
        if (!isLanternOn || lanternLight == null) return;
        
        // Handle flickering
        if (enableFlicker && currentBatteryLife / maxBatteryLife <= lowBatteryThreshold)
        {
            if (!isFlickering)
            {
                isFlickering = true;
                StartCoroutine(LightFlicker());
            }
        }
        else
        {
            isFlickering = false;
        }
    }
    
    private IEnumerator LightFlicker()
    {
        while (isFlickering && isLanternOn)
        {
            float flickerAmount = Random.Range(-lightFlickerIntensity, lightFlickerIntensity);
            lanternLight.intensity = maxLightIntensity + flickerAmount;
            
            // Play flicker sound occasionally
            if (Random.Range(0f, 1f) < 0.1f && audioSource != null && flickerSound != null)
            {
                audioSource.PlayOneShot(flickerSound);
            }
            
            yield return new WaitForSeconds(1f / flickerSpeed);
        }
        
        // Reset light intensity
        if (lanternLight != null)
            lanternLight.intensity = maxLightIntensity;
    }
    
    private void UpdateBatteryUI()
    {
        if (batteryUI == null) return;
        
        batteryUI.SetActive(hasLantern);
        
        if (batteryFillImage != null)
        {
            float batteryPercentage = currentBatteryLife / maxBatteryLife;
            batteryFillImage.fillAmount = batteryPercentage;
            
            // Change color based on battery level
            if (batteryPercentage <= criticalBatteryThreshold)
                batteryFillImage.color = criticalBatteryColor;
            else if (batteryPercentage <= lowBatteryThreshold)
                batteryFillImage.color = lowBatteryColor;
            else
                batteryFillImage.color = fullBatteryColor;
        }
        
        if (batteryText != null)
        {
            if (infiniteBattery)
                batteryText.text = "∞";
            else
            {
                int minutes = Mathf.FloorToInt(currentBatteryLife / 60f);
                int seconds = Mathf.FloorToInt(currentBatteryLife % 60f);
                batteryText.text = $"{minutes:00}:{seconds:00}";
            }
        }
    }
    
    public void GiveLantern()
    {
        hasLantern = true;
        currentBatteryLife = maxBatteryLife;
        SetLanternState(false);
        UpdateBatteryUI();
        
        Debug.Log("Player received lantern!");
    }
    
    public void RemoveLantern()
    {
        hasLantern = false;
        SetLanternState(false);
        if (batteryUI != null)
            batteryUI.SetActive(false);
        
        Debug.Log("Player lost lantern!");
    }
    
    public void RechargeBattery(float amount)
    {
        if (!hasLantern) return;
        
        currentBatteryLife = Mathf.Min(maxBatteryLife, currentBatteryLife + amount);
        UpdateBatteryUI();
        
        Debug.Log($"Lantern battery recharged by {amount}");
    }
    
    public void SetInfiniteBattery(bool infinite)
    {
        infiniteBattery = infinite;
        UpdateBatteryUI();
        
        Debug.Log($"Infinite battery: {infinite}");
    }
    
    private void HandleDebugInput()
    {
        // Debug keys for testing
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Toggle lantern (already handled in HandleInput)
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            RechargeBattery(60f); // Recharge 1 minute
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            GiveLantern();
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            SetInfiniteBattery(!infiniteBattery);
        }
    }
    
    // Public getters
    public bool HasLantern() => hasLantern;
    public bool IsLanternOn() => isLanternOn;
    public bool HasBattery() => hasBattery;
    public float GetBatteryPercentage() => currentBatteryLife / maxBatteryLife;
    public float GetCurrentBatteryLife() => currentBatteryLife;
    public float GetMaxBatteryLife() => maxBatteryLife;
    public bool IsInfiniteBattery() => infiniteBattery;
    
    // Method to be called by other systems (like DullahanHeadInventory)
    public void OnFlashlightToggled(bool isOn)
    {
        // If flashlight is on, turn off lantern to avoid conflicts
        if (isOn && isLanternOn)
        {
            SetLanternState(false);
        }
    }
}
