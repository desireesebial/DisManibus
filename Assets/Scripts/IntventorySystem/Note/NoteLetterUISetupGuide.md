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

## Note Swiping (Optional) — Pile of Papers Setup
Use this to make the player swipe away irrelevant papers with A/D or arrow keys until the last, relevant note is revealed.

### 1) Create the pile UI (under the same Canvas)
```
Canvas
└── PileRoot            (empty GameObject)
    ├── Page_01         (RectTransform, Image or panel)
    ├── Page_02         (RectTransform)
    └── Page_03 ...     (RectTransform)
```
- Order matters: bottom page first, top page last (last is discarded first).
- Each page should be a `RectTransform` (e.g., an `Image`) sized like a paper.

### 2) Add the controller
- Create an empty (e.g., `NotePileController`) and add `NotePileSwiper`.
- Drag your page `RectTransform`s to `pilePages` in the correct order (bottom → top).
- Assign `finalNote` (a `NoteLetterSO`) that should display when the pile is cleared.
- Optionally assign the existing `NoteLetterUI` reference. If left empty, it will auto-find one.

### 3) Activate at runtime
- Enable swiping when needed by toggling `isActive = true` via script or inspector.
- Player can press A/D or Left/Right to discard the top page with a short swipe animation.
- When the pile becomes empty, `finalNote` is shown via `NoteLetterUI`.

### 4) Tuning
- `swipeDistance` and `swipeDuration` control how far/fast pages move off-screen.
- `swipeCurve` adjusts easing.
- `nextPagePopOffset` and `nextPagePopScale` add subtle feedback on the new top page.

### 5) Tips
- Keep pile pages under the same Canvas for consistent scaling.
- You can reuse the same controller and call `ResetPile(newPages)` to refresh content.

## 3D Pile Interaction (Interactable world object)
Use this when you have a 3D model of a paper pile that the player looks at and presses E to start swiping in UI.

### A) Scene setup
- Add your 3D pile model to the scene (collider required).
- Add `NotePilePickable` to the pile root GameObject.
- Add/assign a UI Canvas that contains your pile pages and a `NotePileSwiper` (see previous section). Disable this Canvas by default.
- In `NotePilePickable`:
  - Set `pileCanvas` to that Canvas.
  - Set `pileSwiper` to the `NotePileSwiper` component.
  - Configure `interactionRange`/`useCrosshairDetection`/`pileLayerMask` and optional prompt UI.

### B) Flow
1. Player looks at or moves near the 3D pile and presses E.
2. Player controls are disabled; the pile Canvas is enabled and swiping starts.
3. Player swipes pages with A/D or arrow keys until none remain.
4. If a `finalNote` is assigned on the swiper, it opens in `NoteLetterUI`.
5. When the note is closed, controls are restored and the pile Canvas is hidden.

### C) Notes
- `NotePileSwiper.OnPileCleared` is fired once when the pile is empty. The pickable uses it to hook into `NoteLetterUI.OnNoteClosed` to restore controls after the final note closes.
- If no `finalNote` or `NoteLetterUI` is assigned, controls restore immediately on pile clear.

### D) Cancel, Resume, and Persistence
- Press `Esc` during swiping to cancel. Controls are restored and current progress is saved if `persistProgress` is enabled.
- To enable persistence on the swiper, set:
  - `persistProgress = true`
  - `pileId` = unique string for this pile (e.g., `OfficePile_A`)
- Progress saved = number of remaining pages. On next activation, the swiper hides discarded pages and resumes where the player left off.