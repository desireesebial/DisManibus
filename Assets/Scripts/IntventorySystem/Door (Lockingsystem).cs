using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public int requiredKeyID = 101;
    public string doorName = "Door";
    public PlayerInventory playerInventory;

    [Header("Door Components")]
    public Animator doorAnimator;

    [Header("Audio Components")]
    public AudioSource doorAudioSource;
    [SerializeField] AudioClip doorOpenSound;
    [SerializeField] AudioClip doorCloseSound;
    [SerializeField] AudioClip doorLockedSound;
    [SerializeField] AudioClip doorCreakSound;
    [SerializeField] AudioClip keyInsertSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float soundVolume = 1f;
    [Range(0.5f, 2f)] public float soundPitch = 1f;
    public bool randomizePitch = true;
    [Range(0f, 0.3f)] public float pitchVariation = 0.1f;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;

    private bool isOpen = false;
    private bool playerInRange = false;
    private bool isAnimating = false;

    void Start()
    {
        SetupAudioSource();
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(interactionKey) && !isAnimating)
        {
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                TryOpenDoor();
            }
        }
    }

    private void SetupAudioSource()
    {
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
            if (doorAudioSource == null)
            {
                doorAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        doorAudioSource.volume = soundVolume;
        doorAudioSource.pitch = soundPitch;
        doorAudioSource.spatialBlend = 1f;
        doorAudioSource.rolloffMode = AudioRolloffMode.Linear;
        doorAudioSource.maxDistance = 10f;
    }

    void CheckPlayerDistance()
    {
        if (playerInventory != null)
        {
            float distance = Vector3.Distance(transform.position, playerInventory.transform.position);
            playerInRange = distance <= interactionRange;
        }
    }

    void TryOpenDoor()
    {
        if (HasRequiredKey())
        {
            PlaySound(keyInsertSound, 0.8f);
            StartCoroutine(DelayedOpenDoor(0.3f));
        }
        else
        {
            Debug.Log($"You need a key (ID: {requiredKeyID}) to open the {doorName}!");
            PlaySound(doorLockedSound, 1f);

            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Locked");
            }
        }
    }

    IEnumerator DelayedOpenDoor(float delay)
    {
        isAnimating = true;
        yield return new WaitForSeconds(delay);
        OpenDoor();
    }

    bool HasRequiredKey()
    {
        if (playerInventory == null) return false;

        foreach (KeyItemsSO item in playerInventory.inventoryList)
        {
            if (item != null && item.item_type == itemType.Keys && item.itemID == requiredKeyID)
            {
                return true;
            }
        }
        return false;
    }

    void OpenDoor()
    {
        isOpen = true;
        Debug.Log($"{doorName} opened!");

        PlaySound(doorOpenSound, 1f);

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", true);
        }

        if (doorCreakSound != null)
        {
            StartCoroutine(PlayDelayedSound(doorCreakSound, 0.8f, 0.6f));
        }

        isAnimating = false;
    }

    void CloseDoor()
    {
        isOpen = false;
        isAnimating = true;
        Debug.Log($"{doorName} closed!");

        PlaySound(doorCloseSound, 1f);

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", false);
        }

        StartCoroutine(ResetAnimationFlag(1f));
    }

    IEnumerator ResetAnimationFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAnimating = false;
    }

    IEnumerator PlayDelayedSound(AudioClip clip, float delay, float volumeMultiplier = 1f)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(clip, volumeMultiplier);
    }

    void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && doorAudioSource != null)
        {
            doorAudioSource.volume = soundVolume * volumeMultiplier;

            if (randomizePitch)
            {
                doorAudioSource.pitch = soundPitch + Random.Range(-pitchVariation, pitchVariation);
            }
            else
            {
                doorAudioSource.pitch = soundPitch;
            }

            doorAudioSource.PlayOneShot(clip);
        }
    }

    void OnGUI()
    {
        if (playerInRange && !isAnimating)
        {
            string prompt = isOpen ? $"Press {interactionKey} to close {doorName}"
                                   : $"Press {interactionKey} to open {doorName}";

            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30), prompt);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (doorAudioSource != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, doorAudioSource.maxDistance);
        }
    }
}