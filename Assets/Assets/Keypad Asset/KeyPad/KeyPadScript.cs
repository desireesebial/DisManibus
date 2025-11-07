using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyPadScript : MonoBehaviour
{
    [Header("Keypad Settings")]
    public int[] Code;
    public string CodeLength;
    public string Correct;
    public GameObject Screen;
    
    [Header("Raycast")]
    public float raycastDistance = 100f; // Max distance for keypad raycasts
    public LayerMask raycastLayerMask; // If left empty (0), defaults to Physics.DefaultRaycastLayers
    public Camera raycastCamera; // Optional override; if null, will fallback to MainCamera or any enabled camera
    
    [Header("Door Integration")]
    public doorscript targetDoor;
    public bool unlockDoorOnSuccess = true;
    public bool openDoorAfterUnlock = true;
    
    [Header("Visibility On Success")]
    public GameObject[] objectsToHideOnSuccess;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip buttonPressSound;

    [Header("Kamatayan Integration (KeyPad2 only)")]
    public bool isKeyPad2 = false; // Set to true for KeyPad2
    public KamatayanController kamatayanController; // Reference to Kamatayan

    private int Presses;
    private string result;
    private string ScreenText;
    private int reset;
    private bool isProcessingCode = false; // Prevents multiple simultaneous code processing

    void Start()
    {
        // Validate CodeLength on start
        int codeLen = 4; // Default
        try
        {
            codeLen = Convert.ToInt32(CodeLength);
            if (codeLen <= 0 || codeLen > 20)
            {
                Debug.LogWarning($"[KeyPad] Invalid CodeLength: {codeLen}. Using default 4.");
                codeLen = 4;
                CodeLength = "4";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[KeyPad] Error parsing CodeLength '{CodeLength}': {e.Message}. Using default 4.");
            codeLen = 4;
            CodeLength = "4";
        }
        
        Code = new int[codeLen];
        Presses = 0;
        isProcessingCode = false;
        
        Debug.Log($"[KeyPad] Initialized with code length: {codeLen}, Correct code: {Correct}");
    }
    void Update()
    {
        ScreenText = string.Join("", Code.Select(i => i.ToString()).ToArray());
        // Safely update display text if a TextMeshPro component exists
        if (Screen != null)
        {
            var tmp = Screen.GetComponent<TMPro.TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = ScreenText;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {

            var cam = GetRaycastCamera();
            if (cam == null)
            {
                Debug.LogWarning("[KeyPad] No camera available for raycasting. Ensure a camera is tagged as MainCamera or assign one on the KeyPadScript.");
                return;
            }
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int effectiveMask = GetEffectiveLayerMask();
            if (Physics.Raycast(ray, out hit, raycastDistance, effectiveMask))
            {
                
               if(Presses < Convert.ToInt32(CodeLength))
                {
                    if (hit.transform.gameObject.name == "Base") { }
                    else
                    {
                        Debug.Log(hit.transform.gameObject.name);
                        if (!TryGetButtonNumber(hit.transform.gameObject, out int buttonNumber))
                        {
                            Debug.LogWarning($"[KeyPad] Unable to resolve button number for '{hit.transform.gameObject.name}'. Ensure it has a Number component or a digit in the name.");
                            return;
                        }
                        Code[Presses] = buttonNumber;
                        Presses += 1;

                        // Check if this is KeyPad2 and 6th digit was just entered
                        if (isKeyPad2 && Presses == 6)
                        {
                            Debug.Log("[KeyPad2] 6th digit entered - triggering Kamatayan teleport!");
                            if (kamatayanController != null)
                            {
                                kamatayanController.TeleportToPointLight24();
                            }
                            else
                            {
                                Debug.LogWarning("[KeyPad2] KamatayanController reference not set!");
                            }
                        }

                        // Play button press sound
                        PlayButtonPressSound();
                    }
                }
               if (Presses == Convert.ToInt32(CodeLength) && !isProcessingCode)
                {
                   isProcessingCode = true;
                   result = String.Join("", new List<int>(Code).ConvertAll(i => i.ToString()).ToArray());
                    Debug.Log(result);
                    if(Correct == result)
                    {
                        // Correct code entered
                        Debug.Log("The Code Entered Is Correct");
                        OnCorrectCodeEntered();
                    }
                    else
                    {
                        // Wrong code entered
                        Debug.Log("Wrong code entered. Resetting keypad.");
                        OnWrongCodeEntered();
                        ResetKeypad();
                    }
                    isProcessingCode = false;
                }
            }

        }
    }
    
    private Camera GetRaycastCamera()
    {
        if (raycastCamera != null && raycastCamera.isActiveAndEnabled)
            return raycastCamera;
        if (Camera.main != null && Camera.main.isActiveAndEnabled)
            return Camera.main;
        if (Camera.current != null && Camera.current.isActiveAndEnabled)
            return Camera.current;
        var all = Camera.allCameras;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].isActiveAndEnabled)
                return all[i];
        }
        return null;
    }

    private int GetEffectiveLayerMask()
    {
        // When not set in inspector (value == 0), fallback to Unity's default raycast layers
        return raycastLayerMask.value == 0 ? Physics.DefaultRaycastLayers : raycastLayerMask.value;
    }

    private bool TryGetButtonNumber(GameObject buttonObject, out int number)
    {
        number = 0;

        if (buttonObject == null)
            return false;

        // Primary: dedicated Number component on the object
        var numberComponent = buttonObject.GetComponent<Number>();
        if (numberComponent != null)
        {
            number = numberComponent.number;
            return true;
        }

        // Secondary: Number component on children (safety for nested layouts)
        numberComponent = buttonObject.GetComponentInChildren<Number>();
        if (numberComponent != null)
        {
            number = numberComponent.number;
            return true;
        }

        // Fallback: parse trailing digit from object name (e.g., "Button5")
        string name = buttonObject.name;
        if (!string.IsNullOrEmpty(name))
        {
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(name[i]))
                {
                    number = name[i] - '0';
                    return true;
                }
            }
        }

        // Final fallback: check for TextMeshPro display to infer number visually
        var text = buttonObject.GetComponentInChildren<TMPro.TMP_Text>();
        if (text != null && !string.IsNullOrEmpty(text.text))
        {
            var digits = text.text.Trim();
            if (digits.Length == 1 && char.IsDigit(digits[0]))
            {
                number = digits[0] - '0';
                return true;
            }
        }

        return false;
    }
    
    private bool ApplySuccessVisibility()
    {
        bool changed = false;
        if (objectsToHideOnSuccess != null && objectsToHideOnSuccess.Length > 0)
        {
            for (int i = 0; i < objectsToHideOnSuccess.Length; i++)
            {
                try
                {
                    var target = objectsToHideOnSuccess[i];
                    if (target != null && target.activeSelf)
                    {
                        target.SetActive(false);
                        changed = true;
                        Debug.Log($"[KeyPad] Hidden object: {target.name}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[KeyPad] Error hiding object at index {i}: {e.Message}");
                }
            }
        }
        
        return changed;
    }
    
    private void OnCorrectCodeEntered()
    {
        Debug.Log("[KeyPad] Correct code entered! Processing...");
        
        // Play success sound
        try
        {
            PlaySuccessSound();
        }
        catch (Exception e)
        {
            Debug.LogError($"[KeyPad] Error playing success sound: {e.Message}");
        }
        
        // Unlock and/or open door if configured
        if (targetDoor != null && targetDoor.gameObject != null && unlockDoorOnSuccess)
        {
            try
            {
                // Ensure door is active before unlocking/opening
                if (!targetDoor.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Target door {targetDoor.gameObject.name} is inactive. Activating it.");
                    targetDoor.gameObject.SetActive(true);
                }
                
                Debug.Log($"[KeyPad] Unlocking door: {targetDoor.gameObject.name}");
                targetDoor.UnlockDoor();
                
                if (openDoorAfterUnlock)
                {
                    Debug.Log("[KeyPad] Starting door open coroutine...");
                    // Small delay before opening door for better feel
                    StartCoroutine(OpenDoorAfterDelay(0.5f));
                }

                targetDoor.ApplyKeypadVisibility();
                Debug.Log("[KeyPad] Door unlocking sequence completed successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[KeyPad] Error during door unlock sequence: {e.Message}\n{e.StackTrace}");
            }
        }
        else if (targetDoor == null)
        {
            Debug.LogWarning("[KeyPad] No target door assigned. Code correct but no door to unlock.");
        }
        
        bool visibilityChanged = false;
        try
        {
            visibilityChanged = ApplySuccessVisibility();
        }
        catch (Exception e)
        {
            Debug.LogError($"[KeyPad] Error applying visibility: {e.Message}");
        }
        
        // Keep the correct code displayed for a moment, then reset
        try
        {
            if (!visibilityChanged)
            {
                StartCoroutine(ResetAfterDelay(2f));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[KeyPad] Error starting reset coroutine: {e.Message}");
            // Fallback: reset immediately if coroutine fails
            ResetKeypad();
        }
        
        Debug.Log("[KeyPad] OnCorrectCodeEntered completed.");
    }
    
    private void OnWrongCodeEntered()
    {
        // Play fail sound
        PlayFailSound();
    }
    
    private void ResetKeypad()
    {
        Presses = 0;
        isProcessingCode = false; // Reset processing flag
        
        // Safety check to prevent infinite loops
        int codeLen = Convert.ToInt32(CodeLength);
        if (codeLen <= 0 || codeLen > 100)
        {
            Debug.LogError($"[KeyPad] Invalid CodeLength: {codeLen}. Resetting to 4.");
            CodeLength = "4";
            codeLen = 4;
            Code = new int[codeLen];
        }
        
        // Use safer for loop instead of do-while
        for (int i = 0; i < Code.Length; i++)
        {
            Code[i] = 0;
        }
    }
    
    private IEnumerator OpenDoorAfterDelay(float delay)
    {
        Debug.Log($"[KeyPad] OpenDoorAfterDelay started. Waiting {delay} seconds...");
        yield return new WaitForSeconds(delay);
        
        Debug.Log("[KeyPad] Delay complete. Attempting to open door...");
        
        if (targetDoor != null && targetDoor.gameObject != null)
        {
            // Ensure door is active before attempting to toggle
            bool needsActivation = !targetDoor.gameObject.activeInHierarchy;
            if (needsActivation)
            {
                try
                {
                    Debug.LogWarning($"Target door {targetDoor.gameObject.name} is inactive. Activating it before opening.");
                    targetDoor.gameObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[KeyPad] Error activating door: {e.Message}");
                    yield break;
                }
                
                // Wait one frame after activation
                yield return null;
            }
            
            try
            {
                if (!targetDoor.IsLocked())
                {
                    Debug.Log($"[KeyPad] Toggling door {targetDoor.gameObject.name}...");
                    targetDoor.ToggleDoor();
                    Debug.Log("[KeyPad] Door toggle command sent successfully.");
                }
                else
                {
                    Debug.LogWarning($"[KeyPad] Door {targetDoor.gameObject.name} is still locked!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[KeyPad] Exception in OpenDoorAfterDelay: {e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError("[KeyPad] Target door is null or destroyed. Cannot open door.");
        }
        
        Debug.Log("[KeyPad] OpenDoorAfterDelay completed.");
    }
    
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetKeypad();
    }
    
    private void PlayButtonPressSound()
    {
        if (audioSource != null && buttonPressSound != null)
        {
            audioSource.PlayOneShot(buttonPressSound);
        }
    }
    
    private void PlaySuccessSound()
    {
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
    }
    
    private void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
        }
    }
    
    // Public method to manually reset the keypad
    public void ManualReset()
    {
        ResetKeypad();
    }
    
    // Public method to set a new target door
    public void SetTargetDoor(doorscript door)
    {
        targetDoor = door;
    }
}
