# Health UI Quick Reference

## 🚀 **Quick Setup Checklist**

### **Player Health UI (5 minutes)**
- [ ] Create Canvas (UI > Canvas)
- [ ] Add 3 Health Bar Images (Image > Filled > Horizontal)
- [ ] Add Health Text (TextMeshPro > "Health: 3/3")
- [ ] Add Status Text (TextMeshPro > "Healthy")
- [ ] Create Death Screen (Panel + Text + 2 Buttons)
- [ ] Assign all references in PlayerHealthSystem

### **Enemy Health UI (Optional)**
- [ ] Create World Space Canvas for enemies
- [ ] Add Health Bar (Image > Filled > Horizontal)
- [ ] Add Health Text (TextMeshPro > "Health: 100/100")
- [ ] Position above enemy head
- [ ] Test enemy health changes

## 🎨 **Color Codes**

### **Player Health Colors**
```csharp
// Healthy (3/3)
Color: #1A8B1A (Green)

// Injured (2/3)  
Color: #FFA500 (Orange)

// Critical (1/3)
Color: #FF0000 (Red)
```

### **Enemy Health Colors**
```csharp
// Enemy Health Bar
Color: #FF0000 (Red)

// Enemy Background
Color: #4A0000 (Dark Red)
```

## 📐 **UI Hierarchy**

```
HealthUI (GameObject)
├── HealthBarsContainer
│   ├── HealthBar1 (Image - Filled)
│   ├── HealthBar2 (Image - Filled)
│   └── HealthBar3 (Image - Filled)
├── HealthText (TextMeshProUGUI)
├── StatusText (TextMeshProUGUI)
└── DeathMessageUI (GameObject)
    ├── DeathMessageText (TextMeshProUGUI)
    ├── RetryButton (Button)
    └── MainMenuButton (Button)
```

## 🔧 **Inspector Settings**

### **Health Bar Images**
- Image Type: **Filled**
- Fill Method: **Horizontal**
- Fill Amount: **1.0** (full health)
- Color: **Green** (#1A8B1A)

### **Canvas Settings (All Screen Sizes)**
- Canvas Scaler: **Scale With Screen Size**
- Reference Resolution: **1920x1080**
- Screen Match Mode: **Match Width Or Height**
- Match: **0.5** (balances width and height scaling)
- Reference Pixels Per Unit: **100**

### **Text Settings**
- Font Size: **24** (Health Text)
- Font Size: **18** (Status Text)
- Color: **White**

## 🎮 **Testing Commands**

### **Debug Keys (PlayerHealthSystem)**
- **H Key**: Take 1 damage
- **J Key**: Heal 1 health
- **K Key**: Restore full health

### **Context Menu Tests**
- **"Take 10 Damage"**: Test enemy damage
- **"Heal 10 Health"**: Test enemy healing
- **"Kill Enemy"**: Test enemy death

## 🚨 **Common Issues**

### **Health Bars Not Updating**
- Check Image Type = "Filled"
- Check Fill Method = "Horizontal"
- Verify healthBars array assignment

### **Text Not Showing**
- Check TextMeshProUGUI components
- Verify text color is visible
- Check text assignment in inspector

### **Death Screen Not Working**
- Check deathMessageUI is initially disabled
- Verify button onClick events
- Check Canvas Group settings

## 📱 **Responsive Design - All Screen Sizes**

### **Supported Screen Resolutions**
- **1920x1080** (Full HD) - Reference resolution
- **1366x768** (HD) - Scales down automatically
- **2560x1440** (2K) - Scales up automatically
- **3840x2160** (4K) - Scales up automatically
- **1280x720** (HD) - Scales down automatically
- **800x600** (4:3) - Scales down with letterboxing
- **2560x1080** (21:9) - Ultrawide support
- **3440x1440** (21:9) - Ultrawide 2K
- **1080x1920** (9:16) - Mobile portrait
- **1440x2560** (9:16) - Mobile 2K

### **UI Positioning (Responsive)**
- **Top-Left**: Health UI (anchored)
- **Top-Right**: Enemy Health (anchored)
- **Center**: Death Screen (centered)
- **Bottom**: Status messages (anchored)

### **Screen Size Testing**
1. **1920x1080** (Reference)
2. **1366x768** (Common laptop)
3. **2560x1440** (2K monitor)
4. **3840x2160** (4K monitor)
5. **1280x720** (Low-end)
6. **800x600** (Very low-end)
7. **2560x1080** (Ultrawide)
8. **1080x1920** (Mobile portrait)

## 🎯 **Performance Tips**

1. **Use Canvas Groups** for efficient UI updates
2. **Disable enemy health bars** when not in view
3. **Limit particle effects** to avoid lag
4. **Use Object Pooling** for multiple enemies

---

**Quick setup in 5 minutes, ready for your survival horror game!**
