# Simple Lighting Controller

A developer-friendly way to change the lighting in your Unity game to create different moods!

## 🎮 What it does:
- Changes the main light color and brightness
- Creates 4 different atmospheres: Normal, Eerie, Scary, and Creepy
- Smooth transitions between lighting states
- Developer-only controls (no UI for players)

## 🚀 Step-by-Step Setup Guide:

### Step 1: Create the Controller GameObject
1. In your Unity scene, right-click in the Hierarchy window
2. Select **Create Empty**
3. Rename it to "Lighting Controller"

### Step 2: Add the Script
1. Select the "Lighting Controller" GameObject
2. In the Inspector, click **Add Component**
3. Search for "SimpleLightingController"
4. Click to add the script

### Step 3: Configure the Settings
1. In the Inspector, you'll see the lighting settings:
   - **Main Light**: The script will automatically find your main light
   - **Atmosphere Presets**: Colors and intensity for each mood
   - **Developer Controls**: Keyboard shortcuts (1,2,3,4 by default)
   - **Transition Speed**: How fast lighting changes (2 is good)

### Step 4: Test the Setup
1. Click the **Play** button in Unity
2. Press **1, 2, 3, 4** keys to test different atmospheres:
   - **1**: Normal (white light)
   - **2**: Eerie (warm orange)
   - **3**: Scary (red light)
   - **4**: Creepy (purple light)

### Step 5: Customize (Optional)
1. **Change Colors**: Click the color pickers in the inspector
2. **Adjust Intensity**: Change the brightness values (0-1)
3. **Custom Keys**: Change keyboard shortcuts if needed
4. **Disable Controls**: Uncheck "Enable Keyboard Controls" for builds

## 🎨 The Atmospheres:

### Normal (Key 1)
- White light, full brightness
- Standard lighting

### Eerie (Key 2)
- Warm orange/yellow light
- Medium brightness
- Spooky but not too dark

### Scary (Key 3)
- Red light
- Low brightness
- Dark and menacing

### Creepy (Key 4)
- Purple light
- Very low brightness
- Dark and mysterious

## 🎛️ Developer Customization:

### In the Inspector:
- **Main Light**: Drag your main directional light here
- **Normal/Eerie/Scary/Creepy Colors**: Change the colors for each atmosphere
- **Normal/Eerie/Scary/Creepy Intensity**: Change the brightness for each atmosphere
- **Enable Keyboard Controls**: Turn on/off keyboard shortcuts
- **Custom Keys**: Change the keyboard shortcuts (1,2,3,4 by default)
- **Transition Speed**: How fast the lighting changes (higher = faster)

### Right-Click Menu:
Right-click on the component in the inspector to access:
- Set Normal Lighting
- Set Eerie Lighting
- Set Scary Lighting
- Set Creepy Lighting

### In Code:
```csharp
// Get the controller
SimpleLightingController controller = FindObjectOfType<SimpleLightingController>();

// Change atmospheres
controller.MakeItEerie();
controller.MakeItScary();
controller.MakeItCreepy();
controller.MakeItNormal();
```

## 🎯 How it works:

1. **Light Color**: Changes the color of your main light
2. **Light Intensity**: Changes how bright the light is
3. **Ambient Light**: Changes the overall scene lighting
4. **Smooth Transitions**: Gradually changes between settings

## 🔧 Developer Controls:

### Keyboard Shortcuts (Developer Only):
- **1**: Normal atmosphere
- **2**: Eerie atmosphere
- **3**: Scary atmosphere
- **4**: Creepy atmosphere

### Inspector Controls:
- **Enable Keyboard Controls**: Toggle keyboard shortcuts on/off
- **Custom Keys**: Change the keyboard shortcuts
- **Colors & Intensity**: Adjust each atmosphere's settings

### Context Menu:
Right-click the component in inspector for quick access to lighting presets

## 🔧 Troubleshooting:

### No lighting changes?
- Make sure you have a Light in your scene
- Check that the Light component is enabled
- Verify "Enable Keyboard Controls" is checked
- Try pressing the number keys (1-4)

### Colors not changing?
- The script automatically finds the first Light in your scene
- If you have multiple lights, drag the main one to the "Main Light" field
- Check that you're in Play Mode (lighting changes only work during runtime)

### Keyboard not working?
- Make sure "Enable Keyboard Controls" is enabled in the inspector
- Check that you're in Play Mode
- Try changing the key bindings in the inspector

## 💡 Developer Tips:

- **Test in Play Mode**: The lighting changes only work when the game is running
- **Disable for Build**: Uncheck "Enable Keyboard Controls" before building for players
- **Customize Colors**: Change the colors in the inspector to match your game's style
- **Use Context Menu**: Right-click the component for quick testing
- **Call from Scripts**: Use the public methods in your game logic

## 🎮 Example Usage:

```csharp
// In another script
public class GameManager : MonoBehaviour
{
    public SimpleLightingController lighting;
    
    void Start()
    {
        // Start with normal lighting
        lighting.MakeItNormal();
    }
    
    void OnPlayerEnteredScaryArea()
    {
        // Change to scary lighting
        lighting.MakeItScary();
    }
    
    void OnPlayerExitedScaryArea()
    {
        // Return to normal
        lighting.MakeItNormal();
    }
    
    void OnBossFightStarted()
    {
        // Change to creepy lighting for boss fight
        lighting.MakeItCreepy();
    }
}
```

## 🚫 Player Experience:

- **No UI**: Players won't see any controls
- **Developer Only**: Keyboard shortcuts are for development/testing only
- **Script Control**: Use the public methods to control lighting from your game logic
- **Build Ready**: Disable keyboard controls before building for players

This system is perfect for developers who want to add atmospheric lighting to their games with easy testing controls and no player-facing UI! 