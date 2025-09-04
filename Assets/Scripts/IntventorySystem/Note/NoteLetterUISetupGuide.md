# Note Letter UI Canvas Setup Guide

## Quick Setup Steps

### 1. Create Canvas
1. Right-click in Hierarchy → UI → Canvas
2. Set Render Mode to "Screen Space - Overlay"
3. Add Canvas Scaler component
4. Add Graphic Raycaster component

### 2. Create Note Panel
1. Right-click on Canvas → Create Empty
2. Rename to "NotePanel"
3. Add RectTransform component
4. Set anchors to (0.1, 0.1) to (0.9, 0.9)
5. Set offsets to 0, 0, 0, 0

### 3. Create Paper Background
1. Right-click on NotePanel → UI → Image
2. Rename to "PaperImage"
3. Set anchors to (0, 0) to (1, 1)
4. Set offsets to 0, 0, 0, 0
5. Assign your paper texture sprite
6. Set color to white

### 4. Create Text Content (Single Element)
1. Right-click on NotePanel → UI → Text - TextMeshPro
2. Rename to "TextContent"
3. Set anchors to (0.1, 0.1) to (0.9, 0.9)
4. Set offsets to 0, 0, 0, 0
5. Set font size to 18
6. Set alignment to Left
7. Enable Word Wrap
8. Set text to "Sample content..."
9. Set color to black

### 5. Create Close Button
1. Right-click on NotePanel → UI → Button
2. Rename to "CloseButton"
3. Set anchors to (0.85, 0.85) to (0.95, 0.95)
4. Set offsets to 0, 0, 0, 0
5. Set button color to light gray
6. Add button text:
   - Right-click on CloseButton → UI → Text - TextMeshPro
   - Rename to "ButtonText"
   - Set anchors to (0, 0) to (1, 1)
   - Set text to "X"
   - Set font size to 16
   - Set alignment to Center

### 6. Add Note Controller
1. Create empty GameObject → Rename to "NoteController"
2. Add NoteLetterUI script
3. The script will automatically find all UI elements by name

### 7. Test Setup
1. Play the scene
2. The NotePanel should be hidden initially
3. Use the NoteController to show/hide notes

## Final Hierarchy Structure
```
Canvas
├── NotePanel (initially inactive)
│   ├── PaperImage
│   ├── TextContent (single text element for all content)
│   └── CloseButton
│       └── ButtonText
└── [Other UI elements...]

NoteController (with NoteLetterUI script)
```

## Important Notes
- **Names must match exactly** (case-sensitive)
- **NotePanel starts inactive** (script controls visibility)
- **Only ONE text element** - TextContent handles title, content, author, and date
- **Button must have Button component**
- **Controller can be anywhere in scene**

## How Text Works
The **TextContent** element automatically combines:
- **Title** (bold, larger size)
- **Main content** (normal text)
- **Author and date** (italic, at bottom)

All formatting is handled automatically by the script using TextMeshPro rich text tags.

## Troubleshooting
- If elements aren't found, check Console for warnings
- Use "Refresh UI References" context menu on NoteController
- Verify all GameObject names match exactly
- Ensure Canvas is set to Screen Space - Overlay
