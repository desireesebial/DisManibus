# Note and Letters System Setup Guide

## Overview
This system allows players to interact with note/letter objects in your Unity 3D game, displaying a customizable 2D paper UI with text content. The UI is created manually in the Canvas, and the NoteLetterUI script works as a controller. **Uses only ONE text GameObject for all content.**

## Components

### 1. NoteLetterSO (Scriptable Object)
- **Location**: `Assets/Scripts/IntventorySystem/Note/NoteLetterSO.cs`
- **Purpose**: Stores note data including title, content, paper texture, and styling
- **Usage**: Create via `Create > Game > Note Letter` in Project window

### 2. NoteLetterPickable (Script)
- **Location**: `Assets/Scripts/IntventorySystem/Note/NoteLetterPickable.cs`
- **Purpose**: Handles interaction with note objects in the 3D world
- **Features**: 
  - Player proximity detection
  - E key interaction
  - Audio feedback
  - Player movement control during reading

### 3. NoteLetterUI (Script - Note Controller)
- **Location**: `Assets/Scripts/IntventorySystem/Note/NoteLetterUI.cs`
- **Purpose**: Works as a controller to manage the note UI display
- **Features**:
  - Automatically finds UI elements by name in Canvas
  - **Single text element** handles all content (title, content, author, date)
  - Fade in/out animations
  - Dynamic text content updates
  - ESC key to close
  - Can be attached to any GameObject (not necessarily the Canvas)

## Setup Instructions

### Step 1: Create Note Data
1. Right-click in Project window
2. Select `Create > Game > Note Letter`
3. Configure your note:
   - **Note Title**: The heading of the note
   - **Note Content**: Main text (supports line breaks)
   - **Paper Texture**: 2D sprite for the paper background
   - **Text Color**: Color of the text
   - **Font**: TMP_FontAsset for TextMeshPro (optional)
   - **Audio**: Open/close sound effects (optional)

### Step 2: Setup UI Canvas (Manual Setup)
1. Create a new Canvas in your scene
2. Set Canvas Render Mode to "Screen Space - Overlay"
3. Add Canvas Scaler and Graphic Raycaster components
4. Create the UI hierarchy manually:

```
Canvas
└── NotePanel (GameObject - will be shown/hidden)
    ├── PaperImage (Image component - paper texture)
    ├── TextContent (TextMeshPro - ALL text content)
    └── CloseButton (Button - close the note)
```

**IMPORTANT**: The GameObject names must match exactly:
- `NotePanel`
- `PaperImage`
- `TextContent` (single text element for everything)
- `CloseButton`

### Step 3: Configure UI Elements
1. **NotePanel**: Set to cover most of the screen (e.g., anchors 0.1, 0.1 to 0.9, 0.9)
2. **PaperImage**: Set to fill the NotePanel completely
3. **TextContent**: Set to fill most of the NotePanel (e.g., anchors 0.1, 0.1 to 0.9, 0.9)
4. **CloseButton**: Position at top right, add button text

### Step 4: Add Note Controller
1. Create empty GameObject (e.g., "NoteController")
2. Add the `NoteLetterUI` script to it
3. The script will automatically find UI elements by name
4. Customize element names if needed in the inspector

### Step 5: Create Interactive Note Object
1. Create a 3D GameObject (e.g., a paper model, book, etc.)
2. Add the `NoteLetterPickable` script
3. Assign your `NoteLetterSO` to the **Note Data** field
4. Configure interaction settings:
   - **Interaction Range**: How close player needs to be
   - **Interaction Key**: Key to press (default: E)
   - **Interaction Text**: Text shown in UI prompt

### Step 6: Setup Interaction UI
1. Create interaction prompt UI (similar to existing systems)
2. Assign to **Interaction UI** field in `NoteLetterPickable`
3. Assign TextMeshPro component to **Interaction Text UI**

### Step 7: Add Audio (Optional)
1. Add `AudioSource` component to note object
2. Assign audio clips in `NoteLetterSO`
3. Reference the `AudioSource` in `NoteLetterPickable`

## UI Layout Example

### Recommended Layout Settings:
- **NotePanel**: Anchors (0.1, 0.1) to (0.9, 0.9)
- **PaperImage**: Anchors (0, 0) to (1, 1)
- **TextContent**: Anchors (0.1, 0.1) to (0.9, 0.9)
- **CloseButton**: Anchors (0.85, 0.85) to (0.95, 0.95)

## How Text Content Works

The **TextContent** element automatically combines all information:
- **Title**: Displayed in bold with larger size (24)
- **Main Content**: Normal text with line breaks
- **Author & Date**: Italic text at the bottom, separated by " | "

Example output:
```
**WARNING!**

The Dullahan is near. Be careful when exploring the mansion.

*By: Survivor | Last night*
```

## Example Note Creation

### Basic Note
```
Title: "Welcome Note"
Content: "Welcome to the mansion. Be careful..."
Author: "Unknown"
Date: "2024"
Paper Texture: [Paper sprite]
Text Color: Black
```

### Important Note
```
Title: "WARNING!"
Content: "The Dullahan is near..."
Author: "Survivor"
Date: "Last night"
Paper Texture: [Warning paper sprite]
Text Color: Red
Is Important: true
```

## Customization Options

### Paper Textures
- Use different sprites for different note types
- Create aged paper, modern paper, or themed textures
- Adjust Image component settings for proper scaling

### Text Styling
- Custom fonts for different time periods
- Color coding for importance levels
- Text size and spacing adjustments

### Audio Feedback
- Paper rustling sounds
- Typewriter sounds for modern notes
- Ambient sounds for atmosphere

## Integration with Existing Systems

### Player Movement
- Automatically disables player movement during reading
- Re-enables when note is closed
- Compatible with your existing `SimplePlayerMovement` script

### Interaction System
- Follows the same pattern as other interactive objects
- Uses E key interaction (configurable)
- Shows/hides interaction prompts

### UI Management
- Fade in/out animations
- Canvas-based UI system
- Responsive to ESC key

## Troubleshooting

### Note UI Not Showing
- Check if UI elements exist with exact names in Canvas
- Verify NoteLetterUI script is attached to a GameObject
- Use "Refresh UI References" context menu option
- Check Console for missing element warnings

### Interaction Not Working
- Verify player has "Player" tag
- Check interaction range in scene view (yellow wire sphere)
- Ensure `NoteLetterSO` is assigned to `NoteLetterPickable`

### Text Not Displaying
- Check TextMeshPro component is properly configured
- Verify text content in `NoteLetterSO`
- Ensure UI elements are active in hierarchy

### UI Element Names
- All UI elements must have exact names as specified
- Names are case-sensitive
- Use the inspector to customize element names if needed

## Performance Considerations

- Notes are loaded on-demand
- UI elements are found once at startup
- Minimal impact on frame rate
- Audio clips are loaded once per note

## Future Enhancements

- Multiple page support
- Handwriting recognition
- Note collection system
- Translation/localization support
- Note sharing between players (multiplayer)
