# Phase 4 & 5 Setup Guide - Unity Editor Configuration

This guide explains how to set up the UI/HUD system (Phase 4) and Audio system (Phase 5) in the Unity Editor after the scripts have been restored.

---

## Phase 4: UI/HUD System Setup

### Prerequisites
- **HUDManager.cs** located at `Assets/Scripts/UI/HUDManager.cs`
- **PlayerHealth.cs** with UI events (already configured)
- **TextMeshPro** package installed (should be included with Unity)

### Step 1: Create the HUD Canvas

1. **Create Canvas**
   - Right-click in Hierarchy → **UI** → **Canvas**
   - Rename to "HUD Canvas"
   - Set **Canvas Scaler** component:
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: **1920 x 1080** (or your target resolution)

2. **Add HUDManager Script**
   - Select "HUD Canvas"
   - Click **Add Component**
   - Search for and add **HUDManager**

### Step 2: Create Health Display

1. **Create Health Container**
   - Right-click on "HUD Canvas" → **UI** → **Panel**
   - Rename to "Health Container"
   - **RectTransform** settings:
     - Anchor: Top-Left
     - Position X: 100, Y: -50
     - Width: 300, Height: 80

2. **Create Heart Images**
   - Right-click on "Health Container" → **UI** → **Image**
   - Rename to "Heart 1"
   - **RectTransform** settings:
     - Width: 60, Height: 60
     - Position X: 0, Y: 0

3. **Duplicate Hearts**
   - Duplicate "Heart 1" (Ctrl+D) to create "Heart 2", "Heart 3", etc.
   - Position them horizontally:
     - Heart 1: X = 0
     - Heart 2: X = 70
     - Heart 3: X = 140
     - (Adjust based on your max health)

4. **Assign Heart Sprites**
   - You need two heart sprites:
     - **Full Heart sprite** (filled heart icon)
     - **Empty Heart sprite** (outlined/empty heart icon)
   - Create or import these sprites into `Assets/Art/UI/`
   - Select each Heart Image and set:
     - Source Image: (leave empty for now, will be set dynamically)
     - Color: White (or desired tint)

5. **Connect to HUDManager**
   - Select "HUD Canvas" (HUDManager object)
   - In **HUDManager** component:
     - **Health Container**: Drag "Health Container" object
     - **Health Hearts**: Set size to match your max health (e.g., 3)
     - Drag each heart (Heart 1, Heart 2, Heart 3) into the array slots
     - **Full Heart**: Drag your full heart sprite
     - **Empty Heart**: Drag your empty heart sprite

### Step 3: Create Blink Count Display

1. **Create Blink Count Text**
   - Right-click on "HUD Canvas" → **UI** → **Text - TextMeshPro**
   - If prompted to import TMP Essentials, click **Import TMP Essentials**
   - Rename to "Blink Count Text"
   - **RectTransform** settings:
     - Anchor: Top-Right
     - Position X: -150, Y: -50
     - Width: 200, Height: 60

2. **Configure Text**
   - **TextMeshProUGUI** component:
     - Text: "Blinks: 3/3" (placeholder)
     - Font Size: 32
     - Alignment: Center
     - Color: White

3. **Connect to HUDManager**
   - Select "HUD Canvas" (HUDManager object)
   - In **HUDManager** component:
     - **Blink Count Text**: Drag "Blink Count Text" object

### Step 4: Create Coffee Blink Display

1. **Create Coffee Blink Text**
   - Right-click on "HUD Canvas" → **UI** → **Text - TextMeshPro**
   - Rename to "Coffee Blink Text"
   - **RectTransform** settings:
     - Anchor: Top-Right
     - Position X: -150, Y: -120
     - Width: 300, Height: 60

2. **Configure Text**
   - **TextMeshProUGUI** component:
     - Text: "Coffee Boost: ACTIVE" (placeholder)
     - Font Size: 28
     - Alignment: Center
     - Color: #FFCC66 (coffee color)

3. **Connect to HUDManager**
   - Select "HUD Canvas" (HUDManager object)
   - In **HUDManager** component:
     - **Coffee Blink Text**: Drag "Coffee Blink Text" object

