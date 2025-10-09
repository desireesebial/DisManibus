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

    [Header("Existing Flashlight (Optional)")]
    [Tooltip("Assign an existing Light component if the flashlight already exists in the scene or prefab. Leave empty to auto-create one on the player camera.")]
    public Light flashlightLight;
    [Tooltip("Optional root transform for the flashlight model/light. Used when re-parenting or toggling visuals.")]
    public Transform flashlightRoot;
    [Tooltip("Optional visual GameObject (mesh, particles, etc.) to toggle alongside the light when switching on/off.")]
    public GameObject flashlightVisual;
    [Tooltip("If true, the assigned flashlightRoot will be parented to the detected player camera on start.")]
    public bool parentFlashlightRootToCamera = false;
    [Tooltip("Local position applied to the flashlightRoot after parenting to the camera.")]
    public Vector3 flashlightRootLocalPosition = Vector3.zero;
    [Tooltip("Local Euler rotation (degrees) applied to the flashlightRoot after parenting to the camera.")]
    public Vector3 flashlightRootLocalEulerAngles = Vector3.zero;
    [Tooltip("If true, flashlightVisual (if assigned) is activated only while the light is ON. Make sure the visual is not the same GameObject as this controller.")]
    public bool toggleVisualWithLight = false;
    [Tooltip("If false, the controller will not overwrite the Light component's existing settings when using an assigned flashlightLight.")]
    public bool applyInspectorSettingsToExistingLight = true;
    
    [Header("Audio")]
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

	[Header("Pickup System")]
	[Tooltip("If TRUE: Player must pick up a FlashlightPickup object before using the flashlight. If FALSE: Flashlight works immediately without pickup.")]
	public bool requirePickup = true;
	[Tooltip("Has the player picked up the flashlight? Set this to TRUE to start with flashlight already unlocked (skips pickup requirement).")]
	public bool hasFlashlight = false;
	[Tooltip("Message displayed when player tries to use flashlight before picking it up.")]
	public string needPickupMessage = "You need to find a flashlight first!";

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
    private AudioSource audioSource;
    private Transform playerCamera;
	private float rechargeTimer;
    private bool warnedAboutSelfTogglingVisual;

    private void Start()
    {
        // Find the player camera to attach the flashlight to
        FindPlayerCamera();

        // Resolve or create the flashlight light component
        ResolveFlashlightLight();

        // Setup audio source
        SetupAudioSource();

        if (flashlightLight == null)
        {
            Debug.LogWarning($"[{name}] FlashlightController could not find or create a Light component. Flashlight functionality will be disabled until one is assigned.");
        }
        else
        {
            Debug.Log("Flashlight Controller initialized. Press T to toggle flashlight.");
        }
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
        if (playerCamera == null)
        {
            Debug.LogWarning($"[{name}] Cannot create flashlight light because no player camera was found.");
            return;
        }

        // Create a child object for the flashlight light
        GameObject flashlightObject = new GameObject("FlashlightLight");
        flashlightRoot = flashlightObject.transform;
        flashlightRoot.SetParent(playerCamera);
        flashlightRoot.localPosition = flashlightRootLocalPosition;
        flashlightRoot.localRotation = Quaternion.Euler(flashlightRootLocalEulerAngles);

        // Add and configure the light component
        flashlightLight = flashlightObject.AddComponent<Light>();
        ApplyLightSettings(force: true);

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
            // Check if player has picked up the flashlight
            if (requirePickup && !hasFlashlight)
            {
                Debug.Log(needPickupMessage);
                return;
            }
            
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        EnsureFlashlightLight();
        if (flashlightLight == null)
        {
            Debug.LogWarning($"[{name}] No Light assigned to FlashlightController. Assign a Light or enable auto-create to toggle the flashlight.");
            return;
        }

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

		UpdateFlashlightVisual();
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
        isFlashlightOn = state;
        EnsureFlashlightLight();
        if (flashlightLight != null)
        {
            flashlightLight.enabled = state;
        }
        UpdateFlashlightVisual();
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

    /// <summary>
    /// Called by FlashlightPickup when the player collects the flashlight item.
    /// This unlocks the flashlight for use.
    /// </summary>
    public void PickupFlashlight()
    {
        hasFlashlight = true;
        Debug.Log($"[FlashlightController] Flashlight acquired! Press {flashlightKey} to toggle.");
    }

    public bool HasFlashlight()
    {
        return hasFlashlight;
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
        lightIntensity = intensity;
        lightRange = range;
        lightColor = color;

        if (flashlightLight == null)
        {
            EnsureFlashlightLight();
        }

        ApplyLightSettings(force: true);
    }

    private void ResolveFlashlightLight()
    {
        bool createdNewLight = false;

        if (flashlightLight == null)
        {
            flashlightLight = TryFindLightInHierarchy();
        }

        if (flashlightLight == null && playerCamera != null)
        {
            CreateFlashlightLight();
            createdNewLight = flashlightLight != null;
        }

        if (flashlightLight == null)
        {
            UpdateFlashlightVisual();
            return;
        }

        if (flashlightRoot == null)
        {
            flashlightRoot = flashlightLight.transform;
        }

        ConfigureFlashlightParenting();

        ApplyLightSettings(force: createdNewLight || applyInspectorSettingsToExistingLight);

        flashlightLight.enabled = isFlashlightOn;
        UpdateFlashlightVisual();
    }

    private void EnsureFlashlightLight()
    {
        if (flashlightLight != null) return;
        ResolveFlashlightLight();
    }

    private void ConfigureFlashlightParenting()
    {
        if (!parentFlashlightRootToCamera || flashlightRoot == null || playerCamera == null)
            return;

        flashlightRoot.SetParent(playerCamera);
        flashlightRoot.localPosition = flashlightRootLocalPosition;
        flashlightRoot.localRotation = Quaternion.Euler(flashlightRootLocalEulerAngles);
    }

    private Light TryFindLightInHierarchy()
    {
        Light lightFromRoot = null;

        if (flashlightRoot != null)
        {
            lightFromRoot = flashlightRoot.GetComponentInChildren<Light>(true);
        }

        if (lightFromRoot != null)
        {
            return lightFromRoot;
        }

        // Search beneath this controller but ignore the controller's own GameObject if it already has a light assigned to flashlightLight
        Light[] lights = GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;
            if (flashlightLight != null && lights[i] == flashlightLight) continue;
            return lights[i];
        }

        return null;
    }

    private void ApplyLightSettings(bool force = false)
    {
        if (flashlightLight == null) return;
        if (!force && !applyInspectorSettingsToExistingLight) return;

        flashlightLight.type = lightType;
        flashlightLight.intensity = lightIntensity;
        flashlightLight.range = lightRange;
        flashlightLight.color = lightColor;

        if (flashlightLight.type == LightType.Spot)
        {
            flashlightLight.spotAngle = spotAngle;
        }
    }

    private void UpdateFlashlightVisual()
    {
        if (!toggleVisualWithLight || flashlightVisual == null) return;

        if (flashlightVisual == gameObject)
        {
            if (!warnedAboutSelfTogglingVisual)
            {
                Debug.LogWarning($"[{name}] Flashlight visual is the same GameObject as the controller. Skipping visual toggle to avoid disabling the controller.");
                warnedAboutSelfTogglingVisual = true;
            }
            return;
        }

        flashlightVisual.SetActive(isFlashlightOn);
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
					UpdateFlashlightVisual();
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