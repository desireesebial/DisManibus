# 🏛️ Head Shrine Puzzle - Example Scene Setup

## 🎯 **Complete Example Scene**

This guide shows you how to create a complete Head Shrine Puzzle scene from scratch.

## 🏗️ **Scene Structure**

```
HeadShrineScene
├── Environment
│   ├── Ground (Plane)
│   ├── Walls (Cubes)
│   └── Lighting
├── HeadShrine (GameObject with HeadShrinePuzzle script)
│   ├── Altar1 (Cylinder)
│   │   └── Brazier (Sphere)
│   ├── Altar2 (Cylinder)
│   │   └── Brazier (Sphere)
│   └── Altar3 (Cylinder)
│       └── Brazier (Sphere)
├── DullahanHeads (Spawn points for heads)
│   ├── HeadSpawn1 (Empty GameObject)
│   ├── HeadSpawn2 (Empty GameObject)
│   └── HeadSpawn3 (Empty GameObject)
├── Rewards
│   ├── RewardDoor (Door with Door script)
│   └── RewardSpawnPoint (Empty GameObject)
└── Player (FirstPersonController)
```

## 🛠️ **Step-by-Step Setup**

### **Step 1: Create the Environment**

#### **Ground:**
1. Create a **Plane** (GameObject → 3D Object → Plane)
2. Scale it to `(10, 1, 10)` for a large area
3. Name it `"Ground"`
4. Apply a stone or grass material

#### **Walls (Optional):**
1. Create **Cubes** for walls around the shrine
2. Scale them to create an enclosed area
3. Apply stone or brick materials
4. Name them `"Wall1"`, `"Wall2"`, etc.

#### **Lighting:**
1. Create a **Directional Light** for main lighting
2. Create **Point Lights** around the shrine for atmosphere
3. Set the point lights to a warm color (orange/yellow)
4. Enable shadows for dramatic effect

### **Step 2: Create the Head Shrine**

#### **Main Shrine Object:**
1. Create an **empty GameObject**
2. Name it `"HeadShrine"`
3. Position it at `(0, 0, 0)`
4. Add the `HeadShrinePuzzle` script

#### **Altar 1:**
1. Create a **Cylinder** (GameObject → 3D Object → Cylinder)
2. Name it `"Altar1"`
3. Make it a child of `HeadShrine`
4. Position it at `(-3, 0, 0)`
5. Scale it to `(1, 0.5, 1)`
6. Apply a stone material

#### **Brazier 1:**
1. Create a **Sphere** (GameObject → 3D Object → Sphere)
2. Name it `"Brazier"`
3. Make it a child of `Altar1`
4. Position it at `(0, 0.8, 0)`
5. Scale it to `(0.6, 0.6, 0.6)`
6. Apply a dark stone material

#### **Repeat for Altars 2 and 3:**
- **Altar2**: Position at `(0, 0, 0)`
- **Altar3**: Position at `(3, 0, 0)`
- Same setup for braziers

### **Step 3: Create Materials**

#### **Unlit Brazier Material:**
1. Create a new **Material**
2. Name it `"UnlitBrazier"`
3. Set **Albedo** to dark gray/black
4. Set **Metallic** to 0.8
5. Set **Smoothness** to 0.3

#### **Lit Brazier Material:**
1. Create a new **Material**
2. Name it `"LitBrazier"`
3. Set **Albedo** to orange/red
4. Set **Emission** to orange/red with intensity 2
5. Set **Metallic** to 0.8
6. Set **Smoothness** to 0.3

#### **Altar Material:**
1. Create a new **Material**
2. Name it `"AltarStone"`
3. Set **Albedo** to light gray
4. Set **Metallic** to 0.2
5. Set **Smoothness** to 0.1

#### **Filled Altar Material:**
1. Create a new **Material**
2. Name it `"FilledAltar"`
3. Set **Albedo** to slightly different gray
4. Set **Emission** to very subtle blue with intensity 0.5

### **Step 4: Create Fire Effects**

#### **Fire Particle System:**
1. Create an **empty GameObject**
2. Name it `"FireParticles"`
3. Add a **Particle System** component
4. Configure the particle system:
   - **Start Lifetime**: 2
   - **Start Speed**: 1
   - **Start Size**: 0.5
   - **Start Color**: Orange to Red gradient
   - **Emission Rate**: 50
   - **Shape**: Cone
   - **Gravity Modifier**: -0.5
5. Make it a **Prefab**

#### **Brazier Light:**
1. Create an **empty GameObject**
2. Name it `"BrazierLight"`
3. Add a **Light** component
4. Set **Type** to Point
5. Set **Color** to orange
6. Set **Intensity** to 2
7. Set **Range** to 5
8. Make it a **Prefab**