### Step 5: Connect Player Reference

1. **Assign Player Health**
   - Select "HUD Canvas" (HUDManager object)
   - In **HUDManager** component:
     - **Player Health**: Drag your Player GameObject (it will auto-find the PlayerHealth component)
     - **World State Manager**: Leave empty (auto-finds at runtime)

### Step 6: Test Health Display

1. Play the game
2. The health display should show your current/max health
3. Test by taking damage (touching an enemy during blink)
4. Hearts should update automatically

---

## Phase 5: Audio System Setup

### Prerequisites
- **AudioManager.cs** located at `Assets/Scripts/Audio/AudioManager.cs`
- Audio clips for music and SFX (WAV or OGG recommended)

### Step 1: Create AudioManager GameObject

1. **Create Empty GameObject**
   - Right-click in Hierarchy → **Create Empty**
   - Rename to "AudioManager"
   - Position: (0, 0, 0)

2. **Add AudioManager Script**
   - Select "AudioManager"
   - Click **Add Component**
   - Search for and add **AudioManager**

3. **Add Audio Sources** (Optional - script auto-creates them)
   - The script automatically creates AudioSource components
   - But you can manually add them if preferred:
     - Add Component → **Audio Source** (for music)
     - Add Component → **Audio Source** (for SFX)

### Step 2: Import Audio Files

1. **Create Audio Folders**
   - In Project window: `Assets/Audio/Music/`
   - In Project window: `Assets/Audio/SFX/`

2. **Import Music Clips**
   - Import your music files into `Assets/Audio/Music/`
   - You need:
     - **Normal World Music** - Background music for normal gameplay
     - **Blink World Music** - Background music during blink state

3. **Import SFX Clips**
   - Import your sound effect files into `Assets/Audio/SFX/`
   - You need:
     - **Blink Enter SFX** - Sound when entering blink
     - **Blink Exit SFX** - Sound when exiting blink
     - **Jump SFX** - Sound when jumping
     - **Death SFX** - Sound when player dies
     - **Checkpoint SFX** - Sound when reaching checkpoint

4. **Configure Audio Import Settings** (Recommended)
   - Select each audio file
   - In Inspector:
     - **Music files**:
       - Load Type: **Streaming** (for large files)
       - Compression Format: **Vorbis**
     - **SFX files**:
       - Load Type: **Decompress On Load** (for small files)
       - Compression Format: **PCM** or **ADPCM**

### Step 3: Assign Audio Clips to AudioManager

1. **Select AudioManager GameObject**

2. **Music Sources** (if not auto-created)
   - **Music Source**: Drag the first Audio Source component
   - **SFX Source**: Drag the second Audio Source component

3. **Music Clips**
   - **Normal World Music**: Drag your normal world music clip
   - **Blink World Music**: Drag your blink world music clip

4. **SFX Clips**
   - **Blink Enter SFX**: Drag your blink enter sound
   - **Blink Exit SFX**: Drag your blink exit sound
   - **Jump SFX**: Drag your jump sound
   - **Death SFX**: Drag your death sound
   - **Checkpoint SFX**: Drag your checkpoint sound

5. **Music Settings**
   - **Music Fade Duration**: 1.0 (seconds for cross-fade)
   - **Normal Music Volume**: 0.5 (adjust to taste)
   - **Blink Music Volume**: 0.5 (adjust to taste)

6. **SFX Settings**
   - **SFX Volume**: 0.7 (adjust to taste)

### Step 4: Configure Audio Sources

1. **Music Source Settings** (if manually created)
   - Select the AudioSource used for music
   - **Loop**: ✓ Enabled
   - **Play On Awake**: ✗ Disabled
   - **Volume**: 0.5
   - **Priority**: 128

2. **SFX Source Settings** (if manually created)
   - Select the AudioSource used for SFX
   - **Loop**: ✗ Disabled
   - **Play On Awake**: ✗ Disabled
   - **Volume**: 0.7
   - **Priority**: 128

### Step 5: Test Audio System

1. **Play the game**
2. Normal world music should start playing
3. Press Blink (Space) - should hear:
   - Blink enter SFX
   - Music cross-fades to blink world music
