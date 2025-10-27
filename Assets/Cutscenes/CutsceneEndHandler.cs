using UnityEngine;

/// <summary>
/// Handles cutscene end via Timeline Signal
/// Restores UI elements that were hidden during cutscene
/// </summary>
public class CutsceneEndHandler : MonoBehaviour
{
    [Header("UI References to Restore")]
    public GameObject healthUI;
    public GameObject questUI;
    public GameObject itemTrackerUI;
    public GameObject inventoryPanel;
    public GameObject staminaUI;

    /// <summary>
    /// Called by Timeline Signal when cutscene ends
    /// </summary>
    public void OnCutsceneEnd()
    {
        Debug.Log("[CutsceneEndHandler] OnCutsceneEnd called - restoring UI");

        // Restore all UI elements
        if (healthUI != null)
        {
            healthUI.SetActive(true);
            Debug.Log($"[CutsceneEndHandler] Restored healthUI: {healthUI.name}");
        }

        if (questUI != null)
        {
            questUI.SetActive(true);
            Debug.Log($"[CutsceneEndHandler] Restored questUI: {questUI.name}");
        }

        if (itemTrackerUI != null)
        {
            itemTrackerUI.SetActive(true);
            Debug.Log($"[CutsceneEndHandler] Restored itemTrackerUI: {itemTrackerUI.name}");
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            Debug.Log($"[CutsceneEndHandler] Restored inventoryPanel: {inventoryPanel.name}");
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(true);
            Debug.Log($"[CutsceneEndHandler] Restored staminaUI: {staminaUI.name}");
        }

        Debug.Log("[CutsceneEndHandler] UI restoration complete!");
    }
}
