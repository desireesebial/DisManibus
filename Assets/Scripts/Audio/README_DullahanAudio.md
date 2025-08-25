# Dullahan Audio System Setup Guide

This guide explains how to set up the comprehensive audio system for the Dullahan chase mechanics and three heads puzzle.

## Overview

The `DullahanAudioManager` provides a centralized audio system that handles:
- **Chase Intensity Audio** - Dynamic audio that changes based on Dullahan proximity
- **Head Puzzle Audio** - Sounds for head pickup, effects, and puzzle completion
- **Effect Audio** - Sounds for player buffs/debuffs
- **Ambient Audio** - Background tension and atmosphere sounds

## Audio File Requirements

### Chase Intensity Audio (4 clips)
1. **Chase Start Sound** - Plays when Dullahan begins chasing
2. **Chase End Sound** - Plays when chase ends
3. **Chase Loop Sound** - Looping background during chase
4. **Chase Intensity Clips** (4 clips) - Different intensity levels (0-3)

### Heartbeat Audio (3-4 clips)
- **Heartbeat Clips** - Player heartbeat at different intensities
- Should be 2D audio (spatialBlend = 0)
- Recommended: 3-4 different heartbeat speeds

### Dullahan Footsteps (3-4 clips)
- **Dullahan Footsteps** - Footstep sounds at different speeds
- Should be 3D audio (spatialBlend = 1)
- Recommended: 3-4 different footstep speeds

### Head Puzzle Audio (8 clips)
1. **Head Pickup Sound** - Generic head pickup
2. **Real Head Pickup Sound** - Special sound for real head
3. **Fake Head Pickup Sound** - Special sound for fake heads
4. **Head Drop Sound** - When head is dropped
5. **Wrong Head Sound** - When wrong head is used
6. **Head Effect Sound** - When head effect is applied
7. **Head Glow Sound** - Visual effect sound
8. **Head Attach Sound** - When head is attached to body

### Body Interaction Audio (4 clips)
1. **Body Activate Sound** - When body becomes active
2. **Door Unlock Sound** - When final door unlocks
3. **Puzzle Complete Sound** - When puzzle is completed

### Effect Audio (6 clips)
1. **Speed Boost Sound** - When speed boost is applied
2. **Speed Debuff Sound** - When speed debuff is applied
3. **Vision Boost Sound** - When vision boost is applied
4. **Vision Debuff Sound** - When vision debuff is applied
5. **Fear Effect Sound** - When fear effect is applied
6. **Calm Effect Sound** - When calm effect is applied

### Ambient Audio (3-4 clips)
1. **Dullahan Ambient Sound** - Background Dullahan atmosphere
2. **Puzzle Ambient Sound** - Background puzzle atmosphere
3. **Ambient Tension Clips** (3-4 clips) - Progressive tension sounds

## Setup Instructions

### 1. Create Audio Manager GameObject

1. **Create empty GameObject** named "DullahanAudioManager"
2. **Add DullahanAudioManager script**
3. **Configure audio sources** (will be auto-created if not assigned)

### 2. Configure Audio Sources

The script will automatically create and configure:
- **Dullahan Audio Source** - 3D spatial audio for Dullahan sounds
- **Player Audio Source** - 2D audio for player effects
- **Ambient Audio Source** - 2D audio for background sounds
- **Effect Audio Source** - 2D audio for UI/effect sounds

### 3. Assign Audio Clips

#### Chase Intensity Audio
```
Chase Start Sound: [AudioClip] - Dullahan chase start
Chase End Sound: [AudioClip] - Dullahan chase end
Chase Loop Sound: [AudioClip] - Looping chase background

Chase Intensity Clips: [Array Size: 4]
- Element 0: Low intensity chase audio
- Element 1: Medium intensity chase audio
- Element 2: High intensity chase audio
- Element 3: Maximum intensity chase audio
```

#### Heartbeat Audio
```
Heartbeat Clips: [Array Size: 3-4]
- Element 0: Slow heartbeat (low intensity)
- Element 1: Medium heartbeat (medium intensity)
- Element 2: Fast heartbeat (high intensity)
- Element 3: Very fast heartbeat (maximum intensity)
```

