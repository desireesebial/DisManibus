using UnityEngine;

/// <summary>
/// Chest padlock that requires a specific KeySO to unlock.
/// When unlocked, it opens the associated ChestLid and optionally disables the lock visuals.
/// Integrate with your interaction system by calling TryUseKey(key) or InteractWithKey(key).
/// </summary>
public class ChestPadlock : MonoBehaviour
{
    [Header("Lock Data")]
    [Tooltip("The key ScriptableObject that can unlock this padlock.")]
    public KeySO requiredKey;

    [Header("Chest")]
    [Tooltip("Reference to the chest lid controller.")]
    public ChestLid chestLid;

    [Header("Visuals & Feedback")]
    [Tooltip("Optional: GameObject representing the lock model to hide/disable once unlocked.")]
    public GameObject lockVisual;
    [Tooltip("Play on unlock (optional)")]
    public AudioSource unlockAudio;
    [Tooltip("Play on failed attempt (optional)")]
    public AudioSource deniedAudio;

    [Header("State")]
    [SerializeField]
    private bool isLocked = true;

    /// <summary>
    /// Whether the padlock is currently locked.
    /// </summary>
    public bool IsLocked => isLocked;

    private void Awake()
    {
        if (chestLid == null)
        {
            chestLid = GetComponentInParent<ChestLid>();
        }
    }

    /// <summary>
    /// Attempt to use a key to unlock. Returns true if unlocked.
    /// </summary>
    public bool TryUseKey(KeySO key)
    {
        if (!isLocked)
        {
            // Already unlocked
            return true;
        }

        if (key == null)
        {
            PlayDenied();
            return false;
        }

        // Compare by reference or id
        bool matches = false;
        if (requiredKey != null)
        {
            matches = key == requiredKey || (!string.IsNullOrEmpty(requiredKey.keyId) && key.keyId == requiredKey.keyId);
        }

        if (!matches)
        {
            PlayDenied();
            return false;
        }

        Unlock();
        return true;
    }

    /// <summary>
    /// Call from your interaction system when the player uses the correct key on this lock.
    /// </summary>
    public void InteractWithKey(KeySO key)
    {
        if (TryUseKey(key))
        {
            if (chestLid != null)
            {
                chestLid.Open();
            }
        }
    }

    /// <summary>
    /// Force unlock (e.g., from a trigger, cheat, or quest).
    /// </summary>
    public void Unlock()
    {
        if (!isLocked)
            return;

        isLocked = false;

        if (lockVisual != null)
        {
            lockVisual.SetActive(false);
        }

        PlayUnlock();
    }

    /// <summary>
    /// Relock the chest (optional usage).
    /// </summary>
    public void Lock()
    {
        if (isLocked)
            return;
        isLocked = true;

        if (lockVisual != null)
        {
            lockVisual.SetActive(true);
        }
    }

    private void PlayUnlock()
    {
        if (unlockAudio != null)
        {
            unlockAudio.Play();
        }
    }

    private void PlayDenied()
    {
        if (deniedAudio != null)
        {
            deniedAudio.Play();
        }
    }
}


