# Death UI Image Setup Guide

This guide explains how to set up the death screen UI with background and text in image form for your DisManibus survival horror game.

## 🎨 **Death UI Components**

### **Required Components**
- **Death Background Image**: Full-screen background overlay
- **Death Message Image**: "You are dead" message as an image
- **Retry Button**: Button to restart the level
- **Main Menu Button**: Button to return to main menu

### **Optional Components**
- **Death Message Text**: Alternative text-based death message
- **Death Effects**: Particle effects, animations, etc.

## 🏗️ **UI Hierarchy Setup**

### **Death Screen Structure**
```
DeathScreenUI (GameObject)
├── DeathBackground (Image) - Full screen background
├── DeathMessageImage (Image) - "You are dead" image
├── DeathMessageText (TextMeshProUGUI) - Alternative text
├── RetryButton (Button)
└── MainMenuButton (Button)
```

## 🎯 **Step-by-Step Setup**

### **Step 1: Create Death Screen Canvas**
1. Create new Canvas (UI > Canvas)
2. Set Canvas Scaler to "Scale With Screen Size"
3. Set Reference Resolution to 1920x1080
4. Set Screen Match Mode to "Match Width Or Height"
5. Set Match to 0.5

### **Step 2: Create Death Background**
1. Create RawImage (UI > RawImage) as child of Canvas
2. Name it "DeathBackground"
3. Set Anchor to "Stretch" (full screen)
4. Set all margins to 0
5. Assign your death background texture to the Texture field
6. Set Color to semi-transparent (e.g., RGBA: 0, 0, 0, 0.8)
7. Set UV Rect to (0, 0, 1, 1) for full texture display

### **Step 3: Create Death Message Image**
1. Create Image (UI > Image) as child of Canvas
2. Name it "DeathMessageImage"
3. Set Anchor to "Center"
4. Set Position to (0, 0, 0)
5. Set Size to appropriate dimensions (e.g., 400x100)
6. Set Image Type to "Simple"
7. Assign your "You are dead" texture/image

### **Step 4: Create Buttons**
1. Create Button (UI > Button) as child of Canvas
2. Name it "RetryButton"
3. Set Anchor to "Bottom-Left"
4. Set Position to (100, 100, 0)
5. Set Size to (120, 40)
6. Set Button Text to "Retry"

1. Create Button (UI > Button) as child of Canvas
2. Name it "MainMenuButton"
3. Set Anchor to "Bottom-Right"
4. Set Position to (-100, 100, 0)
5. Set Size to (120, 40)
6. Set Button Text to "Main Menu"

### **Step 5: Configure PlayerHealthSystem**
1. Select your Player GameObject
2. Find PlayerHealthSystem component
3. In the "Death Screen UI" section:
   - **Death Message UI**: Assign the main DeathScreenUI GameObject
   - **Death Background**: Assign the DeathBackground Image
   - **Death Message Image**: Assign the DeathMessageImage Image
   - **Death Message Text**: Assign the DeathMessageText (optional)
   - **Retry Button**: Assign the RetryButton
   - **Main Menu Button**: Assign the MainMenuButton

## 🎨 **Image Requirements**

### **Death Background RawImage**
- **Size**: 1920x1080 (or any resolution)
- **Format**: PNG, JPG, or any texture format
- **Content**: Dark overlay, blood splatter, horror theme
- **Transparency**: Semi-transparent (alpha 0.8)
- **Texture Import**: Set as "UI" texture type for best performance

### **RawImage vs Image Benefits**
- **RawImage**: Direct texture display, no UI scaling, better performance
- **Image**: UI-scaled display, sprite support, better for UI elements
- **For Backgrounds**: RawImage is recommended for full-screen textures
- **For UI Elements**: Image is recommended for buttons, icons, etc.

### **Death Message Image**
- **Size**: 400x100 (or appropriate for your design)
- **Format**: PNG with transparency
- **Content**: "You are dead" text in horror font
- **Style**: Blood red, scary font, horror theme

### **Button Images**
- **Size**: 120x40 (or appropriate for your design)
- **Format**: PNG with transparency
- **Content**: Button background with text
- **Style**: Dark theme, horror aesthetic

## 🔧 **Inspector Configuration**

