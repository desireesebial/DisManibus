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

	[Header("Battery")]
	[Tooltip("Total usable seconds the flashlight can stay on from full to empty.")]
	public float batteryCapacitySeconds = 60f;
	[Tooltip("Current remaining usable seconds. Starts full by default.")]
	public float batterySecondsRemaining = 60f;
	[Tooltip("Delay after turning off or depleting before recharge begins (seconds).")]
	public float rechargeDelaySeconds = 5f;
	[Tooltip("Recharge speed in battery seconds regained per real-time second (when off).")]
	public float rechargeRatePerSecond = 5f;
	[Tooltip("If true, the light cannot be toggled on while battery is fully depleted and recharging.")]
	public bool lockUseWhenDepleted = true;

    // Private variables
    private Light flashlightLight;
    private AudioSource audioSource;
    private Transform playerCamera;
	private float rechargeTimer;

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
		UpdateBattery();
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

		if (!isFlashlightOn && !CanTurnOn())
		{
			// Block turning on if depleted/locked
			PlayAudioEffect(turnOffSound);
			return;
		}
        
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

	public float GetBatteryPercent()
	{
		if (batteryCapacitySeconds <= 0f) return 1f;
		return Mathf.Clamp01(batterySecondsRemaining / batteryCapacitySeconds);
	}

	public bool IsDepleted()
	{
		return batterySecondsRemaining <= 0.0001f;
	}

	public bool IsRecharging()
	{
		return !isFlashlightOn && batterySecondsRemaining < batteryCapacitySeconds && rechargeTimer >= rechargeDelaySeconds;
	}

	// Returns true if the flashlight is currently illuminating the given world position (within cone and range)
	public bool IsIlluminating(Vector3 worldPosition, bool requireLineOfSight = true, LayerMask obstructionMask = new LayerMask())
	{
		if (!isFlashlightOn || flashlightLight == null) return false;

		Vector3 origin = flashlightLight.transform.position;
		Vector3 toTarget = worldPosition - origin;
		float distance = toTarget.magnitude;
		if (distance > flashlightLight.range) return false;

		// Angle check for spotlights
		if (flashlightLight.type == LightType.Spot)
		{
			float halfAngle = flashlightLight.spotAngle * 0.5f;
			float angle = Vector3.Angle(flashlightLight.transform.forward, toTarget);
			if (angle > halfAngle) return false;
		}

		if (!requireLineOfSight) return true;

		int mask = (obstructionMask.value == 0) ? ~0 : obstructionMask.value;
		// If something blocks the path, it's not illuminated. We use RaycastAll to allow caller-side ignoring of self via overload.
		RaycastHit[] hits = Physics.RaycastAll(origin, toTarget.normalized, distance, mask, QueryTriggerInteraction.Ignore);
		// If there are any hits, consider blocked by default.
		return hits.Length == 0;
	}

	// Overload that ignores a specific transform (and its children) when checking LOS
	public bool IsIlluminating(Transform ignoreTransform, bool requireLineOfSight = true, LayerMask obstructionMask = new LayerMask())
	{
		if (!isFlashlightOn || flashlightLight == null || ignoreTransform == null) return false;

		Vector3 origin = flashlightLight.transform.position;
		Vector3 toTarget = ignoreTransform.position - origin;
		float distance = toTarget.magnitude;
		if (distance > flashlightLight.range) return false;

		if (flashlightLight.type == LightType.Spot)
		{
			float halfAngle = flashlightLight.spotAngle * 0.5f;
			float angle = Vector3.Angle(flashlightLight.transform.forward, toTarget);
			if (angle > halfAngle) return false;
		}

		if (!requireLineOfSight) return true;

		int mask = (obstructionMask.value == 0) ? ~0 : obstructionMask.value;
		RaycastHit[] hits = Physics.RaycastAll(origin, toTarget.normalized, distance, mask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < hits.Length; i++)
		{
			Transform hitTr = hits[i].transform;
			if (hitTr == null) continue;
			if (hitTr == ignoreTransform || hitTr.IsChildOf(ignoreTransform))
				continue; // ignore self
			// Any other hit blocks
			return false;
		}
		return true;
	}

    // Method to update flashlight properties at runtime
    public void UpdateFlashlightProperties(float intensity, float range, Color color)
    {
        if (flashlightLight == null) return;
        
        flashlightLight.intensity = intensity;
        flashlightLight.range = range;
        flashlightLight.color = color;
    }

	private bool CanTurnOn()
	{
		if (!lockUseWhenDepleted) return true;
		if (batterySecondsRemaining > 0.0001f) return true;
		// Depleted: require some recharge before allowing turn-on
		return false;
	}

	private void UpdateBattery()
	{
		// Clamp capacity changes safely
		batteryCapacitySeconds = Mathf.Max(0f, batteryCapacitySeconds);
		batterySecondsRemaining = Mathf.Clamp(batterySecondsRemaining, 0f, Mathf.Max(0.0001f, batteryCapacitySeconds));

		if (isFlashlightOn)
		{
			// Drain
			if (batteryCapacitySeconds > 0f)
			{
				batterySecondsRemaining -= Time.deltaTime;
				if (batterySecondsRemaining <= 0f)
				{
					batterySecondsRemaining = 0f;
					// Auto turn off and start recharge delay
					SetFlashlightState(false);
					rechargeTimer = 0f;
					PlayAudioEffect(turnOffSound);
				}
			}
		}
		else
		{
			// Not on: advance recharge timer
			rechargeTimer += Time.deltaTime;
			if (batterySecondsRemaining < batteryCapacitySeconds && rechargeTimer >= rechargeDelaySeconds)
			{
				batterySecondsRemaining += rechargeRatePerSecond * Time.deltaTime;
				if (batterySecondsRemaining >= batteryCapacitySeconds)
				{
					batterySecondsRemaining = batteryCapacitySeconds;
				}
			}
		}
	}
}