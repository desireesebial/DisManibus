# Note Letter UI Canvas Setup Guide

## What you get
- A simple Canvas-based note reader controlled by `NoteLetterUI`.
- One TextMeshProUGUI element (`TextContent`) renders title, content, author, and date.
- Optional pre-written paper mode hides text and shows a custom textured note.

## Prerequisites
- TextMeshPro imported (Unity will prompt to import TMP Essentials on first use).
- A `Canvas` in the scene (Screen Space - Overlay recommended).

## Step 1 — Create the UI hierarchy (names must match exactly)
Create these under your Canvas:
```
Canvas
└── NotePanel           (GameObject)
    ├── PaperImage      (Image)
    ├── TextContent     (TextMeshProUGUI)
    └── CloseButton     (Button)
```

## Step 2 — Configure layout quickly
- `NotePanel`: anchors (0.1, 0.1) → (0.9, 0.9), initially inactive.
- `PaperImage`: anchors (0, 0) → (1, 1), offsets 0, fill the panel.
- `TextContent`: anchors (0.1, 0.1) → (0.9, 0.9), word wrap on, alignment Left, color Black.
- `CloseButton`: top-right of `NotePanel` (e.g., anchors 0.85–0.95 both axes). Add child TMP text with "X" if desired.

## Step 3 — Add the controller
- Create an empty GameObject anywhere (e.g., `NoteController`).
- Add `NoteLetterUI` to it.
- The script will find the UI by names above. Keep names or override in the inspector.
- It adds a `CanvasGroup` to `NotePanel` automatically for fade.

## Step 4 — Create a Note asset
- Project window → Create → Game → Note Letter.
- Fill fields in `NoteLetterSO`:
  - Title, Content, Author, Date
  - Paper Texture (Sprite)
  - Text Color, optional TMP Font Asset
  - Optional: Use Pre-written Paper + Pre-written Paper Texture (hides TextContent)
  - Optional: Open/Close Audio

## Step 5 — Make a note interactable (3D object)
- Add `NoteLetterPickable` to your scene object (e.g., paper mesh).
- Assign the `NoteLetterSO` to Note Data.
- Configure interaction:
  - Interaction Range or enable Crosshair Detection (range + LayerMask)
  - Interaction Key (default E)
  - Interaction UI object and TMP label (optional prompt)
- Optional: set `AudioSource` and it will play open/close clips from the SO.

## Quick test
1. Enter Play Mode.
2. Look at or move near the object (based on your detection mode).
3. Press E. The note fades in and disables player movement/camera.
4. Press the Close button or ESC. The note fades out and restores controls.

## Validation checklist
- Canvas exists and is Screen Space - Overlay.
- Names match exactly: `NotePanel`, `PaperImage`, `TextContent`, `CloseButton`.
- `NotePanel` starts inactive; `NoteLetterUI` toggles it.
- `TextContent` uses TMP. In pre-written mode it is auto-hidden.
- `NoteLetterUI` is in the scene and finds elements (no console errors).
- `NoteLetterPickable` has a Note asset assigned and finds the player tagged `Player`.

## Troubleshooting
- UI not found: Check exact names and hierarchy. Use the component menu on `NoteLetterUI` → "Refresh UI References".
- Nothing happens on E: Ensure player is within range and the object is on the `noteLayerMask` if using crosshair detection.
- Text missing: If "Use Pre-written Paper" is enabled, `TextContent` is hidden by design.
- No cursor/controls stuck: Use the context menu on `NoteLetterPickable` → "Force Reset Player Controls" in the editor, and confirm `FirstPersonController` is present.
- Close button not working: Ensure it has a Button component; the script wires up `onClick` at runtime.

## Notes
- ESC also closes the note while visible.
- Fonts: assign a TMP Font Asset in the SO to override the default.
- The system supports one open note at a time; opening a new note closes any other.
