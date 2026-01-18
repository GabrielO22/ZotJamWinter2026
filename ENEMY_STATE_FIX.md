# Enemy Normal State Fix

## Issues Fixed

### Issue 1: Enemy Frozen in Normal State (No Gravity) ❌ → ✅
**Problem:** After exiting blink, enemies were frozen in place with no gravity applying

**Root Cause:** Enemy Rigidbody2D was set to Kinematic in normal state

**Fix:**
- Line 45-50: Initialize as Dynamic with gravity scale = 1
- Line 150-151: Return to Dynamic with gravity when exiting blink
- Line 182-183: Return to Dynamic with gravity when resetting to spawn

### Issue 2: Enemy Kills Player in Normal State ❌ → ✅
**Problem:** Enemy killed player on contact even when idle (white color, normal state)

**Root Cause:** Collision detection didn't check enemy state before dealing damage

**Fix:**
- Line 54-56: Made enemy collider a trigger (player can pass through)
- Line 212-233: Changed from OnCollisionEnter2D to OnTriggerEnter2D
- Line 215: Only damage player if `currentState == EnemyState.Chasing`
- Line 232: Added comment explaining harmless pass-through in Idle state

### Issue 3: Player Couldn't Pass Through Enemy ❌ → ✅
**Problem:** Player collided with enemy in normal state (like a wall)

**Root Cause:** Enemy collider was solid (not a trigger)

**Fix:**
- Line 36: Added enemyCollider reference
- Line 54-56: Set `enemyCollider.isTrigger = true`
- Player now passes through enemies when they're idle (normal state)

---

## Technical Changes

### EnemyController.cs

**Added Reference:**
```csharp
[SerializeField] private Collider2D enemyCollider; // Line 14
```

**Awake() - Initialize as Physics-Affected:**
```csharp
// Line 45-50: Dynamic body with gravity
rb.bodyType = RigidbodyType2D.Dynamic;
rb.gravityScale = 1f; // Enable gravity
rb.constraints = RigidbodyConstraints2D.FreezeRotation;

// Line 54-56: Make trigger so player can pass through
enemyCollider.isTrigger = true;
```

**HandleExitBlink() - Restore Physics:**
```csharp
// Line 150-151: Return to Dynamic with gravity
rb.bodyType = RigidbodyType2D.Dynamic;
rb.gravityScale = 1f;
```

**ResetToSpawn() - Restore Physics:**
```csharp
// Line 182-183: Return to Dynamic with gravity
rb.bodyType = RigidbodyType2D.Dynamic;
rb.gravityScale = 1f;
```

**Collision Detection Changed:**
```csharp
// OLD: OnCollisionEnter2D (solid collision)
// NEW: OnTriggerEnter2D (trigger collision)

void OnTriggerEnter2D(Collider2D collision)
{
    // Only damage if Chasing (blink state)
    if (currentState == EnemyState.Chasing)
    {
        // Kill player
    }
    // Idle state: player passes through harmlessly
}
```

---

## Expected Behavior

### Normal State (Idle):
✅ Enemy color: White
✅ Enemy affected by gravity (falls if not on ground)
✅ Player can walk through enemy (no collision)
✅ Player takes NO damage from enemy
✅ Enemy doesn't chase player

### Blink State (Chasing):
✅ Enemy color: Dark gray (0.2, 0.2, 0.2, 0.7)
✅ Enemy NOT affected by gravity (floats/flies)
✅ Enemy chases player
✅ Player takes damage on contact
✅ Enemy phases through obstacles

### Transition: Normal → Blink (Press E):
1. Enemy changes color to dark gray
2. Enemy switches from Dynamic to Kinematic (gravity off)
3. Enemy starts chasing player
4. Enemy can now kill player on contact

### Transition: Blink → Normal (After 1 second):
1. Enemy changes color to white
2. Enemy switches from Kinematic to Dynamic (gravity on)
3. Enemy stops chasing
4. Enemy can no longer kill player
5. Enemy falls if in mid-air

---

## Unity Setup Required

### Enemy GameObject Inspector:

