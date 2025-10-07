using UnityEngine;

// Simple: attach to your flashlight model prefab. Use PickUp/Drop from inventory.
public class PhysicalFlashlightController : MonoBehaviour
{
    public enum FlashlightState { World, Held }

    [Header("Required")]
    public Light flashlightLight;           // Spot Light for the beam (assign from child)

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;   // Toggle key
    [Tooltip("If true, allows toggling even while still in the world (not picked up).")]
    public bool allowToggleWhenWorld = false;
    [Tooltip("If true, the light will turn ON automatically right after PickUp (never in world).")]
    public bool startOn = false;

    [Header("Battery")]
    [Tooltip("Total usable seconds from full to empty while the light is ON.")]
    public float batteryCapacitySeconds = 60f;
    [Tooltip("Current remaining usable seconds.")]
    public float batterySecondsRemaining = 60f;
    [Tooltip("Delay (seconds) after turning off/depleting before recharge begins.")]
    public float rechargeDelaySeconds = 4f;
    [Tooltip("Recharge speed in battery seconds regained per real second (when OFF).")]
    public float rechargeRatePerSecond = 6f;
    [Tooltip("If true, you cannot turn the light ON when the battery is fully depleted until some recharge has occurred.")]
    public bool lockUseWhenDepleted = true;

    [Header("Hold Settings")]
    public Transform defaultHoldSocket;     // Player hand/camera socket
    public Vector3 heldLocalPosition;       // Offsets when held
    public Vector3 heldLocalEulerAngles;
    public Vector3 heldLocalScale = Vector3.one;

    [Header("Optional Physics (World)")]
    public Rigidbody worldRigidbody;        // If present, enabled in World, disabled when Held
    public Collider[] worldColliders;       // Auto-detected if left empty

    public FlashlightState CurrentState { get; private set; } = FlashlightState.World;
    public bool IsOn => flashlightLight != null && flashlightLight.enabled;

    Transform originalParent;
    Vector3 originalLocalPosition;
    Vector3 originalLocalEuler;
    Vector3 originalLocalScale;
    float rechargeTimer;

    void Awake()
    {
        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light>(true);

        CacheOriginalTransform();
        AutoPopulateColliders();

        // Always start OFF in world so pickups don't "disappear" from lighting changes
        SetLightState(false);
        ApplyState(CurrentState);
    }

    void Update()
    {
        bool canToggle = CurrentState == FlashlightState.Held || allowToggleWhenWorld;
        if (canToggle && Input.GetKeyDown(toggleKey))
            Toggle();

        UpdateBattery();
    }

    public void Toggle()
    {
        if (!IsOn)
        {
            if (!CanTurnOn()) return;
            SetLightState(true);
        }
        else
        {
            SetLightState(false);
        }
    }

    public void SetLightState(bool on)
    {
        if (flashlightLight != null)
            flashlightLight.enabled = on;
    }

    // Call when inventory selects/puts flashlight in hand
    public void PickUp(Transform holdSocket = null)
    {
        if (holdSocket == null) holdSocket = defaultHoldSocket;
        if (holdSocket != null)
        {
            transform.SetParent(holdSocket);
            transform.localPosition = heldLocalPosition;
            transform.localRotation = Quaternion.Euler(heldLocalEulerAngles);
            transform.localScale = heldLocalScale;
        }

        CurrentState = FlashlightState.Held;
        ApplyState(CurrentState);

        // Optionally turn ON when picked up (only if battery allows)
        if (startOn && !IsOn && CanTurnOn())
            SetLightState(true);
    }

    // Call if you ever need to return it to the world (not required if you never drop)
    public void Drop()
    {
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = Quaternion.Euler(originalLocalEuler);
        transform.localScale = originalLocalScale;

        CurrentState = FlashlightState.World;
        ApplyState(CurrentState);
    }

    void ApplyState(FlashlightState state)
    {
        bool enablePhysics = state == FlashlightState.World;

        if (worldRigidbody != null)
        {
            worldRigidbody.isKinematic = !enablePhysics;
            worldRigidbody.useGravity = enablePhysics;
        }

        if (worldColliders != null)
        {
            for (int i = 0; i < worldColliders.Length; i++)
            {
                if (worldColliders[i] != null)
                    worldColliders[i].enabled = enablePhysics;
            }
        }
    }

    void CacheOriginalTransform()
    {
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalEuler = transform.localEulerAngles;
        originalLocalScale = transform.localScale;
    }

    void AutoPopulateColliders()
    {
        if (worldColliders != null && worldColliders.Length > 0) return;
        worldColliders = GetComponentsInChildren<Collider>(true);
    }

    void OnValidate()
    {
        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light>(true);
    }

    bool CanTurnOn()
    {
        if (!lockUseWhenDepleted) return true;
        return batterySecondsRemaining > 0.0001f;
    }

    void UpdateBattery()
    {
        // Infinite battery if capacity <= 0
        batteryCapacitySeconds = Mathf.Max(0f, batteryCapacitySeconds);
        if (batteryCapacitySeconds <= 0f)
        {
            batterySecondsRemaining = 0f;
            return;
        }

        batterySecondsRemaining = Mathf.Clamp(batterySecondsRemaining, 0f, batteryCapacitySeconds);

        if (IsOn)
        {
            // Drain while ON
            batterySecondsRemaining -= Time.deltaTime;
            if (batterySecondsRemaining <= 0f)
            {
                batterySecondsRemaining = 0f;
                SetLightState(false);
                rechargeTimer = 0f;
            }
        }
        else
        {
            // OFF: count delay, then recharge
            rechargeTimer += Time.deltaTime;
            if (batterySecondsRemaining < batteryCapacitySeconds && rechargeTimer >= rechargeDelaySeconds)
            {
                batterySecondsRemaining += rechargeRatePerSecond * Time.deltaTime;
                if (batterySecondsRemaining >= batteryCapacitySeconds)
                    batterySecondsRemaining = batteryCapacitySeconds;
            }
        }
    }

    public float GetBatteryPercent()
    {
        if (batteryCapacitySeconds <= 0f) return 1f;
        return Mathf.Clamp01(batterySecondsRemaining / batteryCapacitySeconds);
    }
}