# Floor 2 Ending Event System - Complete Summary

## System Overview

The Floor 2 Ending Event System creates a branching narrative where players must choose between helping Dullahan find his real head (good ending) or leaving directly to Floor 1 (bad ending). The choice made in Floor 2 determines which ending the player will receive **after completing Floor 1** (the final level).

## Core Components

### 1. Floor2EndingEventManager.cs (Floor 2)
**Main Controller Script for Floor 2**
- Manages the choice event in Floor 2
- Handles player choice with 45-second timer
- Saves choice to PlayerPrefs for persistence across scenes
- Integrates with all existing systems (inventory, quests, doors, audio)
- Provides comprehensive debug features

**Key Features:**
- Proximity-based event triggering
- Timed choice system with UI feedback
- State machine for event progression
- Choice persistence using PlayerPrefs
- Automatic integration with existing systems

### 2. Floor1EndingManager.cs (Floor 1)
**Ending Trigger Script for Floor 1**
- Checks the saved choice from Floor 2
- Triggers appropriate ending when Floor 1 is completed
- Manages ending UI and scene transitions
- Handles quest completion for different endings

**Key Features:**
- Automatic choice detection from Floor 2
- Ending UI management
- Scene transition handling
- Quest completion integration

### 3. Quest Integration
**Quest ScriptableObjects:**
- `HelpDullahanQuest`: Main quest for good ending path
- `EscapeQuest`: Quest for bad ending path
- `FindRealHeadQuest`: Sub-quest for finding real head
- `AttachHeadQuest`: Sub-quest for attaching head to body

**Quest System Integration:**
- Automatic quest starting based on player choice
- Progress tracking and completion
- Quest UI updates and notifications

### 4. UI System
**Floor 2 UI Components:**
1. **Proximity Warning UI**: Shows when player enters event area
2. **Choice UI**: Two-button interface for player decision
3. **Timer UI**: Countdown display with visual feedback

**Floor 1 UI Components:**
1. **Good Ending UI**: Display for good ending
2. **Bad Ending UI**: Display for bad ending
3. **Continue Button**: Button to proceed to ending scene

**UI Features:**
- Customizable colors and text
- Timer with warning threshold
- Responsive button interactions
- Automatic show/hide based on event state

### 5. Door Management
**Two Critical Doors:**
- **Exit Door** (Key ID: 999): Leads to Floor 1 (final level)
- **Real Head Door** (Key ID: 100): Access to real head room (good ending path)

**Door Operations:**
- Automatic locking/unlocking based on player choice
- Integration with existing door script system
- Visual and audio feedback for door operations

## Event Flow

### Phase 1: Floor 2 Proximity Detection
```
Player enters proximity radius (15 units)
↓
Warning UI appears: "Dullahan's territory... Choose your path carefully."
↓
3-second delay
↓
Choice UI appears with 45-second timer
```

### Phase 2: Floor 2 Player Choice
```
Player has 45 seconds to choose:
├─ Help Dullahan (Good Ending Path)
│  ├─ Real head door opens
│  ├─ HelpDullahanQuest starts
│  ├─ Player must find and attach real head
│  ├─ Exit door opens to Floor 1
│  └─ Choice saved for ending after Floor 1
│
└─ Leave Directly (Bad Ending Path)
   ├─ Exit door opens immediately to Floor 1
   ├─ EscapeQuest starts
   └─ Choice saved for ending after Floor 1
```

### Phase 3: Floor 1 Completion
```
Player completes Floor 1 (final level)
↓
Floor1EndingManager checks saved choice from Floor 2
↓
Appropriate ending is triggered:
├─ Good Ending: Player helped Dullahan
└─ Bad Ending: Player abandoned Dullahan
```

## Integration Points

### Existing System Integration
1. **DullahanHeadInventory**: Automatic head collection and management
2. **DullahanBody**: Head attachment detection and puzzle completion
3. **DullahanPuzzleManager**: Head spawning and puzzle management
4. **DullahanChaseEventManager**: Chase system integration
5. **QuestSystem**: Quest management and progression
6. **SceneTransitionManager**: Smooth scene transitions

### Choice Persistence System
- **PlayerPrefs Storage**: Choice saved as integer (1 = good, 0 = bad)
- **Cross-Scene Persistence**: Choice survives scene transitions
- **Static Access Methods**: Easy access from any script
- **Clear Functionality**: Reset choice for new game

### Audio Integration
- **Floor 2 Audio**: Choice music, path-specific music
- **Floor 1 Audio**: Ending music, transition sounds
- **Door Sounds**: Integration with existing door audio system

## Technical Implementation

### State Management
```csharp
// Floor 2 States
public enum EventState
{
    Waiting,        // Initial state
    Proximity,      // Player in area
    Choice,         // Decision phase
    BadEnding,      // Bad ending path
    GoodEnding,     // Good ending path
    Completion      // Event complete
}
```

