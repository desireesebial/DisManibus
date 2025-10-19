# Health UI Setup Guide

This guide explains how to set up the visual UI elements for the health system in your DisManibus survival horror game.

## 📋 Table of Contents

1. [Player Health UI](#player-health-ui)
2. [Enemy Health UI](#enemy-health-ui)
3. [Health Bar Components](#health-bar-components)
4. [UI Layout Examples](#ui-layout-examples)
5. [Color Schemes](#color-schemes)
6. [Animation Effects](#animation-effects)
7. [Troubleshooting](#troubleshooting)

## 🎮 Player Health UI

### **Core UI Elements**

#### **Health Bars (3-Bar System)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Health Bar 1 (Green)
│  [████████████████████████████████] │ ← Health Bar 2 (Green)  
│  [████████████████████████████████] │ ← Health Bar 3 (Green)
└─────────────────────────────────────┘
```

#### **Health Text Display**
```
Health: 3/3
Status: Healthy
```

#### **Critical Health Warning**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Health Bar 1 (Red)
│  [████████████████████████████████] │ ← Health Bar 2 (Red)
│  [████████████████████████████████] │ ← Health Bar 3 (Red)
│                                     │
│  ⚠️ CRITICALLY INJURED ⚠️            │
│  Status: Critically Injured        │
└─────────────────────────────────────┘
```

### **Player Health UI Setup**

#### **Step 1: Create Health UI Canvas**
1. Create new Canvas (UI > Canvas)
2. Set Canvas Scaler to "Scale With Screen Size"
3. Set Reference Resolution to 1920x1080
4. Set Screen Match Mode to "Match Width Or Height"
5. Set Match to 0.5 (balances width and height scaling)
6. Set Reference Pixels Per Unit to 100

#### **Step 2: Health Bar Container**
```
HealthUI (GameObject)
├── HealthBarsContainer (GameObject)
│   ├── HealthBar1 (Image)
│   ├── HealthBar2 (Image)
│   └── HealthBar3 (Image)
├── HealthText (TextMeshProUGUI)
├── StatusText (TextMeshProUGUI)
└── DeathMessageUI (GameObject)
    ├── DeathMessageText (TextMeshProUGUI)
    ├── RetryButton (Button)
    └── MainMenuButton (Button)
```

#### **Step 3: Configure Health Bars**
1. **HealthBar1, HealthBar2, HealthBar3**:
   - Image Type: Filled
   - Fill Method: Horizontal
   - Fill Amount: 1.0 (full health)
   - Color: Green (#1A8B1A)

2. **Health Text**:
   - Font: Arial or custom horror font
   - Font Size: 24
   - Color: White
   - Text: "Health: 3/3"

3. **Status Text**:
   - Font: Arial or custom horror font
   - Font Size: 18
   - Color: Green
   - Text: "Healthy"

#### **Step 4: Death Screen UI**
1. **DeathMessageUI** (initially disabled):
   - Background: Dark overlay
   - Alpha: 0.8

2. **DeathMessageText**:
   - Font Size: 36
   - Color: Red
   - Text: "You are dead"

3. **RetryButton**:
   - Text: "Retry"
   - Color: White background, black text

4. **MainMenuButton**:
   - Text: "Main Menu"
   - Color: White background, black text

### **Player Health UI Script Assignment**

```csharp
// In PlayerHealthSystem inspector:
Health UI:
├── healthUI = HealthUI GameObject
├── healthBars[0] = HealthBar1 Image
├── healthBars[1] = HealthBar2 Image  
├── healthBars[2] = HealthBar3 Image
├── healthText = HealthText TextMeshProUGUI
├── statusText = StatusText TextMeshProUGUI
├── deathMessageUI = DeathMessageUI GameObject
├── deathMessageText = DeathMessageText TextMeshProUGUI
├── retryButton = RetryButton Button
└── mainMenuButton = MainMenuButton Button
```

## 👹 Enemy Health UI

### **Enemy Health Bar (Optional)**
```
┌─────────────────────────────────────┐
│  Enemy Health: [████████████████]  │ ← Health Bar (Red)
│  Health: 75/100                    │ ← Health Text
└─────────────────────────────────────┘
```

### **Enemy Health UI Setup**

#### **Step 1: Enemy Health Bar**
1. Create Canvas for enemy health (World Space)
2. Set Canvas Render Mode to "World Space"
3. Position above enemy head
4. Scale to appropriate size

#### **Step 2: Enemy Health Components**
```
EnemyHealthUI (GameObject)
├── HealthBarBackground (Image)
├── HealthBarFill (Image)
├── HealthText (TextMeshProUGUI)
└── HealthBarContainer (GameObject)
```

#### **Step 3: Configure Enemy Health Bar**
1. **HealthBarBackground**:
   - Color: Dark Red (#4A0000)
   - Image Type: Filled
   - Fill Method: Horizontal

2. **HealthBarFill**:
   - Color: Red (#FF0000)
   - Image Type: Filled
   - Fill Method: Horizontal
   - Fill Amount: 1.0

3. **HealthText**:
   - Font Size: 16
   - Color: White
   - Text: "Health: 100/100"

## 🎨 Health Bar Components

### **Health Bar States**

#### **Healthy State (Green)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Full health
│  Color: #1A8B1A (Green)             │
│  Status: "Healthy"                 │
└─────────────────────────────────────┘
```

#### **Injured State (Orange)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← 2/3 health
│  [████████████████████████████████] │ ← 2/3 health
│  [                                    ] │ ← Empty
│  Color: #FFA500 (Orange)            │
│  Status: "Injured"                 │
└─────────────────────────────────────┘
```

#### **Critical State (Red)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← 1/3 health
│  [                                    ] │ ← Empty
│  [                                    ] │ ← Empty
│  Color: #FF0000 (Red)               │
│  Status: "Critically Injured"       │
└─────────────────────────────────────┘
```

### **Health Bar Animation**

#### **Damage Flash Effect**
```csharp
// When player takes damage:
1. Health bar flashes red
2. Alpha reduces to 0.3
3. Returns to normal after 0.2 seconds
4. Health bar updates to new value
```

#### **Critical Health Blur**
```csharp
// When player reaches 1 health:
1. Screen blur effect activates
2. Blur intensity: 1.0
3. Blur duration: 5 seconds
4. Repeats every 5 seconds while at critical health
```

## 🎨 Color Schemes

### **Horror Theme Colors**

#### **Player Health Colors**
```csharp
// Healthy State
healthyBarColor = new Color(0.1f, 0.8f, 0.1f);    // Green

// Injured State  
injuredBarColor = new Color(1f, 0.65f, 0.1f);     // Orange

// Critical State
criticalBarColor = new Color(0.9f, 0.1f, 0.1f);    // Red
```

#### **Enemy Health Colors**
```csharp
// Enemy Health Bar
enemyHealthColor = new Color(0.8f, 0.1f, 0.1f);   // Dark Red

// Enemy Health Background
enemyBackgroundColor = new Color(0.3f, 0.1f, 0.1f); // Very Dark Red
```

### **UI Layout Examples**

#### **Top-Left Health Display**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Health Bar 1
│  [████████████████████████████████] │ ← Health Bar 2
│  [████████████████████████████████] │ ← Health Bar 3
│  Health: 3/3                        │
│  Status: Healthy                    │
└─────────────────────────────────────┘
```

#### **Bottom-Left Health Display**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Health Bar 1
│  [████████████████████████████████] │ ← Health Bar 2
│  [████████████████████████████████] │ ← Health Bar 3
│  Health: 3/3                        │
│  Status: Healthy                    │
└─────────────────────────────────────┘
```

#### **Center Health Display**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Health Bar 1
│  [████████████████████████████████] │ ← Health Bar 2
│  [████████████████████████████████] │ ← Health Bar 3
│  Health: 3/3                        │
│  Status: Healthy                    │
└─────────────────────────────────────┘
```

## 🎬 Animation Effects

### **Health Bar Animations**

#### **Damage Flash Animation**
```csharp
// Sequence when player takes damage:
1. Health bar flashes red (0.2 seconds)
2. Alpha reduces to 0.3
3. Returns to normal alpha
4. Health bar updates to new value
5. Color changes based on health state
```

#### **Critical Health Blur**
```csharp
// Sequence when player reaches 1 health:
1. Screen blur effect activates
2. Blur intensity: 1.0
3. Blur duration: 5 seconds
4. Repeats every 5 seconds
5. Stops when health > 1
```

#### **Death Screen Animation**
```csharp
// Sequence when player dies:
1. Screen fades to black
2. Death message appears
3. Buttons fade in
4. Cursor becomes visible
5. Time scale stops (0.0)
```

### **Enemy Health Animations**

#### **Enemy Damage Flash**
```csharp
// Sequence when enemy takes damage:
1. Enemy health bar flashes red
2. Damage effect particle spawns
3. Health bar updates to new value
4. Audio plays damage sound
```

#### **Enemy Death Animation**
```csharp
// Sequence when enemy dies:
1. Death effect particle spawns
2. Health bar fades out
3. Audio plays death sound
4. Enemy disappears after 5 seconds
```

## 🔧 Troubleshooting

### **Common UI Issues**

#### **Health Bars Not Updating**
- Check if healthBars array is assigned in inspector
- Verify Image components have "Filled" type
- Ensure Fill Method is set to "Horizontal"

#### **Text Not Displaying**
- Check if healthText and statusText are assigned
- Verify TextMeshProUGUI components are present
- Ensure text color is visible

#### **Death Screen Not Showing**
- Check if deathMessageUI is assigned
- Verify deathMessageUI is initially disabled
- Ensure buttons have proper onClick events

#### **Critical Health Blur Not Working**
- Check if postProcessVolume is assigned
- Verify blur effect is properly configured
- Ensure blur intensity and duration are set

### **Performance Tips**

1. **Use Object Pooling** for health bars if you have many enemies
2. **Disable enemy health bars** when not in view
3. **Use Canvas Groups** for efficient UI updates
4. **Limit particle effects** to avoid performance issues

## 📱 **Responsive Design - All Screen Sizes**

### **Canvas Scaler Settings**
```csharp
// Universal Canvas Scaler Configuration
Canvas Scaler: Scale With Screen Size
Reference Resolution: 1920x1080
Screen Match Mode: Match Width Or Height
Match: 0.5 (balances width and height scaling)
Reference Pixels Per Unit: 100
```

### **Screen Resolution Support**

#### **Common Screen Sizes**
- **1920x1080** (Full HD) - Reference resolution
- **1366x768** (HD) - Scales down automatically
- **2560x1440** (2K) - Scales up automatically
- **3840x2160** (4K) - Scales up automatically
- **1280x720** (HD) - Scales down automatically
- **1024x768** (4:3) - Scales down with letterboxing
- **800x600** (4:3) - Scales down with letterboxing

#### **Mobile Screen Sizes**
- **1080x1920** (Portrait) - Rotated automatically
- **1920x1080** (Landscape) - Standard landscape
- **1440x2560** (2K Portrait) - High DPI mobile
- **2160x3840** (4K Portrait) - Ultra high DPI

#### **Ultrawide Screen Sizes**
- **2560x1080** (21:9) - Ultrawide monitors
- **3440x1440** (21:9) - Ultrawide 2K
- **5120x1440** (32:9) - Super ultrawide

### **UI Positioning System**

#### **Anchored Positioning**
```csharp
// Health UI (Top-Left)
Anchor: Top-Left
Position: (50, -50, 0)
Size: (300, 150)

// Enemy Health (Top-Right)
Anchor: Top-Right
Position: (-50, -50, 0)
Size: (200, 50)

// Death Screen (Center)
Anchor: Center
Position: (0, 0, 0)
Size: (800, 600)
```

#### **Responsive Anchoring**
- **Top-Left**: Health UI always in top-left corner
- **Top-Right**: Enemy Health always in top-right corner
- **Center**: Death Screen always centered
- **Bottom**: Status messages at bottom

### **Screen Size Testing**

#### **Test Resolutions**
1. **1920x1080** (Reference)
2. **1366x768** (Common laptop)
3. **2560x1440** (2K monitor)
4. **3840x2160** (4K monitor)
5. **1280x720** (Low-end)
6. **800x600** (Very low-end)

#### **Aspect Ratio Testing**
- **16:9** (Standard widescreen)
- **16:10** (MacBook Pro)
- **4:3** (Old monitors)
- **21:9** (Ultrawide)
- **32:9** (Super ultrawide)

### **Responsive UI Components**

#### **Health Bar Scaling**
```csharp
// Health Bar Sizing
Base Size: 300x50 pixels
Scale Factor: 1.0 (1920x1080)
Scale Factor: 0.8 (1366x768)
Scale Factor: 1.2 (2560x1440)
Scale Factor: 1.5 (3840x2160)
```

#### **Text Scaling**
```csharp
// Text Sizing
Base Font Size: 24
Scale Factor: 1.0 (1920x1080)
Scale Factor: 0.8 (1366x768)
Scale Factor: 1.2 (2560x1440)
Scale Factor: 1.5 (3840x2160)
```

#### **Button Scaling**
```csharp
// Button Sizing
Base Size: 120x40 pixels
Scale Factor: 1.0 (1920x1080)
Scale Factor: 0.8 (1366x768)
Scale Factor: 1.2 (2560x1440)
Scale Factor: 1.5 (3840x2160)
```

### **Safe Area Support**

#### **Mobile Safe Areas**
```csharp
// iPhone X/11/12/13/14 Safe Area
Top Safe Area: 44 pixels
Bottom Safe Area: 34 pixels
Left Safe Area: 0 pixels
Right Safe Area: 0 pixels

// Android Safe Area
Top Safe Area: 24 pixels
Bottom Safe Area: 0 pixels
Left Safe Area: 0 pixels
Right Safe Area: 0 pixels
```

#### **Safe Area Anchoring**
- **Health UI**: Top-Left with safe area offset
- **Enemy Health**: Top-Right with safe area offset
- **Death Screen**: Center with safe area consideration
- **Buttons**: Bottom with safe area offset

### **Cross-Platform Testing**

#### **Desktop Platforms**
- **Windows**: 1920x1080, 2560x1440, 3840x2160
- **Mac**: 1920x1080, 2560x1440, 2880x1800
- **Linux**: 1920x1080, 2560x1440, 3840x2160

#### **Mobile Platforms**
- **iOS**: 1080x1920, 1440x2560, 2160x3840
- **Android**: 1080x1920, 1440x2560, 2160x3840

#### **Console Platforms**
- **PlayStation**: 1920x1080, 2560x1440, 3840x2160
- **Xbox**: 1920x1080, 2560x1440, 3840x2160
- **Nintendo Switch**: 1280x720, 1920x1080

### **Screen Size Compatibility Matrix**

| Screen Size | Aspect Ratio | Scaling | Performance | Compatibility |
|-------------|--------------|---------|-------------|---------------|
| 1920x1080   | 16:9         | 1.0x    | Excellent   | ✅ Perfect    |
| 1366x768    | 16:9         | 0.8x    | Excellent   | ✅ Perfect    |
| 2560x1440   | 16:9         | 1.2x    | Excellent   | ✅ Perfect    |
| 3840x2160   | 16:9         | 1.5x    | Good        | ✅ Perfect    |
| 1280x720    | 16:9         | 0.7x    | Excellent   | ✅ Perfect    |
| 800x600     | 4:3          | 0.5x    | Good        | ✅ Good       |
| 2560x1080   | 21:9         | 1.0x    | Good        | ✅ Perfect    |
| 3440x1440   | 21:9         | 1.2x    | Good        | ✅ Perfect    |
| 1080x1920   | 9:16         | 1.0x    | Excellent   | ✅ Perfect    |
| 1440x2560   | 9:16         | 1.2x    | Good        | ✅ Perfect    |

## 🎮 **Quick Setup Checklist**

### **Player Health UI**
- [ ] Create Canvas with proper scaling
- [ ] Add 3 health bar images (filled type)
- [ ] Add health text and status text
- [ ] Create death screen UI
- [ ] Assign all references in PlayerHealthSystem
- [ ] Test health changes and death screen

### **Enemy Health UI (Optional)**
- [ ] Create world space canvas for enemies
- [ ] Add health bar and text components
- [ ] Position above enemy head
- [ ] Test enemy health changes
- [ ] Add audio and visual effects

### **Testing**
- [ ] Test player damage and healing
- [ ] Test critical health blur
- [ ] Test death screen functionality
- [ ] Test enemy health display
- [ ] Test UI scaling on different resolutions

## 🎨 **Visual Health State Examples**

### **Complete Health State Progression**

#### **Healthy State (3/3 Health)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Green (#1A8B1A)
│  [████████████████████████████████] │ ← Green (#1A8B1A)
│  [████████████████████████████████] │ ← Green (#1A8B1A)
│  Health: 3/3                        │
│  Status: Healthy                    │
└─────────────────────────────────────┘
```

#### **Injured State (2/3 Health)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Orange (#FFA500)
│  [████████████████████████████████] │ ← Orange (#FFA500)
│  [                                    ] │ ← Empty (Transparent)
│  Health: 2/3                        │
│  Status: Injured                    │
└─────────────────────────────────────┘
```

#### **Critical State (1/3 Health)**
```
┌─────────────────────────────────────┐
│  [████████████████████████████████] │ ← Red (#FF0000)
│  [                                    ] │ ← Empty (Transparent)
│  [                                    ] │ ← Empty (Transparent)
│  Health: 1/3                        │
│  Status: Critically Injured          │
└─────────────────────────────────────┘
```

#### **Dead State (0/3 Health)**
```
┌─────────────────────────────────────┐
│  [                                    ] │ ← Empty (Transparent)
│  [                                    ] │ ← Empty (Transparent)
│  [                                    ] │ ← Empty (Transparent)
│  Health: 0/3                        │
│  Status: Dead                       │
└─────────────────────────────────────┘
```

### **Screen Layout Overview**
```
┌─────────────────────────────────────────────────────────────────┐
│  GAME SCREEN (1920x1080)                                        │
│                                                                 │
│  ┌─────────────────┐                    ┌─────────────────┐      │
│  │  HEALTH UI      │                    │  ENEMY HEALTH   │      │
│  │  [████████████] │                    │  [████████████] │      │
│  │  [████████████] │                    │  Health: 75/100 │      │
│  │  [████████████] │                    └─────────────────┘      │
│  │  Health: 3/3    │                                            │
│  │  Status: Healthy│                                            │
│  └─────────────────┘                                            │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  DEATH SCREEN (Hidden by default)                          │ │
│  │  ┌─────────────────────────────────────────────────────────┐ │ │
│  │  │  ⚠️ YOU ARE DEAD ⚠️                                    │ │ │
│  │  │  [RETRY] [MAIN MENU]                                   │ │ │
│  │  └─────────────────────────────────────────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

**Your health UI system is now ready for your survival horror game!**
