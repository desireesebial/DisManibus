# Floor 2 Ending Event System Setup Guide

## Overview
The Floor 2 Ending Event System creates a branching narrative where players must choose between helping Dullahan find his real head (good ending) or leaving directly to Floor 1 (bad ending). The choice made in Floor 2 determines which ending the player will receive **after completing Floor 1** (the final level).

## Event Flow

### 1. Proximity Trigger (Floor 2)
- Player enters a designated area (proximity radius)
- Warning UI appears: "Dullahan's territory... Choose your path carefully."
- After 3 seconds, choice UI appears

### 2. Player Choice (Floor 2 - 45-second timer)
- **Help Dullahan**: Sets up good ending path for after Floor 1
- **Leave Directly**: Sets up bad ending path for after Floor 1
- If no choice made within 45 seconds, defaults to bad ending

### 3. Path Execution (Floor 2)
**Good Ending Path:**
- Real head door opens
- Player must find and collect the real head
- Player must attach real head to Dullahan's body
- Exit door opens to Floor 1 (final level)
- Choice is saved for ending after Floor 1 completion

**Bad Ending Path:**
- Exit door opens immediately to Floor 1 (final level)
- Choice is saved for ending after Floor 1 completion

### 4. Floor 1 Completion
- Player completes Floor 1 (final level)
- Floor1EndingManager checks the saved choice from Floor 2
- Appropriate ending is triggered based on Floor 2 choice

## Required Components

### 1. Floor2EndingEventManager (Floor 2)
- Main controller for the Floor 2 choice event
- Manages player choice, timers, and state transitions
- Saves choice to PlayerPrefs for persistence across scenes
- Integrates with existing systems

### 2. Floor1EndingManager (Floor 1)
- Checks the saved choice from Floor 2
- Triggers appropriate ending when Floor 1 is completed
- Manages ending UI and scene transitions

### 3. UI Elements
- **Proximity Warning UI**: Shows when player enters area (Floor 2)
- **Choice UI**: Two buttons for player decision (Floor 2)
- **Timer UI**: Countdown for choice making (Floor 2)
- **Ending UI**: Good/Bad ending display (Floor 1)

### 4. Doors
- **Exit Door**: Leads to Floor 1 (final level)
- **Real Head Door**: Access to real head room (good ending path)

### 5. Quest Integration
- **HelpDullahanQuest**: Main quest for good ending path
- **EscapeQuest**: Quest for bad ending path
- **FindRealHeadQuest**: Sub-quest for finding real head
- **AttachHeadQuest**: Sub-quest for attaching head

## Setup Instructions

### Step 1: Floor 2 Setup
1. Create an empty GameObject named "Floor2EndingEventManager"
2. Add the `Floor2EndingEventManager` script
3. Configure the following settings:
   - **Proximity Radius**: 15 units (adjust as needed)
   - **Choice Time Limit**: 45 seconds
   - **Event Trigger Point**: Position where event should start

### Step 2: Floor 1 Setup
1. Create an empty GameObject named "Floor1EndingManager"
2. Add the `Floor1EndingManager` script
3. Configure the following settings:
   - **Ending Trigger Point**: Position where Floor 1 completion is detected
   - **Good Ending Scene**: Scene name for good ending
   - **Bad Ending Scene**: Scene name for bad ending

### Step 3: Setup UI Elements (Floor 2)
1. **Proximity Warning UI**:
   - Create Canvas with warning text
   - Assign to `proximityUI` and `proximityText`

2. **Choice UI**:
   - Create Canvas with two buttons
   - Assign to `choiceUI`, `helpButton`, `leaveButton`, `choiceText`

3. **Timer UI**:
   - Create timer display with countdown
   - Assign to `timerUI`, `timerText`, `timerFillImage`

### Step 4: Setup UI Elements (Floor 1)
1. **Good Ending UI**:
   - Create Canvas for good ending display
   - Assign to `goodEndingUI`, `endingText`

2. **Bad Ending UI**:
   - Create Canvas for bad ending display
   - Assign to `badEndingUI`

3. **Continue Button**:
   - Create button for continuing to ending scene
   - Assign to `continueButton`

### Step 5: Configure Doors
1. **Exit Door**:
   - Find or create door leading to Floor 1
   - Set `requiredKeyID` to 999
   - Assign to `exitDoor`

2. **Real Head Door**:
   - Find or create door to real head room
   - Set `requiredKeyID` to 100
   - Assign to `realHeadDoor`

### Step 6: Create Quest ScriptableObjects
1. Right-click in Project window → Create → Scriptable Objects → Quests
2. Create the following quests:
   - **HelpDullahanQuest**: Main good ending quest
   - **EscapeQuest**: Bad ending quest
   - **FindRealHeadQuest**: Sub-quest for finding head
   - **AttachHeadQuest**: Sub-quest for attaching head

### Step 7: Audio Setup
1. **Floor 2 Audio**:
   - Add AudioSource component to Floor2EndingEventManager
   - Assign audio clips:
     - `choiceMusic`: Music during choice phase
     - `badEndingMusic`: Music for bad ending path
     - `goodEndingMusic`: Music for good ending path

