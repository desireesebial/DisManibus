using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DullahanHeadInventory : MonoBehaviour
{
    [Header("Dullahan Head Settings")]
    public List<DullahanHeadSO> headInventoryList = new List<DullahanHeadSO>();
    public int maxHeadInventorySize = 3; // Only 3 heads total
    public int selectedHeadIndex = -1; // -1 means no head selected

    [Header("Camera and UI")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;

    [Header("Head Inventory UI")]
    [SerializeField] Image[] headInventorySlotImage = new Image[3];
    [SerializeField] Image[] headInventoryBackgroundImage = new Image[3];
    [SerializeField] Sprite emptySlotImage;

    [Header("Input Keys")]
    [SerializeField] KeyCode throwHeadKey = KeyCode.G;
    [SerializeField] KeyCode pickUpHeadKey = KeyCode.E;
    [SerializeField] KeyCode attachHeadKey = KeyCode.F;
    [SerializeField] KeyCode toggleFlashlightKey = KeyCode.T;

    [Header("Player Head GameObjects")]
    [SerializeField] GameObject dullahanHead_Real;
    [SerializeField] GameObject dullahanHead_Fake1;
    [SerializeField] GameObject dullahanHead_Fake2;

    [Header("Head Prefabs for Dropping")]
    [SerializeField] GameObject dullahanHead_Real_prefab;
    [SerializeField] GameObject dullahanHead_Fake1_prefab;
    [SerializeField] GameObject dullahanHead_Fake2_prefab;

    [Header("Throwing Settings")]
    [SerializeField] GameObject throwObject_gameobject;
    [SerializeField] float throwForce = 5f;

    [Header("Flashlight System")]
    [SerializeField] Light flashlight;
    [SerializeField] float maxBatteryLife = 300f; // 5 minutes in seconds
    [SerializeField] float currentBatteryLife;
    [SerializeField] float batteryDrainRate = 1f; // Battery drain per second
    [SerializeField] float batteryRechargeRate = 0.5f; // Battery recharge per second when off
    [SerializeField] bool isFlashlightOn = false;
    [SerializeField] bool infiniteBattery = false;
    
    [Header("Flashlight Settings")]
    [SerializeField] float flashlightIntensity = 2f;
    [SerializeField] float flashlightRange = 15f;
    [SerializeField] Color flashlightColor = Color.white;
    [SerializeField] float flashlightAngle = 45f;
    
    [Header("Flashlight UI")]
    [SerializeField] Image batteryIndicator;
    [SerializeField] Color fullBatteryColor = Color.green;
    [SerializeField] Color lowBatteryColor = Color.red;
    [SerializeField] Color emptyBatteryColor = Color.gray;
    [SerializeField] GameObject flashlightUI;
    [SerializeField] TMPro.TextMeshProUGUI batteryText;

    [Header("Integration")]
    [SerializeField] PlayerInventory playerInventory; // Reference to main inventory
    [SerializeField] DullahanHeadEffectManager effectManager;
    [SerializeField] DullahanAudioManager audioManager;

    private Dictionary<HeadType, GameObject> headSetActive = new Dictionary<HeadType, GameObject>();
    private bool isInitialized = false;

    void Start()
    {
        // Initialize head dictionary with null checks
        InitializeHeadDictionary();

        // Set initial UI state
        UpdateHeadInventoryUI();

        // Find references if not assigned
        FindReferences();

        // Initialize flashlight
        InitializeFlashlight();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        HandleHeadThrowing();
        HandleHeadPickup();
        HandleHeadSelection();
        HandleHeadAttachment();
        HandleFlashlight();
        UpdateHeadInventoryUI();
        UpdateFlashlightUI();
    }

    private void FindReferences()
    {
        // Find PlayerInventory if not assigned
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        // Find EffectManager if not assigned
        if (effectManager == null)
            effectManager = FindObjectOfType<DullahanHeadEffectManager>();

        // Find AudioManager if not assigned
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();

        // Find camera if not assigned
        if (cam == null)
        {
            FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
            if (fpc != null)
                cam = fpc.playerCamera;
        }

        // Find flashlight if not assigned
        if (flashlight == null)
        {
            flashlight = FindObjectOfType<Light>();
            if (flashlight == null)
            {
                // Create flashlight
                GameObject flashlightObj = new GameObject("Flashlight");
                flashlightObj.transform.SetParent(cam != null ? cam.transform : transform);
                flashlight = flashlightObj.AddComponent<Light>();
            }
        }
    }

    private void InitializeHeadDictionary()
    {
        headSetActive.Clear();

        // Add heads to dictionary only if they exist
        if (dullahanHead_Real != null) 
            headSetActive.Add(HeadType.Real, dullahanHead_Real);
        if (dullahanHead_Fake1 != null) 
            headSetActive.Add(HeadType.Fake1, dullahanHead_Fake1);
        if (dullahanHead_Fake2 != null) 
            headSetActive.Add(HeadType.Fake2, dullahanHead_Fake2);

        // Initially deactivate all heads
        DeactivateAllHeads();
    }

    private void HandleHeadThrowing()
    {
        if (Input.GetKeyDown(throwHeadKey) && HasHeads())
        {
            DullahanHeadSO headToThrow = headInventoryList[selectedHeadIndex];

            // Drop head with physics
            DropHeadWithPhysics(headToThrow);

            // Remove from inventory
            headInventoryList.RemoveAt(selectedHeadIndex);

            // Adjust selected head index
            if (selectedHeadIndex >= headInventoryList.Count && headInventoryList.Count > 0)
            {
                selectedHeadIndex = headInventoryList.Count - 1;
            }
            else if (headInventoryList.Count == 0)
            {
                selectedHeadIndex = -1;
            }

            // Update selected head
            if (HasHeads())
            {
                NewHeadSelected();
            }
            else
            {
                DeactivateAllHeads();
            }
        }
    }

    private void HandleHeadPickup()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 3f)) // 3f reach for heads
        {
            DullahanHeadPickable headPickable = hitInfo.collider.GetComponent<DullahanHeadPickable>();

            if (headPickable != null && !headPickable.isPickedUp)
            {
                // Show pickup prompt
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(true);

                // Handle pickup input
                if (Input.GetKeyDown(pickUpHeadKey))
                {
                    TryPickupHead(headPickable);
                }
            }
            else
            {
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(false);
            }
        }
        else
        {
            if (pressToPickup_gameobject != null)
                pressToPickup_gameobject.SetActive(false);
        }
    }

    private void TryPickupHead(DullahanHeadPickable headPickable)
    {
        // Check if head inventory is full
        if (headInventoryList.Count >= maxHeadInventorySize)
        {
            Debug.Log("Head inventory is full!");
            return;
        }

        // Check if head has valid ScriptableObject
        if (headPickable.headData == null)
        {
            Debug.LogError("Head has no ScriptableObject assigned!");
            return;
        }

        // Add head to inventory
        headInventoryList.Add(headPickable.headData);

        // Apply effects if any
        if (headPickable.headData.hasEffect && effectManager != null)
        {
            effectManager.ApplyHeadEffect(headPickable.headData);
        }

        // Play pickup sound
        if (audioManager != null)
        {
            audioManager.PlayHeadPickupSound(headPickable.headData.headType);
        }

        // Pick up the head (destroys GameObject)
        headPickable.PickItem();

        // If this is the first head, select it
        if (headInventoryList.Count == 1)
        {
            selectedHeadIndex = 0;
            NewHeadSelected();
        }

        Debug.Log($"Picked up: {headPickable.headData.headName}");
    }

    private void HandleHeadSelection()
    {
        if (!HasHeads()) return;

        int newSelection = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1) && headInventoryList.Count > 0) newSelection = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && headInventoryList.Count > 1) newSelection = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && headInventoryList.Count > 2) newSelection = 2;

        if (newSelection != -1 && newSelection != selectedHeadIndex)
        {
            selectedHeadIndex = newSelection;
            NewHeadSelected();
        }
    }

    private void HandleHeadAttachment()
    {
        if (!HasHeads()) return;

        // Check if player is near Dullahan body
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 3f))
        {
            DullahanBody dullahanBody = hitInfo.collider.GetComponent<DullahanBody>();

            if (dullahanBody != null && Input.GetKeyDown(attachHeadKey))
            {
                DullahanHeadSO currentHead = headInventoryList[selectedHeadIndex];
                
                // Check if this is the real head
                if (currentHead.headType == HeadType.Real)
                {
                    // Attach head to Dullahan body
                    if (dullahanBody.AttachHead(currentHead))
                    {
                        // Remove head from inventory
                        headInventoryList.RemoveAt(selectedHeadIndex);

                        // Adjust selected head index
                        if (selectedHeadIndex >= headInventoryList.Count && headInventoryList.Count > 0)
                        {
                            selectedHeadIndex = headInventoryList.Count - 1;
                        }
                        else if (headInventoryList.Count == 0)
                        {
                            selectedHeadIndex = -1;
                        }

                        // Update selected head
                        if (HasHeads())
                        {
                            NewHeadSelected();
                        }
                        else
                        {
                            DeactivateAllHeads();
                        }

                        Debug.Log("Real head attached to Dullahan body!");
                    }
                }
                else
                {
                    Debug.Log("This is not the real head!");
                }
            }
        }
    }

    public void NewHeadSelected()
    {
        if (!HasHeads())
        {
            DeactivateAllHeads();
            return;
        }

        // Clamp selected head to valid range
        selectedHeadIndex = Mathf.Clamp(selectedHeadIndex, 0, headInventoryList.Count - 1);

        DeactivateAllHeads();

        DullahanHeadSO currentHead = headInventoryList[selectedHeadIndex];
        if (currentHead != null && headSetActive.ContainsKey(currentHead.headType))
        {
            GameObject headObject = headSetActive[currentHead.headType];
            if (headObject != null)
            {
                headObject.SetActive(true);
            }
        }
    }

    private void UpdateHeadInventoryUI()
    {
        // Update head inventory slot images
        for (int i = 0; i < headInventorySlotImage.Length; i++)
        {
            if (headInventorySlotImage[i] != null)
            {
                if (i < headInventoryList.Count && headInventoryList[i] != null && headInventoryList[i].headSprite != null)
                {
                    headInventorySlotImage[i].sprite = headInventoryList[i].headSprite;
                }
                else
                {
                    headInventorySlotImage[i].sprite = emptySlotImage;
                }
            }
        }

        // Update background colors for selection
        for (int i = 0; i < headInventoryBackgroundImage.Length; i++)
        {
            if (headInventoryBackgroundImage[i] != null)
            {
                if (i == selectedHeadIndex && HasHeads())
                {
                    headInventoryBackgroundImage[i].color = new Color32(145, 255, 126, 255); // Green for selected
                }
                else
                {
                    headInventoryBackgroundImage[i].color = new Color32(219, 219, 219, 255); // Default gray
                }
            }
        }
    }

    private void DropHeadWithPhysics(DullahanHeadSO headToDrop)
    {
        GameObject prefabToThrow = GetPrefabForHead(headToDrop);
        if (prefabToThrow == null) 
        {
            Debug.LogWarning($"No prefab found for head type: {headToDrop.headType}");
            return;
        }

        // Calculate throw position
        Vector3 throwPosition;
        if (throwObject_gameobject != null)
        {
            throwPosition = throwObject_gameobject.transform.position;
        }
        else if (cam != null)
        {
            throwPosition = cam.transform.position + cam.transform.forward * 1f;
        }
        else
        {
            throwPosition = transform.position + transform.forward * 1f;
        }

        // Instantiate the head
        GameObject droppedHead = Instantiate(prefabToThrow, throwPosition, Quaternion.identity);

        // Set up the dropped head's ScriptableObject reference
        DullahanHeadPickable droppedPickable = droppedHead.GetComponent<DullahanHeadPickable>();
        if (droppedPickable != null)
        {
            droppedPickable.headData = headToDrop;
            droppedPickable.isPickedUp = false;
        }

        // Configure physics
        Rigidbody rb = droppedHead.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedHead.AddComponent<Rigidbody>();
        }

        // Set physics properties
        rb.mass = 1.0f; // Head mass
        if (rb.linearDamping == 0) rb.linearDamping = 1f;
        if (rb.angularDamping == 0) rb.angularDamping = 5f;

        // Apply throw force
        Vector3 throwDirection;
        if (cam != null)
        {
            throwDirection = (cam.transform.forward + Vector3.up * 0.3f).normalized;
        }
        else
        {
            throwDirection = (transform.forward + Vector3.up * 0.3f).normalized;
        }

        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
    }

    private GameObject GetPrefabForHead(DullahanHeadSO head)
    {
        switch (head.headType)
        {
            case HeadType.Real: return dullahanHead_Real_prefab;
            case HeadType.Fake1: return dullahanHead_Fake1_prefab;
            case HeadType.Fake2: return dullahanHead_Fake2_prefab;
            default: return null;
        }
    }

    public void DeactivateAllHeads()
    {
        if (dullahanHead_Real != null) dullahanHead_Real.SetActive(false);
        if (dullahanHead_Fake1 != null) dullahanHead_Fake1.SetActive(false);
        if (dullahanHead_Fake2 != null) dullahanHead_Fake2.SetActive(false);
    }

    public bool HasHeads()
    {
        return headInventoryList != null && headInventoryList.Count > 0;
    }

    // Public methods for other scripts to use
    public bool HasHead(HeadType headType)
    {
        foreach (DullahanHeadSO head in headInventoryList)
        {
            if (head != null && head.headType == headType)
                return true;
        }
        return false;
    }

    public bool HasRealHead()
    {
        return HasHead(HeadType.Real);
    }

    public DullahanHeadSO GetCurrentHead()
    {
        if (HasHeads() && selectedHeadIndex >= 0 && selectedHeadIndex < headInventoryList.Count)
            return headInventoryList[selectedHeadIndex];
        return null;
    }

    public List<DullahanHeadSO> GetHeadsOfType(HeadType type)
    {
        List<DullahanHeadSO> heads = new List<DullahanHeadSO>();
        foreach (DullahanHeadSO head in headInventoryList)
        {
            if (head != null && head.headType == type)
                heads.Add(head);
        }
        return heads;
    }

    // Method to add head from main inventory (for integration)
    public bool AddHeadFromMainInventory(KeyItemsSO item)
    {
        if (headInventoryList.Count >= maxHeadInventorySize)
            return false;

        // Convert KeyItemsSO to DullahanHeadSO if possible
        // This would need to be implemented based on your specific needs
        return false;
    }

    private void InitializeFlashlight()
    {
        if (flashlight == null) return;

        // Setup flashlight properties
        flashlight.type = LightType.Spot;
        flashlight.intensity = 0f; // Start off
        flashlight.range = flashlightRange;
        flashlight.color = flashlightColor;
        flashlight.spotAngle = flashlightAngle;
        flashlight.enabled = false;

        // Initialize battery
        currentBatteryLife = maxBatteryLife;

        // Position flashlight at camera
        if (cam != null)
        {
            flashlight.transform.position = cam.transform.position;
            flashlight.transform.rotation = cam.transform.rotation;
        }
    }

    private void HandleFlashlight()
    {
        if (flashlight == null) return;

        // Toggle flashlight
        if (Input.GetKeyDown(toggleFlashlightKey))
        {
            ToggleFlashlight();
        }

        // Update flashlight position to follow camera
        if (cam != null)
        {
            flashlight.transform.position = cam.transform.position;
            flashlight.transform.rotation = cam.transform.rotation;
        }

        // Update battery
        UpdateBattery();
    }

    private void ToggleFlashlight()
    {
        if (currentBatteryLife <= 0f && !infiniteBattery) return;

        isFlashlightOn = !isFlashlightOn;
        flashlight.enabled = isFlashlightOn;

        if (isFlashlightOn)
        {
            flashlight.intensity = flashlightIntensity;
            // Play flashlight on sound
            if (audioManager != null)
            {
                audioManager.PlayFlashlightOnSound();
            }
        }
        else
        {
            flashlight.intensity = 0f;
            // Play flashlight off sound
            if (audioManager != null)
            {
                audioManager.PlayFlashlightOffSound();
            }
        }

        Debug.Log($"Flashlight {(isFlashlightOn ? "ON" : "OFF")}");
    }

    private bool batteryLowWarningPlayed = false;
    private bool batteryDeadWarningPlayed = false;

    private void UpdateBattery()
    {
        if (infiniteBattery) return;

        if (isFlashlightOn)
        {
            // Drain battery
            currentBatteryLife -= batteryDrainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(0f, currentBatteryLife);

            // Check for low battery warning
            float batteryPercentage = currentBatteryLife / maxBatteryLife;
            if (batteryPercentage <= 0.2f && !batteryLowWarningPlayed)
            {
                if (audioManager != null)
                {
                    audioManager.PlayBatteryLowSound();
                }
                batteryLowWarningPlayed = true;
                Debug.Log("Flashlight battery low!");
            }

            // Turn off flashlight if battery is dead
            if (currentBatteryLife <= 0f)
            {
                isFlashlightOn = false;
                flashlight.enabled = false;
                flashlight.intensity = 0f;
                
                if (!batteryDeadWarningPlayed)
                {
                    if (audioManager != null)
                    {
                        audioManager.PlayBatteryDeadSound();
                    }
                    batteryDeadWarningPlayed = true;
                }
                
                Debug.Log("Flashlight battery dead!");
            }
        }
        else
        {
            // Recharge battery when off
            currentBatteryLife += batteryRechargeRate * Time.deltaTime;
            currentBatteryLife = Mathf.Min(maxBatteryLife, currentBatteryLife);
            
            // Reset warnings when battery is recharged
            if (currentBatteryLife > maxBatteryLife * 0.3f)
            {
                batteryLowWarningPlayed = false;
                batteryDeadWarningPlayed = false;
            }
        }
    }

    private void UpdateFlashlightUI()
    {
        if (batteryIndicator == null) return;

        // Update battery indicator color
        float batteryPercentage = currentBatteryLife / maxBatteryLife;
        
        if (batteryPercentage > 0.5f)
        {
            batteryIndicator.color = fullBatteryColor;
        }
        else if (batteryPercentage > 0.2f)
        {
            batteryIndicator.color = lowBatteryColor;
        }
        else
        {
            batteryIndicator.color = emptyBatteryColor;
        }

        // Update battery indicator fill
        batteryIndicator.fillAmount = batteryPercentage;

        // Update battery text
        if (batteryText != null)
        {
            int batteryPercent = Mathf.RoundToInt(batteryPercentage * 100f);
            batteryText.text = $"{batteryPercent}%";
        }

        // Show/hide flashlight UI
        if (flashlightUI != null)
        {
            flashlightUI.SetActive(true);
        }
    }

    // Public methods for flashlight control
    public void TurnOnFlashlight()
    {
        if (currentBatteryLife > 0f || infiniteBattery)
        {
            isFlashlightOn = true;
            if (flashlight != null)
            {
                flashlight.enabled = true;
                flashlight.intensity = flashlightIntensity;
            }
        }
    }

    public void TurnOffFlashlight()
    {
        isFlashlightOn = false;
        if (flashlight != null)
        {
            flashlight.enabled = false;
            flashlight.intensity = 0f;
        }
    }

    public void RechargeBattery(float amount)
    {
        currentBatteryLife += amount;
        currentBatteryLife = Mathf.Min(maxBatteryLife, currentBatteryLife);
        Debug.Log($"Battery recharged by {amount} seconds. Current: {currentBatteryLife:F1}s");
    }

    public void SetInfiniteBattery(bool infinite)
    {
        infiniteBattery = infinite;
        Debug.Log($"Infinite battery: {(infinite ? "ON" : "OFF")}");
    }

    public float GetBatteryPercentage()
    {
        return currentBatteryLife / maxBatteryLife;
    }

    public bool IsFlashlightOn()
    {
        return isFlashlightOn;
    }
}
