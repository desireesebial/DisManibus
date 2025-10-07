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
    
    private int Presses;
    private string result;
    private string ScreenText;
    private int reset;

    void Start()
    {
      
        Code = new int[(Convert.ToInt32(CodeLength))];
        Presses = 0;
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
                        
                        // Play button press sound
                        PlayButtonPressSound();
                    }
                }
               if (Presses == Convert.ToInt32(CodeLength))
                {
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
                var target = objectsToHideOnSuccess[i];
                if (target != null && target.activeSelf)
                {
                    target.SetActive(false);
                    changed = true;
                }
            }
        }
        
        return changed;
    }
    
    private void OnCorrectCodeEntered()
    {
        // Play success sound
        PlaySuccessSound();
        
        // Unlock and/or open door if configured
        if (targetDoor != null && unlockDoorOnSuccess)
        {
            targetDoor.UnlockDoor();
            
            if (openDoorAfterUnlock)
            {
                // Small delay before opening door for better feel
                StartCoroutine(OpenDoorAfterDelay(0.5f));
            }

            targetDoor.ApplyKeypadVisibility();
        }
        
        bool visibilityChanged = ApplySuccessVisibility();
        
        // Keep the correct code displayed for a moment, then reset
        if (!visibilityChanged)
        {
            StartCoroutine(ResetAfterDelay(2f));
        }
    }
    
    private void OnWrongCodeEntered()
    {
        // Play fail sound
        PlayFailSound();
    }
    
    private void ResetKeypad()
    {
        Presses = 0;
        reset = Convert.ToInt32(CodeLength) - 1;
        do
        {
            Code[reset] = 0;
            reset -= 1;
        } while (reset > -1);
    }
    
    private IEnumerator OpenDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (targetDoor != null && !targetDoor.IsLocked())
        {
            targetDoor.ToggleDoor();
        }
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
