# Bootstrap Scene Improvements Guide

## Instructions for Implementing Bootstrap.unity Improvements

### Priority 1: Fix UI Responsiveness (CRITICAL)

1. **Open Bootstrap.unity** in Unity Editor
2. **Select "Loading Screen" Canvas** in Hierarchy
3. In Inspector, find **Canvas Scaler** component:
   - Change `UI Scale Mode` from "Constant Pixel Size" to **"Scale With Screen Size"**
   - Set `Reference Resolution` to **1920 x 1080**
   - Set `Match` slider to **0.5** (balanced scaling)
4. **Save Scene** (Ctrl+S)

### Priority 2: Fix Loading Text Position (CRITICAL)

1. **Select "LoadingText"** GameObject in Hierarchy
2. In Inspector, **RectTransform** component:
   - Set `Anchor Preset` to **Center** (hold Alt+Shift, click center square)
   - Set `Pos X` to **0**
   - Set `Pos Y` to **-400** (bottom-center)
   - Or position as desired
3. **Save Scene**

### Priority 3: Add EventSystem (CRITICAL)

1. **Right-click in Hierarchy** → **UI** → **Event System**
2. An EventSystem will be created
3. **Save Scene**

### Priority 4: Improve Progress Bar Visuals

1. **Select "Slider"** GameObject
2. **Expand slider hierarchy** to find:
   - **Background** → Change Image color to darker shade
   - **Fill** → Change Image color to green/blue gradient
3. **Adjust slider size/position** if needed via RectTransform
4. **Save Scene**

### Priority 5: Optimize Camera and Lighting

**Option A: Keep Camera, Remove Unnecessary Light**
1. **Select "Directional Light"** in Hierarchy
2. **Delete** (not needed for UI-only loading screen)
3. **Save Scene**

**Option B: Optimize Camera**
1. **Select "Main Camera"**
2. In Transform, set Position to **(0, 0, -10)** (standard UI camera position)
3. **Save Scene**

**Option C: Disable Camera (if using Screen Space Overlay)**
1. Check if Canvas Render Mode is "Screen Space - Overlay"
2. If yes, you can disable Main Camera entirely during loading
3. **Save Scene**

### Priority 6: Improve Loading Text (Optional)

**Option A: Fix Static Text**
1. **Select "LoadingText"**
2. In TextMeshPro component, change text from "Loading....." to **"Loading..."**
3. **Save Scene**

**Option B: Add Animated Dots Script (Recommended)**
1. **Create new C# script**: `Assets/Scripts/UI/LoadingTextAnimator.cs`
2. Copy this code:

```csharp
using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingTextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private string baseText = "Loading";
    [SerializeField] private float dotInterval = 0.5f;
    [SerializeField] private int maxDots = 3;

    private int currentDots = 0;

    void Start()
    {
        if (loadingText == null)
            loadingText = GetComponent<TextMeshProUGUI>();

        if (loadingText != null)
            StartCoroutine(AnimateDots());
    }

    private IEnumerator AnimateDots()
    {
        while (true)
        {
            currentDots = (currentDots % maxDots) + 1;
            string dots = new string('.', currentDots);
            loadingText.text = baseText + dots;
            yield return new WaitForSecondsRealtime(dotInterval);
        }
    }
}
```

3. **Attach script to "LoadingText"** GameObject
4. **Assign references** in Inspector
5. **Save Scene**

### Priority 7: Add Loading Tips (Optional)

1. **Select "LoadingText"** or create new Text object
2. **Create new script**: `Assets/Scripts/UI/LoadingTips.cs`

```csharp
using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingTips : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tipsText;
    [SerializeField] private float tipChangeInterval = 3f;
    [SerializeField] private string[] tips = new string[]
    {
        "Tip: Use your flashlight wisely... battery is limited",
        "Tip: Listen carefully for enemy sounds",
        "Tip: Some rooms hold valuable clues",
        "Tip: Not all enemies can be avoided",
        "Did you know: The mansion has multiple floors to explore"
    };

    void Start()
    {
        if (tipsText != null)
            StartCoroutine(RotateTips());
    }

    private IEnumerator RotateTips()
    {
        int index = 0;
        while (true)
        {
            if (tips.Length > 0)
            {
                tipsText.text = tips[index];
                index = (index + 1) % tips.Length;
            }
            yield return new WaitForSecondsRealtime(tipChangeInterval);
        }
    }
}
```

3. **Create new TextMeshPro** object below loading bar for tips
4. **Attach LoadingTips script**
5. **Configure tips array** in Inspector
6. **Save Scene**

### Testing

1. **Enter Play Mode** to test responsiveness
2. **Resize Game window** to test different resolutions
3. **Verify animations** are working
4. **Check console** for errors

## Notes

- All changes must be made through Unity Editor
- Never edit .unity files directly in text editor
- Always save scene after changes (Ctrl+S)
- Test at different resolutions (16:9, 16:10, 4:3)
