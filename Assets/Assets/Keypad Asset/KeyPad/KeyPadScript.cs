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
    
    [Header("Door Integration")]
    public doorscript targetDoor;
    public bool unlockDoorOnSuccess = true;
    public bool openDoorAfterUnlock = true;
    
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
        Screen.GetComponent<TMPro.TextMeshPro>().text = ScreenText;

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10))
            {
                
               if(Presses < Convert.ToInt32(CodeLength))
                {
                    if (hit.transform.gameObject.name == "Base") { }
                    else
                    {
                        Debug.Log(hit.transform.gameObject.name);
                        int buttonNumber = hit.transform.gameObject.GetComponent<Number>().number;
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
        
        // Keep the correct code displayed for a moment, then reset
        StartCoroutine(ResetAfterDelay(2f));
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
