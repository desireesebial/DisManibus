# Note and Letters System

A lightweight note/letter interaction system for Unity. Interact with world objects to open a stylized paper UI, read content, and close to resume gameplay.

## Quick Start
1. Create a Note asset: Project → Create → Game → Note Letter.
2. Build the UI once in your main Canvas:
```
Canvas
└── NotePanel
    ├── PaperImage (Image)
    ├── TextContent (TextMeshProUGUI)
    └── CloseButton (Button)
```
   - Names must match exactly. `NotePanel` starts inactive.
3. Add `NoteLetterUI` to any GameObject (e.g., `NoteController`). It auto-finds the UI by names and adds a `CanvasGroup` to `NotePanel`.
4. In the scene, add `NoteLetterPickable` to a 3D object. Assign the Note asset to "Note Data".
5. Enter Play, look at or approach the object, press E to open, press Close or ESC to exit.

## Key Features
- Single TMP text element for title, content, author, date with rich-text formatting.
- Optional pre-written paper mode (hides text, shows texture-only notes).
- Fade in/out animations with `CanvasGroup`.
- Disables player movement/camera while reading; restores on close.
- Interaction by proximity or crosshair raycast (configurable).
- Optional audio for open/close via `NoteLetterSO` and an `AudioSource`.

## Components
- `NoteLetterSO`: Title, Content, Paper Texture, Text Color, optional TMP Font, Pre-written Paper options, Author/Date, Open/Close Audio.
- `NoteLetterUI`: Finds `NotePanel`, `PaperImage`, `TextContent`, `CloseButton`. Handles text composition, textures, fades, ESC close.
- `NoteLetterPickable`: Shows the UI on interaction, disables/enables player controls, handles crosshair feedback and prompts.

## Configuration Tips
- TextContent: Enable Word Wrap; set alignment Left; adjust margins to avoid the paper edge.
- Fonts: Assign a TMP Font Asset in the SO to override the default font at runtime.
- Crosshair Detection: Set a LayerMask for notes; place note colliders on that layer.
- Interaction UI: Provide a small prompt GameObject and a TMP label for the interaction text.

## Validation Checklist
- Canvas is in scene, Screen Space - Overlay.
- Exact names exist under Canvas: `NotePanel`, `PaperImage`, `TextContent`, `CloseButton`.
- `NotePanel` initially inactive; becomes active when opening a note.
- `NoteLetterUI` present; no console errors about missing UI.
- `NoteLetterSO` assigned on each `NoteLetterPickable`.
- Player has tag `Player`; `FirstPersonController` present in scene.

## Troubleshooting
- Note UI not showing: Check GameObject names; use `NoteLetterUI` → "Refresh UI References".
- Interaction not working: Ensure within range or crosshair ray hits the note on `noteLayerMask`.
- Text not visible: Pre-written mode hides `TextContent` intentionally.
- Controls/cursor stuck: Use `NoteLetterPickable` → "Force Reset Player Controls" context command.
- Close button inert: Ensure a Button component exists; the script wires onClick at Start.

## Future Ideas
- Multi-page notes, collection/logbook, localization, accessibility options.