### **PlayerHealthSystem Settings**
```csharp
[Header("Death Screen UI")]
public GameObject deathMessageUI;           // Main death screen container
public RawImage deathBackground;            // Background overlay as RawImage
public Image deathMessageImage;             // Death message as image
public TextMeshProUGUI deathMessageText;   // Alternative text message
public Button retryButton;                  // Retry level button
public Button mainMenuButton;               // Main menu button
public string mainMenuSceneName = "MainMenu";
public bool pauseOnDeath = true;
```

### **RawImage Component Settings**
```csharp
// Death Background RawImage
Texture: Your death background texture
Color: RGBA(0, 0, 0, 0.8)  // Semi-transparent black
Anchor: Stretch
Margins: 0, 0, 0, 0
UV Rect: (0, 0, 1, 1)      // Full texture display

// Death Message Image
Image Type: Simple
Color: RGBA(1, 1, 1, 1)    // Full opacity
Anchor: Center
Position: (0, 0, 0)
Size: (400, 100)
```

## 🎮 **Death Screen Behavior**

### **When Player Dies**
1. **Time Scale**: Pauses to 0 (if pauseOnDeath is true)
2. **Cursor**: Unlocks and becomes visible
3. **Background**: Death background image appears
4. **Message**: Death message image appears
5. **Buttons**: Retry and Main Menu buttons appear

### **When Player Restarts**
1. **Time Scale**: Resumes to 1
2. **Cursor**: Locks and becomes invisible
3. **UI**: All death screen elements hidden
4. **Player**: Respawns with full health

## 🎨 **Visual Design Tips**

### **Horror Theme Colors**
```csharp
// Death Background
Color: RGBA(0.2, 0.0, 0.0, 0.8)  // Dark red overlay

// Death Message
Color: RGBA(0.8, 0.0, 0.0, 1.0)  // Blood red text

// Buttons
Color: RGBA(0.3, 0.3, 0.3, 1.0)  // Dark gray background
```

### **Font Recommendations**
- **Horror Fonts**: Blood, gothic, scary fonts
- **Size**: Large enough to be readable
- **Color**: Blood red, white, or dark colors
- **Style**: Bold, dramatic, horror-themed

## 🔧 **Advanced Features**

### **Death Screen Animation**
```csharp
// Add animation to death screen
public void ShowDeathScreen()
{
    // Fade in background
    if (deathBackground != null)
    {
        deathBackground.gameObject.SetActive(true);
        // Add fade-in animation here
    }
    
    // Slide in message
    if (deathMessageImage != null)
    {
        deathMessageImage.gameObject.SetActive(true);
        // Add slide-in animation here
    }
}
```

### **Multiple Death Messages**
```csharp
// Different death messages based on cause
public Image[] deathMessageImages;  // Array of different death images

private void ShowDeathScreen()
{
    // Show random death message
    if (deathMessageImages != null && deathMessageImages.Length > 0)
    {
        int randomIndex = Random.Range(0, deathMessageImages.Length);
        deathMessageImages[randomIndex].gameObject.SetActive(true);
    }
}
```

## 🧪 **Testing Death Screen**

### **Debug Methods**
```csharp
// Test death screen in editor
[ContextMenu("Test Death Screen")]
private void TestDeathScreen()
{
    ShowDeathScreen();
}

// Test death screen with different messages
[ContextMenu("Test Death with Message")]
private void TestDeathWithMessage()
{
    currentHealth = 0;
    HandlePlayerDeath();
}
```

### **Testing Checklist**
- [ ] Death screen appears when player dies
- [ ] Background image covers full screen
- [ ] Death message image is visible
- [ ] Buttons are clickable
- [ ] Retry button restarts level
- [ ] Main Menu button returns to main menu
- [ ] Death screen hides when player restarts
- [ ] Cursor behavior is correct

## 🎯 **Quick Setup Checklist**

### **UI Setup**
- [ ] Create Canvas with proper scaling
- [ ] Create DeathBackground Image (full screen)
- [ ] Create DeathMessageImage (centered)
- [ ] Create RetryButton (bottom-left)
- [ ] Create MainMenuButton (bottom-right)
- [ ] Assign all references in PlayerHealthSystem

### **Image Assets**
- [ ] Create death background texture
- [ ] Create death message texture
- [ ] Create button textures
- [ ] Import textures to Unity
- [ ] Assign textures to Image components

### **Testing**
- [ ] Test death screen appearance
- [ ] Test button functionality
- [ ] Test screen scaling
- [ ] Test cursor behavior
- [ ] Test time scale pausing

---

**Your death screen UI is now ready with image-based background and message!**