#### Dullahan Footsteps
```
Dullahan Footsteps: [Array Size: 3-4]
- Element 0: Slow footsteps
- Element 1: Medium footsteps
- Element 2: Fast footsteps
- Element 3: Very fast footsteps
```

#### Head Puzzle Audio
```
Head Pickup Sound: [AudioClip] - Generic pickup
Real Head Pickup Sound: [AudioClip] - Special real head sound
Fake Head Pickup Sound: [AudioClip] - Special fake head sound
Head Drop Sound: [AudioClip] - Head drop sound
Wrong Head Sound: [AudioClip] - Wrong head attempt
Head Effect Sound: [AudioClip] - Effect application
Head Glow Sound: [AudioClip] - Visual glow effect
Head Attach Sound: [AudioClip] - Head attachment
```

#### Body Interaction Audio
```
Body Activate Sound: [AudioClip] - Body activation
Door Unlock Sound: [AudioClip] - Door unlock
Puzzle Complete Sound: [AudioClip] - Puzzle completion
```

#### Effect Audio
```
Speed Boost Sound: [AudioClip] - Speed boost effect
Speed Debuff Sound: [AudioClip] - Speed debuff effect
Vision Boost Sound: [AudioClip] - Vision boost effect
Vision Debuff Sound: [AudioClip] - Vision debuff effect
Fear Effect Sound: [AudioClip] - Fear effect
Calm Effect Sound: [AudioClip] - Calm effect
```

#### Ambient Audio
```
Dullahan Ambient Sound: [AudioClip] - Background atmosphere
Puzzle Ambient Sound: [AudioClip] - Puzzle atmosphere

Ambient Tension Clips: [Array Size: 3-4]
- Element 0: Low tension background
- Element 1: Medium tension background
- Element 2: High tension background
- Element 3: Maximum tension background
```

### 4. Configure Audio Settings

#### Volume Settings
```
Master Volume: 1.0 (Overall volume control)
Chase Volume: 0.8 (Dullahan chase audio volume)
Effect Volume: 0.6 (Player effects volume)
Ambient Volume: 0.4 (Background audio volume)
```

#### Crossfade Settings
```
Crossfade Time: 2.0 (Time for audio transitions)
Heartbeat Fade Time: 1.0 (Heartbeat fade in/out time)
```

#### Spatial Audio Settings
```
Dullahan Max Distance: 20.0 (Max hearing distance for Dullahan)
Dullahan Min Distance: 1.0 (Min hearing distance for Dullahan)
Player Max Distance: 10.0 (Max distance for player audio)
```

## Integration with Existing Scripts

### DullahanChaseSystem Integration
The chase system automatically integrates with the audio manager:
- Calls `audioManager.StartChase()` when chase begins
- Calls `audioManager.EndChase()` when chase ends
- Calls `audioManager.SetChaseIntensity(intensity)` during chase

### Head Pickable Integration
Head pickup automatically plays appropriate sounds:
- Real head: `audioManager.PlayHeadPickupSound(HeadType.Real)`
- Fake heads: `audioManager.PlayHeadPickupSound(HeadType.Fake1/Fake2)`

### Effect Manager Integration
Effect application automatically plays effect sounds:
- Speed effects: `audioManager.PlaySpeedBoostSound()` / `PlaySpeedDebuffSound()`
- Vision effects: `audioManager.PlayVisionBoostSound()` / `PlayVisionDebuffSound()`
- Fear/Calm effects: `audioManager.PlayFearEffectSound()` / `PlayCalmEffectSound()`

### Body Integration
Body interactions automatically play sounds:
- Head attachment: `audioManager.PlayHeadAttachSound()`
- Door unlock: `audioManager.PlayDoorUnlockSound()`

### Puzzle Manager Integration
Puzzle events automatically play sounds:
- Puzzle start: `audioManager.StartPuzzleAmbient()`
- Puzzle complete: `audioManager.PlayPuzzleCompleteSound()`

## Audio File Recommendations

