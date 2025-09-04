# Note System

This folder contains all the scripts and documentation for the Note and Letters system.

## Files

- **NoteLetterSO.cs** - Scriptable Object for storing note data
- **NoteLetterPickable.cs** - Interactive note object script
- **NoteLetterUI.cs** - Note UI controller script
- **README_NoteLetterSystem.md** - Complete system setup guide
- **NoteLetterUISetupGuide.md** - Step-by-step UI setup guide

## Quick Start

1. Create UI Canvas manually (follow NoteLetterUISetupGuide.md)
2. Add NoteLetterUI script to any GameObject
3. Create NoteLetterSO assets for your notes
4. Add NoteLetterPickable to 3D objects

## Features

- ✅ **Single text GameObject** - All content in one element
- ✅ No UI scripts - UI created manually in Canvas
- ✅ Note controller manages everything
- ✅ **FirstPersonController integration** - Works with your FPS controller
- ✅ **Crosshair system support** - Maintains crosshair during gameplay
- ✅ **Smart cursor management** - Unlocks cursor for notes, locks for FPS
- ✅ Customizable paper textures and text
- ✅ Audio support
- ✅ Player movement control during reading
- ✅ Fade animations
- ✅ ESC key to close

## UI Structure

**Only 4 GameObjects needed:**
- `NotePanel` - Main container
- `PaperImage` - Paper background texture
- `TextContent` - **Single text element for everything**
- `CloseButton` - Close button

The script automatically formats:
- Title (bold, large)
- Content (normal)
- Author & Date (italic, bottom)

## FirstPersonController Integration

The system automatically integrates with your FirstPersonController:
- **Disables movement** when reading notes
- **Unlocks cursor** for note interaction
- **Re-locks cursor** when closing notes
- **Maintains crosshair** settings
- **Preserves all FPS controller settings**
