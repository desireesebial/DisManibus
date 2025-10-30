using UnityEngine;
using TMPro;

public class DoorMessageHandler : MonoBehaviour
{
    [Header("References")]
    public doorscript door;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI promptText;
    public Transform player;
    public Camera playerCamera;
    public KeyCode interactKey = KeyCode.E;
    public DialogueManager dialogueManager;     // ✅ Reference to DialogueManager

    [Header("Interaction")]
    public float interactRange = 3f;
    public LayerMask doorMask = ~0;

    [Header("Feedback")]
    public float messageDuration = 1.0f;
    public AudioSource audioSource;
    public AudioClip lockedSound;
    public float shakeIntensity = 2f;
    public float shakeDuration = 0.2f;
    public string lockedDoorDialogue = "It's locked... maybe there's a key nearby."; // ✅ Custom line

    private Coroutine hideRoutine;
    private bool isShaking = false;
    private bool soundCooldown = false;
    private Collider doorCollider;

    void Awake()
    {
        if (!door) door = GetComponentInParent<doorscript>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!playerCamera) playerCamera = Camera.main;
        if (!audioSource && door) audioSource = door.doorAudioSource;
        if (!dialogueManager) dialogueManager = FindObjectOfType<DialogueManager>(); // ✅ Auto-find

        if (messageText) messageText.text = "";
        if (promptText) promptText.text = "";

        if (door) doorCollider = door.GetComponentInChildren<Collider>();
        if (!doorCollider && door) doorCollider = door.GetComponent<Collider>();
    }

    void Update()
    {
        if (!door || !player || !playerCamera) return;

        bool closeEnough = DistanceToDoor(player.position) <= interactRange;
        bool aimingAtThisDoor = IsAimingAtThisDoor();

        if (promptText)
            promptText.text = (closeEnough && aimingAtThisDoor && !door.IsOpen()) ? "Press [E] to interact" : "";

        if (closeEnough && aimingAtThisDoor && Input.GetKeyDown(interactKey))
            StartCoroutine(HandleAttempt());
    }

    System.Collections.IEnumerator HandleAttempt()
    {
        Quaternion startRot = door.transform.rotation;
        yield return new WaitForSeconds(0.15f);
        Quaternion endRot = door.transform.rotation;

        if (Quaternion.Angle(startRot, endRot) > 1f || door.IsOpen())
            yield break;

        // ✅ Trigger dialogue when locked
        if (dialogueManager)
            dialogueManager.ShowLine(lockedDoorDialogue);

        // Optional: keep visual/sound feedback
        ShowMessage("Door Locked");
        PlayLockedSound();
        if (!isShaking) StartCoroutine(ShakeDoor());
    }

    // ---- Helpers ----
    float DistanceToDoor(Vector3 fromPos)
    {
        if (doorCollider)
        {
            Vector3 cp = doorCollider.ClosestPoint(fromPos);
            return Vector3.Distance(fromPos, cp);
        }
        return Vector3.Distance(fromPos, door.transform.position);
    }

    bool IsAimingAtThisDoor()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange + 1f, doorMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider && (hit.collider.transform == door.transform || hit.collider.transform.IsChildOf(door.transform)))
                return true;

            var dcol = doorCollider ? doorCollider.transform : door.transform;
            if (hit.collider && (dcol == hit.collider.transform || dcol.IsChildOf(hit.collider.transform)))
                return true;
        }
        return false;
    }

    void ShowMessage(string msg)
    {
        if (!messageText) return;
        messageText.text = msg;
        messageText.alpha = 1f;
        messageText.gameObject.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(FadeMessage());
    }

    System.Collections.IEnumerator FadeMessage()
    {
        yield return new WaitForSeconds(messageDuration);
        float fade = 0.6f, t = 0f;
        Color c = messageText.color;
        while (t < fade)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fade);
            messageText.color = c;
            yield return null;
        }
        messageText.text = "";
        c.a = 1f;
        messageText.color = c;
        hideRoutine = null;
    }

    void PlayLockedSound()
    {
        if (soundCooldown || !lockedSound) return;
        if (audioSource) audioSource.PlayOneShot(lockedSound);
        soundCooldown = true;
        Invoke(nameof(ResetSoundCD), 0.35f);
    }
    void ResetSoundCD() => soundCooldown = false;

    System.Collections.IEnumerator ShakeDoor()
    {
        isShaking = true;
        Transform t = door.transform;
        Vector3 baseRot = t.localEulerAngles;
        float tElapsed = 0f;

        while (tElapsed < shakeDuration)
        {
            tElapsed += Time.deltaTime;
            float s = Mathf.Sin(tElapsed * 80f) * shakeIntensity;
            t.localEulerAngles = baseRot + new Vector3(0f, 0f, s);
            yield return null;
        }
        t.localEulerAngles = baseRot;
        isShaking = false;
    }
}
