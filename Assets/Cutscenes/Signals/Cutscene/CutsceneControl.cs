using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CutsceneControl : MonoBehaviour
{
    // Static property to track if any cutscene is currently playing
    public static bool IsPlayingCutscene { get; private set; }

    public PlayableDirector director;

    // Drag the components you want disabled here (movement/look/input).
    public Behaviour[] behavioursToDisable;

    // Optional: auto-find by type names so you don't have to drag anything
    [Tooltip("Optional type names, e.g. FirstPersonController, PlayerInput, MouseLook")]
    public string[] extraTypeNames;
    List<Behaviour> _auto = new();

    [Header("UI Elements to Hide During Cutscenes")]
    [Tooltip("Inventory panel GameObject")]
    public GameObject inventoryPanel;

    [Tooltip("Health UI GameObject")]
    public GameObject healthUI;

    [Tooltip("Quest UI GameObject (usually contains the quest text component)")]
    public GameObject questUI;

    [Tooltip("Item tracker/clues UI GameObject")]
    public GameObject itemTrackerUI;

    [Tooltip("Stamina UI GameObject (usually StaminaBG or parent of stamina bar)")]
    public GameObject staminaUI;

    // Store previous UI states
    private Dictionary<GameObject, bool> _previousUIStates = new Dictionary<GameObject, bool>();

    void Awake()
    {
        Debug.Log($"[CutsceneControl] Awake() called on {gameObject.name}");

        if (!director)
        {
            director = GetComponent<PlayableDirector>();
            Debug.Log(director != null
                ? $"[CutsceneControl] Found PlayableDirector on {gameObject.name}"
                : $"[CutsceneControl] ERROR: No PlayableDirector found on {gameObject.name}!");
        }

        // Auto-resolve extra type names (search in the whole scene or tag your player)
        if (extraTypeNames != null && extraTypeNames.Length > 0)
        {
            var allBehaviours = FindObjectsOfType<Behaviour>(true);
            foreach (var name in extraTypeNames)
            {
                var match = allBehaviours.FirstOrDefault(b => b && b.GetType().Name == name);
                if (match) _auto.Add(match);
            }
        }
    }

    void OnEnable()
    {
        Debug.Log($"[CutsceneControl] OnEnable() called on {gameObject.name}");

        if (director != null)
        {
            director.played += OnPlayed;
            director.stopped += OnStopped;
            Debug.Log($"[CutsceneControl] Event handlers subscribed for {gameObject.name}");

            // If director is already playing when we enable, manually trigger OnPlayed
            if (director.state == PlayState.Playing)
            {
                Debug.Log($"[CutsceneControl] Director already playing! Manually calling OnPlayed()");
                OnPlayed(director);
            }
        }
        else
        {
            Debug.LogError($"[CutsceneControl] Cannot subscribe to events - director is NULL on {gameObject.name}");
        }
    }

    void OnDisable()
    {
        Debug.Log($"[CutsceneControl] OnDisable() called on {gameObject.name}");

        if (director != null)
        {
            director.played -= OnPlayed;
            director.stopped -= OnStopped;
            Debug.Log($"[CutsceneControl] Event handlers unsubscribed for {gameObject.name}");
        }
    }

    void OnPlayed(PlayableDirector d)
    {
        Debug.Log("[CutsceneControl] Cutscene started - OnPlayed called");

        IsPlayingCutscene = true;

        foreach (var b in behavioursToDisable) if (b) b.enabled = false;
        foreach (var b in _auto) if (b) b.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide UI elements
        Debug.Log("[CutsceneControl] Calling HideUI()");
        HideUI();
    }

    void OnStopped(PlayableDirector d)
    {
        Debug.Log("[CutsceneControl] Cutscene stopped - OnStopped called");

        IsPlayingCutscene = false;

        // Clean up player Animator and Rigidbody state (Timeline animation can leave these in bad state)
        CleanupPlayerState();

        foreach (var b in behavioursToDisable) if (b) b.enabled = true;
        foreach (var b in _auto) if (b) b.enabled = true;

        // Restore UI elements with delay to ensure it happens after all cleanup
        Debug.Log("[CutsceneControl] Scheduling RestoreUI()");
        StartCoroutine(RestoreUIDelayed());
    }

    private IEnumerator RestoreUIDelayed()
    {
        // Wait for end of frame + a bit more to ensure all cutscene cleanup is done
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[CutsceneControl] Now calling RestoreUI()");
        RestoreUI();

        // FORCE restore - directly enable UI elements even if not in dictionary
        yield return new WaitForSeconds(0.2f);
        ForceRestoreUI();
    }

    private void ForceRestoreUI()
    {
        Debug.Log("[CutsceneControl] ForceRestoreUI() - FORCING UI to show");

        if (healthUI != null)
        {
            healthUI.SetActive(true);
            Debug.Log($"[CutsceneControl] FORCED healthUI ON: {healthUI.name}");
        }

        if (questUI != null)
        {
            questUI.SetActive(true);
            Debug.Log($"[CutsceneControl] FORCED questUI ON: {questUI.name}");
        }

        if (itemTrackerUI != null)
        {
            itemTrackerUI.SetActive(true);
            Debug.Log($"[CutsceneControl] FORCED itemTrackerUI ON: {itemTrackerUI.name}");
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            Debug.Log($"[CutsceneControl] FORCED inventoryPanel ON: {inventoryPanel.name}");
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(true);
            Debug.Log($"[CutsceneControl] FORCED staminaUI ON: {staminaUI.name}");
        }

        Debug.Log("[CutsceneControl] ForceRestoreUI() complete - ALL UI should be visible now!");
    }

    private void HideUI()
    {
        Debug.Log("[CutsceneControl] HideUI() started");
        _previousUIStates.Clear();

        // Store previous state and hide each UI element
        if (inventoryPanel != null)
        {
            _previousUIStates[inventoryPanel] = inventoryPanel.activeSelf;
            Debug.Log($"[CutsceneControl] Hiding inventoryPanel '{inventoryPanel.name}' (was {(inventoryPanel.activeSelf ? "active" : "inactive")})");
            inventoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] inventoryPanel is NULL - not assigned in Inspector!");
        }

        if (healthUI != null)
        {
            _previousUIStates[healthUI] = healthUI.activeSelf;
            Debug.Log($"[CutsceneControl] Hiding healthUI '{healthUI.name}' (was {(healthUI.activeSelf ? "active" : "inactive")})");
            healthUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] healthUI is NULL - not assigned in Inspector!");
        }

        if (questUI != null)
        {
            _previousUIStates[questUI] = questUI.activeSelf;
            Debug.Log($"[CutsceneControl] Hiding questUI '{questUI.name}' (was {(questUI.activeSelf ? "active" : "inactive")})");
            questUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] questUI is NULL - not assigned in Inspector!");
        }

        if (itemTrackerUI != null)
        {
            _previousUIStates[itemTrackerUI] = itemTrackerUI.activeSelf;
            Debug.Log($"[CutsceneControl] Hiding itemTrackerUI '{itemTrackerUI.name}' (was {(itemTrackerUI.activeSelf ? "active" : "inactive")})");
            itemTrackerUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] itemTrackerUI is NULL - not assigned in Inspector!");
        }

        if (staminaUI != null)
        {
            _previousUIStates[staminaUI] = staminaUI.activeSelf;
            Debug.Log($"[CutsceneControl] Hiding staminaUI '{staminaUI.name}' (was {(staminaUI.activeSelf ? "active" : "inactive")})");
            staminaUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] staminaUI is NULL - not assigned in Inspector!");
        }

        Debug.Log($"[CutsceneControl] HideUI() complete - {_previousUIStates.Count} UI elements hidden");
    }

    private void RestoreUI()
    {
        Debug.Log($"[CutsceneControl] RestoreUI() started - restoring {_previousUIStates.Count} UI elements");

        // Restore each UI element to its previous state
        foreach (var kvp in _previousUIStates)
        {
            if (kvp.Key != null)
            {
                bool shouldBeActive = kvp.Value;
                Debug.Log($"[CutsceneControl] Restoring '{kvp.Key.name}' to {(shouldBeActive ? "active" : "inactive")}");
                kvp.Key.SetActive(shouldBeActive);

                // Double-check it actually got set
                if (kvp.Key.activeSelf != shouldBeActive)
                {
                    Debug.LogWarning($"[CutsceneControl] Failed to restore '{kvp.Key.name}' - trying again!");
                    kvp.Key.SetActive(shouldBeActive);
                }
                else
                {
                    Debug.Log($"[CutsceneControl] Successfully restored '{kvp.Key.name}' - now {(kvp.Key.activeSelf ? "ACTIVE" : "inactive")}");
                }
            }
        }

        _previousUIStates.Clear();
        Debug.Log("[CutsceneControl] RestoreUI() complete");
    }

    private void CleanupPlayerState()
    {
        Debug.Log("[CutsceneControl] CleanupPlayerState() started");

        // Find player using FirstPersonController singleton
        FirstPersonController fpc = FirstPersonController.Instance;
        if (fpc == null)
        {
            Debug.LogWarning("[CutsceneControl] Cannot find FirstPersonController.Instance for cleanup - trying FindObjectOfType");
            fpc = FindObjectOfType<FirstPersonController>();
            if (fpc == null)
            {
                Debug.LogError("[CutsceneControl] Still cannot find FirstPersonController - aborting cleanup");
                return;
            }
        }

        GameObject playerObj = fpc.gameObject;
        Debug.Log($"[CutsceneControl] Found player: {playerObj.name}");

        // Check and disable Animator (Timeline may have created/used one)
        Animator animator = playerObj.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log($"[CutsceneControl] Found Animator on player - disabling it (enabled={animator.enabled})");
            animator.enabled = false;
        }

        // Reset Rigidbody state
        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"[CutsceneControl] Resetting Rigidbody - isKinematic={rb.isKinematic}, constraints={rb.constraints}, drag={rb.linearDamping}, mass={rb.mass}");

            // Ensure not kinematic
            rb.isKinematic = false;

            // Clear all constraints (Timeline animation might have set these)
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Zero out velocities
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Ensure drag is not corrupted (should be 0 for first person controller)
            if (rb.linearDamping > 0.1f)
            {
                Debug.LogWarning($"[CutsceneControl] Rigidbody drag was abnormal: {rb.linearDamping}, resetting to 0");
                rb.linearDamping = 0f;
            }

            // Force physics engine to update
            Physics.SyncTransforms();

            // Wake up the Rigidbody to ensure it's responsive
            rb.WakeUp();

            Debug.Log($"[CutsceneControl] Rigidbody reset complete - isKinematic={rb.isKinematic}, constraints={rb.constraints}, drag={rb.linearDamping}");
        }
        else
        {
            Debug.LogWarning("[CutsceneControl] No Rigidbody found on player!");
        }

        // Explicitly ensure movement is enabled
        Debug.Log($"[CutsceneControl] FirstPersonController state - playerCanMove: {fpc.playerCanMove}, cameraCanMove: {fpc.cameraCanMove}");
        fpc.playerCanMove = true;
        fpc.cameraCanMove = true;
        Debug.Log($"[CutsceneControl] Set playerCanMove=true, cameraCanMove=true");

        // Check and fix movement parameters that may have been corrupted
        Debug.Log($"[CutsceneControl] Movement parameters - walkSpeed: {fpc.walkSpeed}, maxVelocityChange: {fpc.maxVelocityChange}");

        if (fpc.maxVelocityChange < 1f)
        {
            Debug.LogWarning($"[CutsceneControl] maxVelocityChange was corrupted ({fpc.maxVelocityChange}), resetting to 10");
            fpc.maxVelocityChange = 10f;
        }

        if (fpc.walkSpeed < 1f)
        {
            Debug.LogWarning($"[CutsceneControl] walkSpeed was corrupted ({fpc.walkSpeed}), resetting to 5");
            fpc.walkSpeed = 5f;
        }

        Debug.Log($"[CutsceneControl] Final movement parameters - walkSpeed: {fpc.walkSpeed}, maxVelocityChange: {fpc.maxVelocityChange}");

        Debug.Log("[CutsceneControl] CleanupPlayerState() complete");
    }

    public void Play() => director?.Play();
}