**Transform:**
- Position: Place enemy on a platform

**Rigidbody2D:**
- Body Type: Dynamic (set automatically by script)
- Gravity Scale: 1 (set automatically by script)
- Constraints: Freeze Rotation Z (set automatically by script)

**Collider2D (BoxCollider2D or similar):**
- Is Trigger: ✅ **CHECKED** (set automatically by script, but verify)
- Size: Adjust to match enemy sprite

**Enemy Controller Component:**
- Player: Leave empty for auto-find or assign manually
- Rb: Leave empty (auto-assigned)
- Sprite Renderer: Leave empty (auto-assigned)
- Enemy Collider: Leave empty (auto-assigned)
- Speed Type: Slow/Medium/Fast
- Normal Color: White (1, 1, 1, 1)
- Ghost Color: Dark Gray (0.2, 0.2, 0.2, 0.7)

**IMPORTANT:** Make sure "Is Trigger" is checked on the enemy's collider!

---

## Testing Checklist

### Test 1: Normal State Gravity
1. ✅ Place enemy in mid-air
2. ✅ Play scene
3. ✅ Enemy should fall to ground
4. ✅ Enemy should stop on platform

### Test 2: Player Pass-Through
1. ✅ Place enemy on ground
2. ✅ Play scene
3. ✅ Walk player through enemy
4. ✅ Player should pass through without collision
5. ✅ Player should NOT die

### Test 3: Blink State Damage
1. ✅ Stand near enemy
2. ✅ Press E to blink
3. ✅ Enemy turns dark gray and chases
4. ✅ Let enemy touch player
5. ✅ Player should die
6. ✅ Console shows: "[EnemyName] killed player during blink state"

### Test 4: Normal State Immunity
1. ✅ Wait for blink to end (1 second)
2. ✅ Enemy turns white and stops chasing
3. ✅ Walk player into enemy
4. ✅ Player should pass through
5. ✅ Player should NOT die

### Test 5: Physics Consistency
1. ✅ Place enemy on platform
2. ✅ Press E to blink
3. ✅ Enemy floats/flies (no gravity)
4. ✅ Wait for blink to end
5. ✅ Enemy falls back to ground (gravity restored)

---

## Common Issues

### "Enemy still kills player in normal state"
**Check:**
- Enemy collider "Is Trigger" is checked
- OnTriggerEnter2D checks `currentState == EnemyState.Chasing`
- Enemy is white color (not gray) when player touches it

### "Enemy doesn't fall"
**Check:**
- Rigidbody2D Body Type is Dynamic (not Kinematic)
- Rigidbody2D Gravity Scale is 1 (not 0)
- Enemy has a platform beneath it with a collider

### "Player bounces off enemy instead of passing through"
**Check:**
- Enemy Collider2D "Is Trigger" is **CHECKED**
- Player doesn't have "Collide With" enemy layer enabled

### "Enemy flies away after collision"
**Should not happen anymore** - enemy is Kinematic during blink (no physics forces)

---

## Summary of States

| State | Body Type | Gravity | Trigger? | Damages Player? | Chases? | Color |
|-------|-----------|---------|----------|-----------------|---------|-------|
| **Idle (Normal)** | Dynamic | Yes (1.0) | Yes | ❌ No | ❌ No | White |
| **Chasing (Blink)** | Kinematic | No (ignored) | Yes | ✅ Yes | ✅ Yes | Dark Gray |

---

## Debug Console Messages

When working correctly, you should see:

**On Scene Load:**
```
[EnemyName] subscribed to WorldStateManager
```

**On Blink (Press E):**
```
Entering Blink state (Count: 1)
[EnemyName] transformed into ghost - chasing player
```

**On Blink End (1 second later):**
```
Exiting Blink state - Returning to Normal
[EnemyName] returned to normal form
```

**On Enemy Touch Player (During Blink):**
```
[EnemyName] killed player during blink state
Player died!
```

**On Player Death:**
```
Player death #1. Respawning at checkpoint...
[EnemyName] reset to spawn position
Player respawned at [position]
```