### File Formats
- **Format**: WAV or OGG
- **Sample Rate**: 44.1 kHz
- **Bit Depth**: 16-bit or 24-bit
- **Channels**: Mono for 3D sounds, Stereo for 2D sounds

### File Organization
```
Assets/Audio/Dullahan/
├── Chase/
│   ├── ChaseStart.wav
│   ├── ChaseEnd.wav
│   ├── ChaseLoop.wav
│   └── Intensity/
│       ├── Intensity0.wav
│       ├── Intensity1.wav
│       ├── Intensity2.wav
│       └── Intensity3.wav
├── Heartbeat/
│   ├── HeartbeatSlow.wav
│   ├── HeartbeatMedium.wav
│   ├── HeartbeatFast.wav
│   └── HeartbeatVeryFast.wav
├── Footsteps/
│   ├── FootstepsSlow.wav
│   ├── FootstepsMedium.wav
│   ├── FootstepsFast.wav
│   └── FootstepsVeryFast.wav
├── Heads/
│   ├── HeadPickup.wav
│   ├── RealHeadPickup.wav
│   ├── FakeHeadPickup.wav
│   ├── HeadDrop.wav
│   ├── WrongHead.wav
│   ├── HeadEffect.wav
│   ├── HeadGlow.wav
│   └── HeadAttach.wav
├── Body/
│   ├── BodyActivate.wav
│   ├── DoorUnlock.wav
│   └── PuzzleComplete.wav
├── Effects/
│   ├── SpeedBoost.wav
│   ├── SpeedDebuff.wav
│   ├── VisionBoost.wav
│   ├── VisionDebuff.wav
│   ├── FearEffect.wav
│   └── CalmEffect.wav
└── Ambient/
    ├── DullahanAmbient.wav
    ├── PuzzleAmbient.wav
    └── Tension/
        ├── TensionLow.wav
        ├── TensionMedium.wav
        ├── TensionHigh.wav
        └── TensionMax.wav
```

## Audio Import Settings

### 3D Audio (Dullahan sounds)
```
Force To Mono: Enabled
Load Type: Compressed In Memory
Compression Format: Vorbis/MP3
Quality: 70-80%
```

### 2D Audio (Player effects, ambient)
```
Force To Mono: Disabled (for stereo)
Load Type: Compressed In Memory
Compression Format: Vorbis/MP3
Quality: 70-80%
```

### Loop Audio (Ambient, chase loop)
```
Force To Mono: Enabled
Load Type: Streaming
Compression Format: Vorbis/MP3
Quality: 70-80%
```

## Testing and Debugging

### Audio Testing Checklist
- [ ] Chase start/end sounds play correctly
- [ ] Intensity audio crossfades smoothly
- [ ] Heartbeat changes with intensity
- [ ] Footsteps play at correct intervals
- [ ] Head pickup sounds play for each type
- [ ] Effect sounds play when applied
- [ ] Ambient audio transitions smoothly
- [ ] Volume levels are appropriate
- [ ] 3D audio positioning works correctly

### Common Issues
1. **No audio playing**: Check AudioSource components and volume settings
2. **Audio not changing with intensity**: Verify intensity clips array is assigned
3. **3D audio not working**: Check spatialBlend and rolloff settings
4. **Audio overlapping**: Adjust crossfade times and volume levels
5. **Performance issues**: Use streaming for long audio files

## Performance Optimization

### Audio Optimization Tips
- Use compressed audio formats (Vorbis/MP3)
- Stream long audio files (ambient, loops)
- Limit concurrent audio sources
- Use audio pooling for frequent sounds
- Adjust quality settings based on platform

### Memory Management
- Unload unused audio clips
- Use audio compression
- Monitor audio memory usage
- Implement audio culling for distant sounds

## Platform-Specific Considerations

### Mobile
- Reduce audio quality to 60-70%
- Use shorter audio files
- Limit concurrent audio sources
- Test on actual devices

### PC
- Higher audio quality (80-90%)
- More concurrent audio sources
- Longer audio files acceptable
- Better 3D audio support

### Console
- Follow platform-specific guidelines
- Optimize for target hardware
- Test on target platforms
