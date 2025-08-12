using UnityEngine;

public class CrosshairTest : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode toggleKey = KeyCode.C;
    public KeyCode colorKey = KeyCode.R;
    public KeyCode sizeKey = KeyCode.T;
    public KeyCode recreateKey = KeyCode.Y;
    
    private WorkingCrosshair crosshair;
    
    void Start()
    {
        // Find the crosshair component
        crosshair = GetComponent<WorkingCrosshair>();
        
        if (crosshair == null)
        {
            Debug.LogError("CrosshairTest: No WorkingCrosshair component found! Please attach WorkingCrosshair.cs to this GameObject.");
        }
        else
        {
            Debug.Log("CrosshairTest: Found WorkingCrosshair component successfully!");
        }
    }
    
    void Update()
    {
        if (crosshair == null) return;
        
        // Toggle crosshair visibility
        if (Input.GetKeyDown(toggleKey))
        {
            bool isVisible = crosshair.gameObject.activeInHierarchy;
            crosshair.ToggleCrosshair(!isVisible);
            Debug.Log("CrosshairTest: Toggled crosshair visibility to: " + !isVisible);
        }
        
        // Change color
        if (Input.GetKeyDown(colorKey))
        {
            Color[] colors = { Color.white, Color.red, Color.green, Color.blue, Color.yellow };
            Color randomColor = colors[Random.Range(0, colors.Length)];
            crosshair.SetColor(randomColor);
            Debug.Log("CrosshairTest: Changed crosshair color to: " + randomColor);
        }
        
        // Change size
        if (Input.GetKeyDown(sizeKey))
        {
            float[] sizes = { 10f, 20f, 30f, 40f, 50f };
            float randomSize = sizes[Random.Range(0, sizes.Length)];
            crosshair.SetSize(randomSize);
            Debug.Log("CrosshairTest: Changed crosshair size to: " + randomSize);
        }
        
        // Force recreate
        if (Input.GetKeyDown(recreateKey))
        {
            crosshair.ForceRecreate();
            Debug.Log("CrosshairTest: Force recreated crosshair");
        }
    }
    
    void OnGUI()
    {
        // Display test instructions on screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Crosshair Test Controls:", GUI.skin.box);
        GUILayout.Label("C - Toggle visibility");
        GUILayout.Label("R - Random color");
        GUILayout.Label("T - Random size");
        GUILayout.Label("Y - Recreate crosshair");
        GUILayout.EndArea();
    }
}
