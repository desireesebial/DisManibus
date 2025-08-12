using UnityEngine;
using UnityEngine.UI;

public class WorkingCrosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Color crosshairColor = Color.white;
    public float crosshairSize = 20f;
    public float crosshairThickness = 2f;
    public float crosshairGap = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Canvas crosshairCanvas;
    private GameObject crosshairObject;
    private bool isInitialized = false;
    
    void Start()
    {
        Debug.Log("WorkingCrosshair: Starting initialization...");
        CreateCrosshair();
    }
    
    void CreateCrosshair()
    {
        try
        {
            // Step 1: Find or create Canvas
            crosshairCanvas = FindObjectOfType<Canvas>();
            if (crosshairCanvas == null)
            {
                Debug.Log("WorkingCrosshair: No Canvas found, creating new one...");
                GameObject canvasGO = new GameObject("CrosshairCanvas");
                crosshairCanvas = canvasGO.AddComponent<Canvas>();
                crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                crosshairCanvas.sortingOrder = 100;
                
                // Add required components
                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<GraphicRaycaster>();
                
                Debug.Log("WorkingCrosshair: Canvas created successfully");
            }
            else
            {
                Debug.Log("WorkingCrosshair: Using existing Canvas: " + crosshairCanvas.name);
            }
            
            // Step 2: Create crosshair object
            crosshairObject = new GameObject("Crosshair");
            crosshairObject.transform.SetParent(crosshairCanvas.transform, false);
            
            // Step 3: Add Image component
            Image crosshairImage = crosshairObject.AddComponent<Image>();
            crosshairImage.color = crosshairColor;
            
            // Step 4: Set up RectTransform
            RectTransform rectTransform = crosshairObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(crosshairSize + crosshairGap * 2, crosshairSize + crosshairGap * 2);
            
            // Step 5: Create the crosshair sprite
            crosshairImage.sprite = CreateCrosshairSprite();
            
            isInitialized = true;
            Debug.Log("WorkingCrosshair: Crosshair created successfully!");
            
            // Step 6: Verify it's visible
            if (crosshairObject.activeInHierarchy)
            {
                Debug.Log("WorkingCrosshair: Crosshair object is active");
            }
            else
            {
                Debug.LogWarning("WorkingCrosshair: Crosshair object is not active!");
            }
            
            if (crosshairCanvas.gameObject.activeInHierarchy)
            {
                Debug.Log("WorkingCrosshair: Canvas is active");
            }
            else
            {
                Debug.LogWarning("WorkingCrosshair: Canvas is not active!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("WorkingCrosshair: Error creating crosshair: " + e.Message);
        }
    }
    
    Sprite CreateCrosshairSprite()
    {
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];
        
        // Fill with transparent pixels first
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Create cross pattern
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                bool isCross = false;
                
                // Vertical line (top and bottom)
                if (x >= textureSize/2 - crosshairThickness/2 && x <= textureSize/2 + crosshairThickness/2)
                {
                    if (y < crosshairGap || y > textureSize - crosshairGap)
                    {
                        isCross = true;
                    }
                }
                
                // Horizontal line (left and right)
                if (y >= textureSize/2 - crosshairThickness/2 && y <= textureSize/2 + crosshairThickness/2)
                {
                    if (x < crosshairGap || x > textureSize - crosshairGap)
                    {
                        isCross = true;
                    }
                }
                
                if (isCross)
                {
                    pixels[y * textureSize + x] = crosshairColor;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
        Debug.Log("WorkingCrosshair: Crosshair sprite created with size: " + sprite.rect.size);
        return sprite;
    }
    
    void Update()
    {
        // Debug info
        if (showDebugInfo && isInitialized)
        {
            if (crosshairObject != null)
            {
                Debug.Log("WorkingCrosshair: Crosshair position: " + crosshairObject.transform.position + 
                         ", Active: " + crosshairObject.activeInHierarchy + 
                         ", Canvas Active: " + (crosshairCanvas != null ? crosshairCanvas.gameObject.activeInHierarchy.ToString() : "null"));
            }
        }
    }
    
    // Public methods
    public void SetColor(Color newColor)
    {
        crosshairColor = newColor;
        if (crosshairObject != null)
        {
            Image image = crosshairObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = newColor;
            }
        }
    }
    
    public void SetSize(float newSize)
    {
        crosshairSize = newSize;
        if (crosshairObject != null)
        {
            RectTransform rectTransform = crosshairObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(crosshairSize + crosshairGap * 2, crosshairSize + crosshairGap * 2);
            }
        }
    }
    
    public void ToggleCrosshair(bool show)
    {
        if (crosshairObject != null)
        {
            crosshairObject.SetActive(show);
            Debug.Log("WorkingCrosshair: Crosshair visibility set to: " + show);
        }
    }
    
    public void ForceRecreate()
    {
        Debug.Log("WorkingCrosshair: Force recreating crosshair...");
        if (crosshairObject != null)
        {
            DestroyImmediate(crosshairObject);
        }
        isInitialized = false;
        CreateCrosshair();
    }
}
