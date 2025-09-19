using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public KeyCode flashlightKey = KeyCode.T;
    public bool isFlashlightOn = false;
    
    [Header("Light Properties")]
    public float lightIntensity = 2f;
    public float lightRange = 10f;
    public Color lightColor = Color.white;
    public LightType lightType = LightType.Spot;
    public float spotAngle = 60f;
    
    [Header("Audio")]
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

    // Private variables
    private Light flashlightLight;
    private AudioSource audioSource;
    private Transform playerCamera;

    private void Start()
    {
        // Find the player camera to attach the flashlight to
        FindPlayerCamera();
        
        // Create the flashlight light component
        CreateFlashlightLight();
        
        // Setup audio source
        SetupAudioSource();
        
        Debug.Log("Flashlight Controller initialized. Press T to toggle flashlight.");
    }

    private void FindPlayerCamera()
    {
        // Try to find FirstPersonController first
        FirstPersonController fpsController = FindAnyObjectByType<FirstPersonController>();
        if (fpsController != null && fpsController.playerCamera != null)
        {
            playerCamera = fpsController.playerCamera.transform;
            Debug.Log("Found player camera via FirstPersonController");
            return;
        }
        
        // Try to find SimplePlayerMovement
        SimplePlayerMovement simplePlayer = FindAnyObjectByType<SimplePlayerMovement>();
        if (simplePlayer != null && simplePlayer.playerCamera != null)
        {
            playerCamera = simplePlayer.playerCamera;
            Debug.Log("Found player camera via SimplePlayerMovement");
            return;
        }
        
        // Fallback to main camera
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
            Debug.Log("Using main camera as fallback");
        }
        else
        {
            Debug.LogError("No player camera found! Flashlight will not work properly.");
        }
    }

    private void CreateFlashlightLight()
    {
        if (playerCamera == null) return;
        
        // Create a child object for the flashlight light
        GameObject flashlightObject = new GameObject("FlashlightLight");
        flashlightObject.transform.SetParent(playerCamera);
        flashlightObject.transform.localPosition = Vector3.zero;
        flashlightObject.transform.localRotation = Quaternion.identity;
        
        // Add and configure the light component
        flashlightLight = flashlightObject.AddComponent<Light>();
        flashlightLight.type = lightType;
        flashlightLight.intensity = lightIntensity;
        flashlightLight.range = lightRange;
        flashlightLight.color = lightColor;
        
        if (lightType == LightType.Spot)
        {
            flashlightLight.spotAngle = spotAngle;
        }
        
        // Start with flashlight off
        flashlightLight.enabled = false;
        
        Debug.Log("Flashlight light created and attached to player camera");
    }

    private void SetupAudioSource()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for UI feedback
    }

    private void Update()
    {
        HandleFlashlightInput();
    }

    private void HandleFlashlightInput()
    {
        if (Input.GetKeyDown(flashlightKey))
        {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        if (flashlightLight == null) return;
        
        isFlashlightOn = !isFlashlightOn;
        flashlightLight.enabled = isFlashlightOn;
        
        // Play audio effect
        if (isFlashlightOn)
        {
            PlayAudioEffect(turnOnSound);
            Debug.Log("Flashlight turned ON");
        }
        else
        {
            PlayAudioEffect(turnOffSound);
            Debug.Log("Flashlight turned OFF");
        }
    }

    private void PlayAudioEffect(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public methods for external control
    public void SetFlashlightState(bool state)
    {
        if (flashlightLight == null) return;
        
        isFlashlightOn = state;
        flashlightLight.enabled = state;
    }

    public bool IsFlashlightOn()
    {
        return isFlashlightOn;
    }

    // Method to update flashlight properties at runtime
    public void UpdateFlashlightProperties(float intensity, float range, Color color)
    {
        if (flashlightLight == null) return;
        
        flashlightLight.intensity = intensity;
        flashlightLight.range = range;
        flashlightLight.color = color;
    }
}