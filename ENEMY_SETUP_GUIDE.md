# Enemy GameObject Setup Guide - Unity Editor

## Overview
This guide explains how to set up enemy GameObjects in Unity that integrate with your blink system. Enemies are harmless office workers in the normal world but become dangerous ghosts during blink state.

---

## Enemy Behavior Summary

### Normal World (Non-Blink)
- **State:** Idle/Patrolling
- **Physics:** Dynamic Rigidbody2D with gravity
- **Collider:** Physics collider enabled (solid)
- **Trigger:** Disabled (can't harm player)
- **Appearance:** Normal sprite, normal color

### Blink World
- **State:** Chasing
- **Physics:** Kinematic (no gravity, phases through walls)
- **Collider:** Physics collider disabled (ghost mode)
- **Trigger:** Enabled (can detect and harm player)
- **Appearance:** Ghost sprite, semi-transparent dark color

### Coffee Power-Up Active
- **Behavior:** Enemy stays in ghost form but doesn't chase
- **Safe:** Player is invulnerable to enemy contact

---

## Unity Editor Setup - Step by Step

### Step 1: Create Enemy GameObject

1. **Create Empty GameObject**
   - Hierarchy → Right-click → Create Empty
   - Name it: "Enemy_OfficWorker" (or similar)
   - Position it where you want the enemy to spawn

2. **Tag the GameObject** (Optional but Recommended)
   - Select the enemy GameObject
   - Inspector → Tag → Add Tag... → "Enemy"
   - Select enemy again and set Tag to "Enemy"

---

### Step 2: Add Required Components

Add these components to your enemy GameObject in this order:

#### A. Rigidbody2D
```
Component → Physics 2D → Rigidbody 2D
```

**Settings:**
- Body Type: `Dynamic`
- Simulated: ✓ (checked)
- Use Auto Mass: ✓ (checked)
- Mass: 1
- Linear Drag: 0
- Angular Drag: 0.05
- Gravity Scale: 1
- Collision Detection: `Discrete`
- Sleeping Mode: `Start Awake`
- Interpolate: `None`
- Constraints:
  - Freeze Position: None
  - Freeze Rotation: ✓ Z (checked)

**Why:** The script will manage body type changes (Dynamic ↔ Kinematic) during blink transitions.

---

#### B. BoxCollider2D #1 (Physics Collider)
```
Component → Physics 2D → Box Collider 2D
```

**Settings:**
- Is Trigger: ☐ (unchecked) - **IMPORTANT**
- Used By Effector: ☐ (unchecked)
- Auto Tiling: ☐ (unchecked)
- Offset: X: 0, Y: 0
- Size: X: 1, Y: 1.5 (adjust to fit sprite)

**Purpose:** Makes enemy solid in normal world (collides with platforms)

---

#### C. BoxCollider2D #2 (Trigger Collider)
```
Component → Physics 2D → Box Collider 2D
```

**Settings:**
- Is Trigger: ✓ (checked) - **IMPORTANT**
- Used By Effector: ☐ (unchecked)
- Auto Tiling: ☐ (unchecked)
- Offset: X: 0, Y: 0
- Size: X: 1.2, Y: 1.7 (slightly larger than physics collider)

**Purpose:** Detects player collision during blink state (for killing player)

**Tip:** Make the trigger collider slightly larger than the physics collider for better detection.

---

#### D. Sprite Renderer
```
Component → Rendering → Sprite Renderer
```

**Settings:**
- Sprite: Your normal enemy sprite (office worker)
- Color: White (255, 255, 255, 255)
- Flip: None
- Material: `Sprites-Default`
- Sorting Layer: `Default` or your character layer
- Order in Layer: 0 (adjust as needed)

---

#### E. EnemyController Script
```
Component → Scripts → Enemy Controller
```

Now configure the script in the Inspector:

---

### Step 3: Configure EnemyController Script

#### References Section
```
Player: (Leave empty - auto-finds)
Rb: (Leave empty - auto-finds)
Sprite Renderer: (Leave empty - auto-finds)
Physics Collider: (Leave empty - auto-assigns to first BoxCollider2D)
Trigger Collider: (Leave empty - auto-assigns to second BoxCollider2D)
```

**Auto-Detection:** The script automatically finds these components. You can manually assign them if auto-detection fails.

---

#### Enemy Settings Section
```
Speed Type: Medium (dropdown)
  Options: Slow, Medium, Fast

Slow Speed: 0.5
Medium Speed: 1.0
Fast Speed: 2.0
```

**Speed Type Guide:**
- **Slow:** Easy to avoid, good for tutorial enemies
- **Medium:** Standard difficulty
- **Fast:** Challenging, experienced players only

---

#### Visual Settings Section
```
Normal Sprite: Drag your normal enemy sprite here
Ghost Sprite: Drag your ghost/dark enemy sprite here

Normal Color: RGB(255, 255, 255, 255) - White
Ghost Color: RGB(51, 51, 51, 179) - Dark semi-transparent

Flip Sprite On Direction: ✓ (checked)
```

**Sprite Requirements:**
- Both sprites should be the same dimensions
- Ghost sprite should look ethereal/scary
- If you don't have a ghost sprite, leave empty (will use color tint only)

---

### Step 4: Verify Collider Assignment

After setting up, select your enemy and check in the Inspector:

1. **Expand EnemyController component**
2. **Check References:**
   - Physics Collider → Should show "BoxCollider2D (0)"
   - Trigger Collider → Should show "BoxCollider2D (1)"

3. **If wrong:**
   - Click the circle next to each field
   - Manually select the correct collider from the list

**Correct Setup:**
- Physics Collider = First BoxCollider2D (Is Trigger: OFF)
- Trigger Collider = Second BoxCollider2D (Is Trigger: ON)

---

### Step 5: Set Up Player Reference

The enemy needs to know where the player is to chase them.

**Option 1: Automatic (Recommended)**
- Leave Player field empty
- Make sure your player GameObject has Tag: "Player"
- Script will find it automatically in Start()

**Option 2: Manual**
- Drag your Player GameObject from Hierarchy into the "Player" field

---

### Step 6: Test Enemy Behavior

#### In Editor Play Mode

1. **Start Play Mode**
   - Enemy should fall due to gravity
   - Enemy should be idle/standing

2. **Trigger Blink** (Press E)
   - Enemy should change to ghost sprite/color
   - Enemy should start chasing player
   - Enemy should float (no gravity)
   - Enemy should phase through walls

3. **Exit Blink** (Wait for timer)
   - Enemy returns to normal sprite/color
   - Enemy stops chasing
   - Enemy affected by gravity again

4. **Test Coffee Power-Up** (If implemented)
   - Enemy becomes ghost but doesn't chase
   - Player is safe from enemy

#### Debug Visualization
- **Scene View (when enemy selected):**
  - Cyan wire sphere = spawn position
  - Red line = chase direction (when chasing)

---

## Advanced Setup

### Multiple Enemies

**Option 1: Duplicate GameObject**
1. Select enemy in Hierarchy
2. Ctrl+D (Duplicate)
3. Move to new position
4. Rename (Enemy_OfficeWorker_2, etc.)

**Option 2: Create Prefab**
1. Drag enemy from Hierarchy to Project window
2. Creates a prefab
3. Drag prefab into scene for new instances

**Prefab Benefits:**
- Changes to prefab affect all instances
- Easy to create variations (Slow, Medium, Fast)

---

### Enemy Spawner Setup

For centralized enemy management:

1. **Create Empty GameObject**
   - Hierarchy → Create Empty
   - Name: "EnemySpawner"

2. **Add EnemySpawner Script**
   - Component → Scripts → Enemy Spawner

3. **Configure:**
   ```
   Auto Find Enemies: ✓ (checked)
   ```
   OR manually assign enemies:
   ```
   Auto Find Enemies: ☐ (unchecked)
   Enemies: [Array Size]
     Element 0: Enemy_OfficeWorker_1
     Element 1: Enemy_OfficeWorker_2
     ...
   ```

**Purpose:** Automatically registers all enemies with CheckpointManager for proper respawn behavior.

---

## Collision Layers Setup (Important!)

### Recommended Layer Setup

1. **Open Layer Manager**
   - Edit → Project Settings → Tags and Layers

2. **Create Layers:**
   - Layer 8: `Player`
   - Layer 9: `Enemy`
   - Layer 10: `Ground`

3. **Assign Layers:**
   - Player GameObject → Layer: `Player`
   - All Enemy GameObjects → Layer: `Enemy`
   - All Platform GameObjects → Layer: `Ground`

4. **Configure Collision Matrix**
   - Edit → Project Settings → Physics 2D
   - Scroll to Layer Collision Matrix

**Matrix Setup:**
```
           Player  Enemy  Ground
Player       ✓      ✓      ✓
Enemy        ✓      ☐      ✓
Ground       ✓      ✓      ✓
```

**Result:**
- Player collides with everything
- Enemies collide with ground and player
- Enemies don't collide with each other

---

## Troubleshooting

### Enemy Not Chasing in Blink Mode

**Check:**
1. WorldStateManager exists in scene
2. Enemy subscribed successfully (check Console log)
3. Ghost sprite/color assigned
4. Player tagged correctly ("Player")

**Solution:**
- Open Console (Ctrl+Shift+C)
- Look for: "[EnemyName] subscribed to WorldStateManager"
- If missing, WorldStateManager not in scene

---

### Enemy Falls Through Floor

**Check:**
1. Enemy has Physics Collider (Is Trigger: OFF)
2. Floor has Collider2D component
3. Floor layer set correctly in collision matrix

**Solution:**
- Select enemy → Inspector → BoxCollider2D (first one)
- Ensure "Is Trigger" is UNCHECKED

---

### Enemy Doesn't Kill Player

**Check:**
1. Enemy has Trigger Collider (Is Trigger: ON)
2. Player has "Player" tag
3. Player has PlayerHealth component
4. Trigger collider is enabled during blink

**Solution:**
- Play mode → Trigger blink
- Select enemy → Inspector → BoxCollider2D (second one)
- "Enabled" should be checked during blink

---

### Enemy Doesn't Change Sprite

**Check:**
1. Ghost Sprite assigned in Inspector
2. SpriteRenderer component exists
3. Sprite Renderer field populated

**Solution:**
- If no ghost sprite, enemy will only change color
- Create/import ghost sprite and assign it

---

### Colliders Not Auto-Assigning

**Manual Fix:**
1. Select enemy GameObject
2. Inspector → EnemyController component
3. Click circle next to "Physics Collider"
4. Select BoxCollider2D that has "Is Trigger: OFF"
5. Click circle next to "Trigger Collider"
6. Select BoxCollider2D that has "Is Trigger: ON"

---

## Performance Tips

### For Many Enemies (10+)

1. **Use Object Pooling**
   - Create enemy pool at scene start
   - Reuse defeated enemies
   - Reduces instantiation overhead

2. **Limit Update Calls**
   - Only enemies on-screen chase player
   - Use distance checks to disable far enemies

3. **Optimize Sprite Changes**
   - Use sprite atlas for enemy sprites
   - Reduces draw calls

---

## Checklist - Quick Reference

Before marking enemy setup complete:

- [ ] GameObject created and named
- [ ] Rigidbody2D added (Dynamic, Freeze Rotation Z)
- [ ] BoxCollider2D #1 added (Is Trigger: OFF)
- [ ] BoxCollider2D #2 added (Is Trigger: ON)
- [ ] SpriteRenderer added with normal sprite
- [ ] EnemyController script added
- [ ] Normal sprite assigned
- [ ] Ghost sprite assigned (or color tint configured)
- [ ] Speed type selected
- [ ] Player reference set (or auto-find enabled)
- [ ] Colliders auto-assigned correctly
- [ ] Enemy layer assigned
- [ ] Collision matrix configured
- [ ] Tested in Play Mode
- [ ] Enemy chases during blink ✓
- [ ] Enemy returns to normal after blink ✓
- [ ] Enemy kills player on contact ✓

---

## Example Hierarchy

```
Scene
├── WorldStateManager
├── Player
│   ├── Rigidbody2D
│   ├── BoxCollider2D
│   ├── SpriteRenderer
│   ├── PlayerMovement
│   └── PlayerHealth
├── EnemySpawner
│   └── EnemySpawner Script
├── Enemies
│   ├── Enemy_OfficeWorker_1
│   │   ├── Rigidbody2D
│   │   ├── BoxCollider2D (Physics)
│   │   ├── BoxCollider2D (Trigger)
│   │   ├── SpriteRenderer
│   │   └── EnemyController
│   ├── Enemy_OfficeWorker_2
│   │   └── [Same components...]
│   └── Enemy_OfficeWorker_3
│       └── [Same components...]
└── Platforms
    ├── Ground
    └── Platform_1
```

---

## Related Files

- **Script:** `Assets/Scripts/AI/EnemyController.cs`
- **Spawner:** `Assets/Scripts/AI/EnemySpawner.cs`
- **Integration:** `Assets/Scripts/Managers/WorldStateManager.cs`
- **Player Health:** `Assets/Scripts/Player/PlayerHealth.cs`

---

## Next Steps

After setting up enemies:
1. Create enemy prefab variants (Slow, Medium, Fast)
2. Set up EnemySpawner for checkpoint integration
3. Test with multiple enemies
4. Adjust chase speeds for difficulty balance
5. Create different enemy sprites for variety

---

## Support

If you encounter issues not covered in troubleshooting:
1. Check Unity Console for errors
2. Verify all components are assigned
3. Review WorldStateManager setup
4. Check collision layer matrix
5. Test with simple scene first (one enemy, flat ground)

**Common Mistakes:**
- Forgetting to set collider "Is Trigger" correctly
- Missing "Player" tag on player GameObject
- No WorldStateManager in scene
- Collision layers not configured