4. Exit blink - should hear:
   - Blink exit SFX
   - Music cross-fades back to normal world music

---

## Optional: Integrate Audio with Existing Scripts

### Add Jump Sound to PlayerMovement

1. Open `Assets/Scripts/PlayerMovement.cs`
2. In the `OnJump()` method, after the jump is executed, add:
   ```csharp
   // Play jump sound
   if (AudioManager.Instance != null)
   {
       AudioManager.Instance.PlayJumpSFX();
   }
   ```

### Add Death Sound to PlayerHealth

1. Open `Assets/Scripts/Player/PlayerHealth.cs`
2. In the `Die()` method, add:
   ```csharp
   // Play death sound
   if (AudioManager.Instance != null)
   {
       AudioManager.Instance.PlayDeathSFX();
   }
   ```

### Add Checkpoint Sound to CheckpointTrigger

1. Open `Assets/Scripts/Triggers/CheckpointTrigger.cs`
2. In the checkpoint activation code, add:
   ```csharp
   // Play checkpoint sound
   if (AudioManager.Instance != null)
   {
       AudioManager.Instance.PlayCheckpointSFX();
   }
   ```

---

## Troubleshooting

### HUD Not Showing
- Check that Canvas is enabled
- Verify Camera has "HUD Canvas" in its render layers
- Check that HUDManager references are assigned

### Health Hearts Not Updating
- Verify PlayerHealth component has the OnHealthChanged event
- Check that HUDManager has reference to PlayerHealth
- Look for errors in Console

### Audio Not Playing
- Check that AudioManager GameObject exists in scene
- Verify all audio clips are assigned
- Check AudioListener exists (should be on Main Camera)
- Verify WorldStateManager exists and AudioManager subscribed to events

### Music Not Cross-Fading
- Ensure Music Source has Loop enabled
- Check Music Fade Duration is > 0
- Verify both music clips are assigned

### SFX Not Playing
- Check SFX Volume is > 0
- Verify SFX clips are assigned
- Ensure only one AudioManager exists (check for duplicates)

---

## Testing Checklist

### Phase 4 (UI/HUD)
- [ ] Health hearts display correctly at game start
- [ ] Hearts update when taking damage
- [ ] Hearts restore when respawning
- [ ] Blink count shows current/max blinks
- [ ] Blink count updates when using blink
- [ ] Blink count color changes when low
- [ ] Coffee blink indicator shows when active
- [ ] Coffee blink indicator hides when inactive

### Phase 5 (Audio)
- [ ] Normal world music plays at game start
- [ ] Music cross-fades smoothly when entering blink
- [ ] Music cross-fades smoothly when exiting blink
- [ ] Blink enter SFX plays when entering blink
- [ ] Blink exit SFX plays when exiting blink
- [ ] Jump SFX plays when jumping (if integrated)
- [ ] Death SFX plays when dying (if integrated)
- [ ] Checkpoint SFX plays at checkpoint (if integrated)

---

## Asset Recommendations

### UI Assets
- **Heart Sprites**: 64x64 pixels, transparent background
  - Full heart: Solid red/pink heart
  - Empty heart: Heart outline only
- **Font**: Bold, readable font for blink counter
- **Colors**: High contrast for visibility

### Audio Assets
- **Music**:
  - Normal: Upbeat, calm office/corporate music
  - Blink: Darker, more intense version or different key
  - Loop seamlessly (same tempo and key recommended)
- **SFX**:
  - Blink enter: Whoosh or phase-in sound (0.3-0.5s)
  - Blink exit: Reverse whoosh or phase-out (0.3-0.5s)
  - Jump: Short, punchy sound (0.1-0.2s)
  - Death: Game over sound (0.5-1.0s)
  - Checkpoint: Success chime (0.3-0.5s)

---

## Next Steps

After setup is complete:
1. Adjust volumes to taste
2. Add more heart sprites if max health > 3
3. Consider adding health bar as alternative to hearts
4. Add screen shake or visual effects on damage
5. Implement music layers that add/remove during blink
6. Add ambient sounds for different rooms
7. Create sound variations to prevent repetition

For questions or issues, check the Console for error messages and verify all references are assigned in the Inspector.