2. **Floor 1 Audio**:
   - Add AudioSource component to Floor1EndingManager
   - Assign audio clips:
     - `goodEndingMusic`: Music for good ending
     - `badEndingMusic`: Music for bad ending
     - `endingTransitionSound`: Sound for scene transition

## Scene Configuration

### Floor 2 Scene Requirements
1. **Player**: Must have "Player" tag
2. **DullahanHeadInventory**: Player inventory system
3. **DullahanBody**: Body for head attachment
4. **DullahanPuzzleManager**: Manages head collection
5. **QuestSystem**: Quest management system
6. **Floor2EndingEventManager**: Choice event system

### Floor 1 Scene Requirements
1. **Player**: Must have "Player" tag
2. **QuestSystem**: Quest management system
3. **Floor1EndingManager**: Ending trigger system
4. **SceneTransitionManager**: For scene transitions

### Door Setup
```csharp
// Exit Door (to Floor 1)
exitDoor.requiredKeyID = 999;
exitDoor.isLocked = true;

// Real Head Door (Good Ending Path)
realHeadDoor.requiredKeyID = 100;
realHeadDoor.isLocked = true;
```

## Event States

### Floor2EndingEventManager States
- **Waiting**: Initial state, waiting for proximity
- **Proximity**: Player in area, showing warning
- **Choice**: Player making decision
- **BadEnding**: Bad ending path activated
- **GoodEnding**: Good ending path activated
- **Completion**: Event complete

### Floor1EndingManager States
- **Waiting**: Waiting for Floor 1 completion
- **EndingTriggered**: Ending sequence started
- **EndingComplete**: Ending sequence finished

## Integration with Existing Systems

### DullahanChaseEventManager Integration
```csharp
// When player chooses to help
chaseEventManager.playerChoseToHelp = true;
chaseEventManager.choiceMade = true;
```

### Quest System Integration
```csharp
// Start quest when choice made
questSystem.StartQuest(helpDullahanQuest);

// Complete quest when head attached
questSystem.CompleteQuest(helpDullahanQuest);
```

### Choice Persistence
```csharp
// Save choice (Floor 2)
Floor2EndingEventManager.SaveEndingChoice(true);

// Check choice (Floor 1)
bool choseGoodEnding = Floor2EndingEventManager.GetEndingChoice();

// Clear choice (new game)
Floor2EndingEventManager.ClearEndingChoice();
```

## Debug Features

### Floor 2 Debug Mode
Enable `debugMode` in inspector to access:
- **P Key**: Trigger proximity event
- **Space Key**: Force help choice

### Floor 1 Debug Mode
Enable `debugMode` in inspector to access:
- **E Key**: Trigger ending check
- **G Key**: Force good ending
- **B Key**: Force bad ending

### Debug Logging
The system provides comprehensive debug logging:
- Event state changes
- Player choices
- Choice persistence
- Ending triggers

## Customization Options

### Timing Adjustments
- **Proximity Radius**: Distance to trigger event (default: 15 units)
- **Choice Time Limit**: Time to make decision (default: 45 seconds)
- **Warning Threshold**: When timer turns red (default: 10 seconds)
- **Ending Display Time**: Time to show ending text (default: 5 seconds)

### UI Customization
- **Colors**: Timer and UI colors
- **Text**: All UI text can be customized
- **Layout**: UI positioning and styling
- **Animations**: Optional UI animations

### Audio Customization
- **Music Clips**: Different music for each phase
- **Volume Levels**: Individual audio source control
- **Fade Effects**: Audio transitions

## Troubleshooting

### Common Issues
1. **Event not triggering**: Check proximity radius and player tag
2. **Doors not opening**: Verify door assignments and key IDs
3. **UI not showing**: Check Canvas setup and assignments
4. **Choice not persisting**: Verify PlayerPrefs are being saved
5. **Ending not triggering**: Check Floor1EndingManager setup

### Debug Commands
```csharp
// Check current state (Floor 2)
Debug.Log($"Current State: {eventManager.GetCurrentState()}");

// Force event trigger (Floor 2)
eventManager.OnPlayerEnteredProximity();

// Check saved choice (Floor 1)
bool choice = Floor2EndingEventManager.GetEndingChoice();

// Force ending (Floor 1)
floor1EndingManager.ForceGoodEnding();
floor1EndingManager.ForceBadEnding();
```

## Performance Considerations

### Optimization
- Use object pooling for UI elements
- Minimize Update() calls when not needed
- Cache component references
- Use coroutines for delays

### Memory Management
- Properly dispose of audio clips
- Clean up UI references
- Reset event state on scene unload
- Clear PlayerPrefs when starting new game

## Future Enhancements

### Potential Additions
1. **Multiple Choice Paths**: More than two options
2. **Dynamic Consequences**: Different outcomes based on player history
3. **Save System Integration**: Persist choice across sessions
4. **Achievement System**: Unlock achievements for different paths
5. **Branching Dialogue**: Different NPC interactions based on choice
6. **Environmental Changes**: Visual changes based on choice

### Scalability
- Modular design allows easy addition of new events
- Quest system integration supports complex narratives
- State machine pattern enables complex event flows
- PlayerPrefs system allows for multiple choice tracking
