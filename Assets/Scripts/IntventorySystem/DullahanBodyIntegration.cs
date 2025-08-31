using UnityEngine;

/// <summary>
/// Integration script to connect DullahanBody with Floor2EndingEventManager
/// This ensures that when the real head is attached, the good ending is triggered
/// </summary>
public class DullahanBodyIntegration : MonoBehaviour
{
    [Header("Integration")]
    public DullahanBody dullahanBody;
    public Floor2EndingEventManager floor2EventManager;
    
    void Start()
    {
        // Find references if not assigned
        if (dullahanBody == null)
            dullahanBody = GetComponent<DullahanBody>();
            
        if (floor2EventManager == null)
            floor2EventManager = FindObjectOfType<Floor2EndingEventManager>();
        
        // Subscribe to DullahanBody events
        if (dullahanBody != null)
        {
            // We'll need to modify DullahanBody to include this event
            // For now, we'll use a different approach
            SetupIntegration();
        }
    }
    
    private void SetupIntegration()
    {
        // Create a custom event system for head attachment
        StartCoroutine(MonitorHeadAttachment());
    }
    
    private System.Collections.IEnumerator MonitorHeadAttachment()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // Check every half second
            
            // Check if DullahanBody has completed the puzzle
            if (dullahanBody != null && dullahanBody.IsPuzzleCompleted())
            {
                // Notify Floor2EndingEventManager
                if (floor2EventManager != null)
                {
                    floor2EventManager.OnRealHeadAttached();
                    Debug.Log("DullahanBodyIntegration: Real head attached, notifying Floor2EndingEventManager");
                }
                
                // Stop monitoring
                break;
            }
        }
    }
    
    // Alternative method: Direct call from DullahanBody
    public void OnRealHeadAttached()
    {
        if (floor2EventManager != null)
        {
            floor2EventManager.OnRealHeadAttached();
            Debug.Log("DullahanBodyIntegration: Real head attached event received");
        }
    }
    
    // Public method to manually trigger the good ending (for testing)
    [ContextMenu("Trigger Good Ending")]
    public void TriggerGoodEnding()
    {
        if (floor2EventManager != null)
        {
            floor2EventManager.OnRealHeadAttached();
            Debug.Log("DullahanBodyIntegration: Manually triggered good ending");
        }
    }
}
