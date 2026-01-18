# Unity Editor Setup Guide - Phase 3 Implementation

This document provides step-by-step instructions for setting up the blink mechanic system in the Unity Editor through the end of Phase 3.

---

## Prerequisites

- Unity project is open
- All Phase 3 scripts are in the `Assets/Scripts/` folder structure
- Player prefab exists with PlayerMovement and PlayerHealth components
- Enemy prefab exists with EnemyController component

---

## Table of Contents

1. [Scene Setup](#1-scene-setup)
2. [Layer Configuration](#2-layer-configuration)
3. [Manager Setup](#3-manager-setup)
4. [Player Configuration](#4-player-configuration)
5. [Enemy Configuration](#5-enemy-configuration)
6. [Camera Configuration](#6-camera-configuration)
7. [Platform Setup - BlinkObject](#7-platform-setup---blinkobject)
8. [Platform Setup - CrumblingPlatform](#8-platform-setup---crumblingplatform)
9. [Platform Setup - BlinkTrampoline](#9-platform-setup---blinktrampoline)
10. [Platform Setup - MovingPlatform](#10-platform-setup---movingplatform)
11. [Checkpoint Setup](#11-checkpoint-setup)
12. [Room Trigger Setup](#12-room-trigger-setup)
13. [Testing Checklist](#13-testing-checklist)

---

## 1. Scene Setup

### Create Scene Hierarchy

Open your main game scene and ensure you have these root objects:

```
Scene
├── Managers (Empty GameObject)
├── Player
├── Main Camera
├── Platforms (Empty GameObject - organizer)
├── Enemies (Empty GameObject - organizer)
├── Checkpoints (Empty GameObject - organizer)
└── Rooms (Empty GameObject - organizer)
```

---

## 2. Layer Configuration

### Create Collision Layers

1. Go to **Edit > Project Settings > Tags and Layers**
2. Add the following layers:
   - Layer 6: `NormalSolid` (platforms visible/solid in Normal world)
   - Layer 7: `BlinkSolid` (platforms visible/solid in Blink world)
   - Layer 8: `Player`
   - Layer 9: `Enemy`

### Configure Physics Collision Matrix

1. Go to **Edit > Project Settings > Physics 2D**
2. Scroll to **Layer Collision Matrix**
3. Configure collisions:
   - **Player** collides with: `NormalSolid`, `BlinkSolid`, `Default`
   - **Player** does NOT collide with: `Enemy`
   - **Enemy** collides with: `NormalSolid`, `Default`
   - **Enemy** does NOT collide with: `Player`, `BlinkSolid`
   - **NormalSolid** collides with: `Player`, `Enemy`, `Default`
   - **BlinkSolid** collides with: `Player`, `Default`

---

## 3. Manager Setup

### Create WorldStateManager

1. Select the `Managers` GameObject
2. Right-click in Hierarchy > **Create Empty Child**
3. Rename to `WorldStateManager`
4. **Add Component** > `WorldStateManager` script
5. Configure in Inspector:
   - **Blink Duration**: `1.5` (seconds)
   - **Blink Cooldown**: `0.5` (seconds)
   - **Coffee Blinks**: `3`
   - **Normal Solid Layer**: `NormalSolid`
   - **Blink Solid Layer**: `BlinkSolid`
   - **Main Camera**: Drag Main Camera from Hierarchy
   - **Enable Color Inversion**: ✓ (checked)
   - **Normal World Tint**: White (255, 255, 255, 255)
   - **Blink World Tint**: Light Blue (179, 179, 255, 255)
   - **Enable Vignette**: ✓ (checked)
   - **Vignette Intensity**: `0.3`
   - **Enable Screen Shake**: ✓ (checked)
   - **Shake Intensity**: `0.15`
   - **Shake Duration**: `0.2`

### Create CheckpointManager

1. Select the `Managers` GameObject
2. Right-click in Hierarchy > **Create Empty Child**
3. Rename to `CheckpointManager`
4. **Add Component** > `CheckpointManager` script
5. Configure in Inspector:
   - **Player**: Drag Player GameObject from Hierarchy
   - **Player Health**: Should auto-populate from Player

---

## 4. Player Configuration

### Update Player GameObject

1. Select **Player** in Hierarchy
2. Set **Layer** to `Player`
3. Verify components:
   - **Transform**
   - **Sprite Renderer**
   - **Rigidbody2D**:
     - Body Type: `Dynamic`
     - Gravity Scale: `1`
     - Freeze Rotation Z: ✓
   - **Collider2D** (BoxCollider2D or CapsuleCollider2D)
   - **PlayerMovement** script:
     - **Move Speed**: `5`
     - **Jump Force**: `10`
     - **Ground Layer**: Set to `NormalSolid`, `BlinkSolid`, `Default`
   - **PlayerHealth** script:
     - **Max Health**: `3`
     - **Current Health**: `3`
   - **PhaseTransitionValidator** script:
     - Should be present

### Configure Input Actions

1. Select `Assets/InputSystem_Actions.inputactions`
2. Verify actions exist:
   - **Move** (WASD/Arrow Keys)
   - **Jump** (Space)
   - **Blink** (E key)

---

## 5. Enemy Configuration

### Setup Enemy GameObject

1. Select **Enemy** prefab or instance
2. Set **Layer** to `Enemy`
3. **Add two Collider2D components**:
   - **First Collider** (BoxCollider2D):
     - Is Trigger: ✗ (unchecked) - This is the physics collider
     - Size: Match enemy sprite
   - **Second Collider** (BoxCollider2D):
     - Is Trigger: ✓ (checked) - This is the trigger collider
     - Size: Slightly larger than first collider
4. **Rigidbody2D**:
   - Body Type: `Dynamic` (will change at runtime)
   - Gravity Scale: `1`
   - Freeze Rotation Z: ✓
5. **EnemyController** script:
   - **Chase Speed**: `3`
   - **Detection Range**: `10`
   - **Physics Collider**: Drag first (non-trigger) collider
   - **Trigger Collider**: Drag second (trigger) collider
6. **Sprite Renderer**:
   - Sprite: Enemy sprite
   - Color: Black (0, 0, 0, 255) for Normal state

### Register Enemies

1. Create empty GameObject under `Enemies` folder: `EnemySpawner`
2. **Add Component** > `EnemySpawner` script
3. This will auto-register all enemies in the scene

---

## 6. Camera Configuration

### Update Main Camera

1. Select **Main Camera** in Hierarchy
2. Verify components:
   - **Camera**:
     - Projection: `Orthographic`
     - Size: `5`
     - Background: Black (0, 0, 0, 255)
   - **CameraController** script (renamed from CameraFollow):
     - **Target**: Drag Player from Hierarchy
     - **Follow Speed**: `2`
     - **Fixed X**: (will auto-set at runtime)
     - **Offset Z**: `-10`
     - **Lock X**: ✓ (if you want 2D platformer camera)

---

## 7. Platform Setup - BlinkObject

BlinkObject is used for platforms that toggle visibility between worlds.

### Create Normal-Only Platform

1. Right-click `Platforms` > **2D Object > Sprite > Square**
2. Rename to `NormalPlatform`
3. Set **Layer** to `NormalSolid`
4. **Add Component** > `BlinkObject` script
5. Configure:
   - **Visibility Mode**: `NormalOnly`
   - **Disable On Hide**: ✗ (for performance, just hide renderer)
6. **Add Component** > `BoxCollider2D`
7. Set sprite color to white or a distinct color

### Create Blink-Only Platform

1. Duplicate `NormalPlatform` (Ctrl+D)
2. Rename to `BlinkPlatform`
3. Set **Layer** to `BlinkSolid`
4. **BlinkObject** settings:
   - **Visibility Mode**: `BlinkOnly`
5. Set sprite color to blue or purple for distinction

### Create Always-Visible Platform

1. Create another platform
2. Set **Layer** to `Default`
3. **BlinkObject** settings:
   - **Visibility Mode**: `Always`
4. This platform is always solid, regardless of world state

---

## 8. Platform Setup - CrumblingPlatform

Platforms that crumble when stepped on and reform during blink.

### Create Crumbling Platform

1. Right-click `Platforms` > **2D Object > Sprite > Square**
2. Rename to `CrumblingPlatform`
3. Set **Layer** to `NormalSolid` or `BlinkSolid`
4. **Add Component** > `CrumblingPlatform` script
5. Configure:
   - **Crumble Delay**: `0.5` (seconds before crumbling)
   - **One Time Use**: ✗ (reforms every blink)
   - **Stable Color**: Light Gray (200, 200, 200, 255)
   - **Warning Color**: Red (255, 100, 100, 255)
   - **Crumbled Color**: Dark Gray (100, 100, 100, 255)
6. **Add Component** > `BoxCollider2D`
7. **Add Component** > `Rigidbody2D`:
   - Body Type: `Static`

---

## 9. Platform Setup - BlinkTrampoline

Bounce pads that only work in the blink world.

### Create Trampoline

1. Right-click `Platforms` > **2D Object > Sprite > Square**
2. Rename to `BlinkTrampoline`
3. Set **Layer** to `BlinkSolid`
4. **Add Component** > `BlinkTrampoline` script
5. Configure:
   - **Bounce Force**: `15` (adjust based on gravity)
   - **Inactive Color**: Gray (150, 150, 150, 255)
   - **Active Color**: Orange (255, 165, 0, 255)
6. **Add Component** > `BoxCollider2D`:
   - Is Trigger: ✓ (checked)
7. Scale Y to be shorter (like `0.5`) to look like a platform

---

## 10. Platform Setup - MovingPlatform

Platforms that shift position when blinking.

### Create Moving Platform

1. Right-click `Platforms` > **2D Object > Sprite > Square**
2. Rename to `MovingPlatform`
3. Set **Layer** to `NormalSolid`
4. **Add Component** > `MovingPlatform` script
5. Configure:
   - **Movement Type**: `Smooth` or `Instant`
   - **Normal Position**: `(0, 0, 0)` (relative to GameObject)
   - **Blink Offset**: `(5, 0, 0)` (moves 5 units right in blink)
   - **Transition Speed**: `5` (only for smooth movement)
6. **Add Component** > `BoxCollider2D`
7. **Add Component** > `Rigidbody2D`:
   - Body Type: `Kinematic`

**Note**: In Scene view, you'll see gizmos showing both normal (blue) and blink (red) positions.

---

## 11. Checkpoint Setup

### Create Checkpoint Trigger

1. Right-click `Checkpoints` > **Create Empty**
2. Rename to `Checkpoint_01`
3. Position at desired checkpoint location
4. **Add Component** > `CheckpointTrigger` script
5. Configure:
   - **One Time Activation**: ✓ (checked)
   - **Spawn Offset**: `(0, 0, 0)` (or slight Y offset like `0.5`)
6. **Add Component** > `BoxCollider2D`:
   - Is Trigger: ✓ (checked)
   - Size: `(2, 2)` or appropriate size
7. **(Optional)** Add child GameObject with SpriteRenderer for visual indicator:
   - Create child: `Visual`
   - Add Sprite Renderer with flag/marker sprite
   - Drag into **Visual Indicator** field in CheckpointTrigger
   - Set colors:
     - **Inactive Color**: Gray
     - **Active Color**: Green
8. **(Optional)** Add activation sound:
   - Drag AudioClip into **Activation Sound** field

---

## 12. Room Trigger Setup

Room triggers create smooth camera transitions between areas.

### Create Room Trigger

1. Right-click `Rooms` > **Create Empty**
2. Rename to `Room_01_Trigger`
3. Position at room entrance
4. **Add Component** > `RoomTrigger` script
5. Configure:
   - **Room Name**: `Room 1`
   - **Camera Target Position**: Position where camera should center (e.g., `(10, 0, 0)`)
   - **Camera Size**: `5` (orthographic size for this room)
   - **Transition Speed**: `3`
   - **Lock Player During Transition**: ✗ (optional)
   - **Use Room Bounds**: ✓ (optional, for advanced setups)
   - **Room Bounds**: Configure if using bounds
6. **Add Component** > `BoxCollider2D`:
   - Is Trigger: ✓ (checked)
   - Size: Large enough to detect player entry (e.g., `(3, 10)`)

**Note**: In Scene view, you'll see:
- Yellow wireframe: Trigger area
- Cyan sphere: Camera target position
- Green wireframe: Room bounds (if enabled)

---

## 13. Testing Checklist

### Phase 1 & 2 - Core Systems

- [ ] **Player Movement**:
  - [ ] WASD/Arrow keys move player left/right
  - [ ] Space bar makes player jump
  - [ ] Player has gravity and falls naturally

- [ ] **Blink Mechanic**:
  - [ ] Press E to enter blink state
  - [ ] Gravity reverses (player flips upside down)
  - [ ] After ~1.5 seconds, automatically returns to normal
  - [ ] Cannot blink again immediately (cooldown works)
  - [ ] Screen shake occurs on transition
  - [ ] Visual tint changes (slight blue in blink)

- [ ] **Enemy Behavior**:
  - [ ] Enemy is idle (black box, not moving) in Normal world
  - [ ] Player can pass through enemy in Normal world
  - [ ] When entering Blink, enemy turns gray and chases player
  - [ ] Enemy kills player on contact during Blink
  - [ ] When exiting Blink, enemy turns white and stops chasing
  - [ ] Enemy affected by gravity in Normal (stands on platforms)
  - [ ] Enemy does NOT fall through platforms in Normal
  - [ ] Enemy phases through platforms during Blink (chase)

- [ ] **Death & Respawn**:
  - [ ] Player dies when enemy touches them during Blink
  - [ ] Player respawns at last checkpoint after 0.5s delay
  - [ ] Enemy resets to spawn position after player death
  - [ ] World returns to Normal state after respawn

### Phase 3 - Enhanced Mechanics

- [ ] **BlinkObject Platforms**:
  - [ ] Normal-only platforms visible/solid in Normal, invisible in Blink
  - [ ] Blink-only platforms invisible in Normal, visible/solid in Blink
  - [ ] Always-visible platforms work in both states
  - [ ] Player falls through invisible platforms

- [ ] **CrumblingPlatform**:
  - [ ] Platform turns red when player steps on it
  - [ ] Platform crumbles after delay (disappears)
  - [ ] Platform reforms when player blinks
  - [ ] (If one-time use) Platform doesn't reform after first use

- [ ] **BlinkTrampoline**:
  - [ ] Trampoline is gray and inactive in Normal world
  - [ ] Player doesn't bounce in Normal world
  - [ ] Trampoline turns orange in Blink world
  - [ ] Player bounces high when touching it in Blink
  - [ ] Bounce direction adapts to gravity (up when normal, down when reversed)

- [ ] **MovingPlatform**:
  - [ ] Platform at position A in Normal world
  - [ ] When entering Blink, platform moves to position B
  - [ ] Smooth movement if set to Smooth type
  - [ ] Instant teleport if set to Instant type
  - [ ] Returns to position A when exiting Blink

- [ ] **Checkpoint System**:
  - [ ] Player triggers checkpoint on first entry
  - [ ] Visual indicator changes color (gray → green)
  - [ ] (Optional) Sound plays on activation
  - [ ] After death, player spawns at activated checkpoint
  - [ ] Checkpoint doesn't re-trigger after activation

- [ ] **Camera System**:
  - [ ] Camera smoothly follows player vertically
  - [ ] Camera X is locked (if lockX enabled)
  - [ ] When entering RoomTrigger, camera smoothly transitions
  - [ ] Camera orthographic size adjusts during room transition
  - [ ] Camera resumes following player after room transition

### Visual Effects

- [ ] **Screen Effects**:
  - [ ] Screen shakes when entering blink
  - [ ] Slight shake when exiting blink
  - [ ] Color tint changes (white → blue-ish)
  - [ ] Vignette effect during blink
  - [ ] All effects can be toggled in WorldStateManager

### Performance

- [ ] No stuttering during blink transitions
- [ ] No lag when many BlinkObjects are in scene
- [ ] Camera transitions are smooth
- [ ] Enemy behavior is consistent

---

## Common Issues & Solutions

### Issue: Player not responding to input
**Solution**:
- Check InputSystem_Actions.inputactions exists
- Verify InputSystem_Actions.cs is generated
- Ensure PlayerMovement script references the correct input actions

### Issue: Enemy falls through platforms
**Solution**:
- Verify enemy has TWO colliders (physics + trigger)
- Check Physics Collider is NOT a trigger
- Ensure enemy is on `Enemy` layer
- Verify Physics2D collision matrix allows Enemy × NormalSolid

### Issue: BlinkObject not toggling visibility
**Solution**:
- Check WorldStateManager singleton exists in scene
- Verify BlinkObject subscribes to WorldStateManager events
- Ensure layer matches visibility mode (NormalSolid for NormalOnly, etc.)

### Issue: Camera not transitioning between rooms
**Solution**:
- Verify CameraController component exists on Main Camera (not CameraFollow)
- Check RoomTrigger is a trigger collider
- Ensure player has "Player" tag

### Issue: Checkpoint not activating
**Solution**:
- Check CheckpointManager singleton exists
- Verify checkpoint collider is a trigger
- Ensure player has "Player" tag

---

## Next Steps (Post-Phase 3)

After Phase 3 is complete and tested:

1. **Create Prefabs**: Save configured platforms, enemies, checkpoints as prefabs
2. **Level Design**: Build levels using the platform components
3. **Audio**: Add sound effects for blink, death, checkpoint, etc.
4. **UI**: Create HUD showing blink count, coffee blinks, health
5. **Particle Effects**: Add VFX for transitions, death, checkpoint activation
6. **Polish**: Tune movement values, blink duration, enemy speed

---

## Script Reference

All scripts are located in `Assets/Scripts/`:

```
Scripts/
├── Managers/
│   ├── WorldStateManager.cs
│   └── CheckpointManager.cs
├── Player/
│   ├── PlayerMovement.cs
│   └── PlayerHealth.cs
├── AI/
│   ├── EnemyController.cs
│   └── EnemySpawner.cs
├── Levels/
│   ├── BlinkObject.cs
│   ├── BlinkCollider.cs
│   ├── CrumblingPlatform.cs
│   ├── BlinkTrampoline.cs
│   ├── MovingPlatform.cs
│   ├── CheckpointTrigger.cs
│   ├── RoomTrigger.cs
│   └── PhaseTransitionValidator.cs
└── CameraController.cs (formerly CameraFollow.cs)
```

---

## Summary

Phase 3 implementation is complete when:

1. ✓ All manager GameObjects are configured in Managers folder
2. ✓ Player has proper input, movement, health, and phase validation
3. ✓ Enemies have dual-collider setup and proper state machine
4. ✓ Camera supports room transitions and smooth following
5. ✓ Multiple platform types (BlinkObject, Crumbling, Trampoline, Moving) are tested
6. ✓ Checkpoint system activates and respawns player correctly
7. ✓ Visual effects (shake, tint, vignette) enhance the blink experience
8. ✓ All testing checklist items pass

The layer-based blink system allows for flexible level design without scene-switching overhead, enabling artists to place objects in both Normal and Blink worlds easily.