### Key Methods
- `OnPlayerEnteredProximity()`: Triggers event sequence (Floor 2)
- `OnHelpChosen()`: Initiates good ending path (Floor 2)
- `OnLeaveChosen()`: Initiates bad ending path (Floor 2)
- `SaveEndingChoice()`: Saves choice to PlayerPrefs (Floor 2)
- `GetEndingChoice()`: Retrieves saved choice (Floor 1)
- `TriggerGoodEnding()`: Triggers good ending (Floor 1)
- `TriggerBadEnding()`: Triggers bad ending (Floor 1)

### Choice Persistence
```csharp
// Save choice (Floor 2)
PlayerPrefs.SetInt("Floor2EndingChoice", choseGoodEnding ? 1 : 0);
PlayerPrefs.Save();

// Check choice (Floor 1)
bool choseGoodEnding = Floor2EndingEventManager.GetEndingChoice();

// Clear choice (new game)
Floor2EndingEventManager.ClearEndingChoice();
```

### Debug Features
- **Floor 2 Debug Mode**: Trigger events, force choices
- **Floor 1 Debug Mode**: Force endings, trigger completion
- **Comprehensive Logging**: All state changes and actions
- **Choice Verification**: Check saved choices

## Setup Requirements

### Floor 2 Scene Setup
1. **Floor2EndingEventManager GameObject**: Main controller
2. **UI Canvas**: Proximity, choice, and timer UI elements
3. **Doors**: Exit door and real head door with proper key IDs
4. **Player**: Must have "Player" tag
5. **Audio Sources**: For music and sound effects

### Floor 1 Scene Setup
1. **Floor1EndingManager GameObject**: Ending trigger system
2. **UI Canvas**: Good/Bad ending UI elements
3. **Ending Trigger Point**: Position for completion detection
4. **Audio Sources**: For ending music and sounds

### Required Scripts
- `Floor2EndingEventManager.cs` (Floor 2)
- `Floor1EndingManager.cs` (Floor 1)
- `QuestLogEntry.cs` (UI component)
- `Floor2Quests.cs` (Quest definitions)
- `DullahanBodyIntegration.cs` (Integration helper)
- All existing Dullahan system scripts

### Quest ScriptableObjects
Create via Unity menu: Create → Scriptable Objects → Quests
- HelpDullahanQuest
- EscapeQuest
- FindRealHeadQuest
- AttachHeadQuest

## Customization Options

### Timing Adjustments
- **Proximity Radius**: Distance to trigger event (default: 15 units)
- **Choice Time Limit**: Decision time (default: 45 seconds)
- **Warning Threshold**: Timer warning point (default: 10 seconds)
- **Ending Display Time**: Time to show ending text (default: 5 seconds)

### UI Customization
- **Colors**: Timer and UI colors
- **Text**: All UI text can be customized
- **Layout**: UI positioning and styling
- **Animations**: Optional UI animations

### Audio Customization
- **Music Clips**: Different music for each phase
- **Volume Levels**: Individual audio source control
- **Fade Effects**: Smooth audio transitions

## Performance Considerations

### Optimization Features
- **Efficient Updates**: Only runs when event is active
- **Cached References**: Minimizes FindObjectOfType calls
- **Coroutine Usage**: Non-blocking delays and transitions
- **State-Based Processing**: Only processes relevant states

### Memory Management
- **Proper Cleanup**: Resets all references on completion
- **Audio Management**: Proper audio clip handling
- **UI Cleanup**: Automatic UI element management
- **PlayerPrefs Management**: Efficient choice storage

## Future Enhancements

### Potential Additions
1. **Multiple Choice Paths**: More than two options
2. **Dynamic Consequences**: Different outcomes based on player history
3. **Save System Integration**: Persist choice across sessions
4. **Achievement System**: Unlock achievements for different paths
5. **Branching Dialogue**: Different NPC interactions
6. **Environmental Changes**: Visual changes based on choice
7. **Multiple Endings**: More than just good/bad endings

### Scalability
- **Modular Design**: Easy to add new events
- **Quest Integration**: Supports complex narratives
- **State Machine**: Enables complex event flows
- **PlayerPrefs System**: Allows for multiple choice tracking
- **Component-Based**: Easy to extend and modify

## Troubleshooting Guide

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

// Clear choice (new game)
Floor2EndingEventManager.ClearEndingChoice();
```

## Conclusion

The Floor 2 Ending Event System provides a complete, integrated solution for creating a branching narrative experience that spans multiple levels. It seamlessly integrates with existing systems while providing extensive customization options and robust error handling.

The system successfully creates meaningful player choice with clear consequences that are revealed at the end of the game, enhancing the overall game experience and replayability. The choice persistence system ensures that player decisions have lasting impact throughout the game.

Key Features:
- **Cross-Level Choice System**: Choices in Floor 2 affect endings after Floor 1
- **Persistent Choice Storage**: PlayerPrefs system maintains choices across scenes
- **Modular Design**: Easy to extend and modify
- **Comprehensive Integration**: Works with all existing Dullahan systems
- **Robust Debug System**: Extensive testing and debugging capabilities