### **Step 5: Create Audio**

#### **Audio Clips Needed:**
1. **Head Placed Sound**: Soft "thud" or "clink"
2. **Brazier Lit Sound**: Fire ignition sound
3. **Shrine Complete Sound**: Triumphant or mystical sound
4. **Wrong Head Sound**: Rejection or error sound
5. **Mystical Chanting**: Ambient background chanting

#### **Audio Setup:**
1. Import your audio files
2. Set **Compression Format** to Vorbis for smaller file sizes
3. Set **Load Type** to Compressed In Memory
4. Set **3D Sound Settings** as needed

### **Step 6: Create Head Spawn Points**

#### **Head Spawn Points:**
1. Create **empty GameObjects** for each head spawn
2. Name them `"HeadSpawn1"`, `"HeadSpawn2"`, `"HeadSpawn3"`
3. Position them around the shrine area
4. Add `DullahanHeadPickable` components to each
5. Assign the appropriate `DullahanHeadSO` to each

### **Step 7: Create Rewards**

#### **Reward Door:**
1. Create a **Cube** for the door
2. Name it `"RewardDoor"`
3. Position it near the shrine
4. Add a **Door** script (if you have one)
5. Set it to be locked initially

#### **Reward Spawn Point:**
1. Create an **empty GameObject**
2. Name it `"RewardSpawnPoint"`
3. Position it where rewards should appear
4. Assign it to the shrine puzzle script

### **Step 8: Configure the Head Shrine Puzzle**

#### **In the Inspector:**
```csharp
[Header("🏛️ Shrine Settings")]
requiredHeadIDs = { 1, 2, 3 };
interactionDistance = 4f;

[Header("🎨 Visual Settings")]
unlitBrazierMaterial = UnlitBrazier;
litBrazierMaterial = LitBrazier;
emptyAltarMaterial = AltarStone;
filledAltarMaterial = FilledAltar;

[Header("🔥 Fire Effects")]
fireParticlePrefab = FireParticles;
brazierLightPrefab = BrazierLight;

[Header("🎵 Audio")]
headPlacedSound = [Your placement sound];
brazierLitSound = [Your lighting sound];
shrineCompleteSound = [Your completion sound];
wrongHeadSound = [Your wrong head sound];
mysticalChantingSound = [Your chanting sound];

[Header("🎁 Rewards")]
rewardDoor = RewardDoor;
rewardItems = [Your reward items array];
rewardSpawnPoint = RewardSpawnPoint;
```

## 🎮 **Testing the Scene**

### **Test Checklist:**
1. **Player can approach shrine** ✅
2. **Player can pick up heads** ✅
3. **Player can place heads on altars** ✅
4. **Braziers light up when heads placed** ✅
5. **Fire particles start when braziers light** ✅
6. **Audio plays for all actions** ✅
7. **Shrine completes when all altars lit** ✅
8. **Rewards are granted on completion** ✅

### **Common Issues:**
1. **Heads not spawning**: Check `DullahanHeadPickable` components
2. **Materials not applied**: Check material assignments
3. **Audio not playing**: Check AudioSource and audio clips
4. **Particles not working**: Check ParticleSystem components
5. **Lights not working**: Check Light components

## 🎨 **Visual Polish**

### **Atmospheric Elements:**
1. **Fog**: Add fog for mystical atmosphere
2. **Particles**: Add ambient particle effects
3. **Lighting**: Use warm, dramatic lighting
4. **Textures**: Use high-quality stone textures
5. **Post-Processing**: Add bloom and color grading

### **Animation:**
1. **Shrine Completion**: Animate the shrine when complete
2. **Brazier Lighting**: Animate the brazier when lit
3. **Head Placement**: Animate heads when placed
4. **Reward Spawning**: Animate rewards when they appear

## 🚀 **Performance Optimization**

### **Optimization Tips:**
1. **Use LOD groups** for distant objects
2. **Optimize particle systems** for performance
3. **Use object pooling** for effects
4. **Optimize materials** with shared textures
5. **Use audio pooling** for sounds

### **Mobile Considerations:**
1. **Reduce particle count** for mobile
2. **Use simpler materials** for mobile
3. **Optimize audio** for mobile
4. **Use lower resolution textures** for mobile

## 🎯 **Final Result**

You should have a beautiful, atmospheric Head Shrine Puzzle that:
- **Looks mystical and ancient**
- **Has clear visual feedback**
- **Plays atmospheric audio**
- **Provides satisfying completion**
- **Grants meaningful rewards**

The puzzle should feel like a sacred ritual where players are placing heads on ancient altars to awaken a mystical shrine!
