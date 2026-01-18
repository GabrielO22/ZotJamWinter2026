# Enemy Behavior Fixes

## Issues Diagnosed and Fixed

### Issue 1: Enemy Chases Player on Scene Load ❌ → ✅

**Problem:**
- Enemies were chasing the player immediately when the scene loaded
- Should only chase during blink state (when player presses E)

**Root Cause:**
- Old `Enemy_Movement.cs` script was still in the project
- This script unconditionally chases player in Update() without any state checking

**Fix:**
✅ Deleted `Assets/Scripts/AI/Enemy_Movement.cs` (obsolete script)
✅ Enemies now use `EnemyController.cs` which only chases in `EnemyState.Chasing`

---

### Issue 2: Enemy Flies Off After Player Death ❌ → ✅

**Problem:**
- When enemy kills player, enemy flies off in opposite direction of collision
- Enemies should stay in place when idle

**Root Cause:**
- Enemy Rigidbody2D was set to `Dynamic` when exiting blink state
- Dynamic mode applies physics forces (gravity, collisions, impulses)
- Collision with player applied physics impulse, launching enemy

**Fix:**
✅ Line 42-47: Initialize enemies as `Kinematic` in Awake()
✅ Line 145-149: Keep enemies as `Kinematic` when returning to idle (HandleExitBlink)
✅ Line 177-181: Keep enemies as `Kinematic` when resetting to spawn
✅ Enemies remain `Kinematic` during both idle and chasing states (no physics forces)

**Why Kinematic?**
- Kinematic bodies don't respond to physics forces
- They can still detect collisions (OnCollisionEnter2D still works)
- Perfect for enemies that use direct transform movement

---

### Issue 3: Blink Doesn't Affect Enemy State ❌ → ✅

**Problem:**
- Pressing E to blink didn't trigger enemy state changes
- Enemies didn't transform into ghosts during blink

**Root Cause:**
- WorldStateManager might not exist when enemies subscribe in OnEnable()
- Race condition: enemies enabled before WorldStateManager singleton initializes

**Fix:**
✅ Line 67-88: Added coroutine to delay subscription by one frame
✅ Added debug logging when subscription succeeds/fails
✅ Ensures WorldStateManager.Instance exists before subscribing

**How it works:**
```csharp
void OnEnable() {
    StartCoroutine(SubscribeToWorldStateManager());
}

IEnumerator SubscribeToWorldStateManager() {
    yield return null; // Wait one frame
    if (WorldStateManager.Instance != null) {
        // Subscribe to events
    }
}
```

---

## Summary of Changes

**EnemyController.cs:**
1. Line 42-47: Initialize as Kinematic in Awake()
2. Line 67-88: Delayed subscription to WorldStateManager
3. Line 145-149: Keep Kinematic when exiting blink
4. Line 177-181: Keep Kinematic when resetting to spawn

**Deleted Files:**
- `Assets/Scripts/AI/Enemy_Movement.cs` (obsolete, replaced by EnemyController)

---

## Expected Behavior After Fixes

### On Scene Load:
✅ Enemies should be idle (white color)
✅ Enemies should not move or chase player
✅ Console should show: "[EnemyName] subscribed to WorldStateManager"

### When Pressing E (Blink):
✅ Enemies transform to ghost (dark gray, semi-transparent)
✅ Enemies start chasing player
✅ Enemies phase through obstacles (Kinematic, no collisions)
✅ Console shows: "[EnemyName] transformed into ghost - chasing player"

### After 1 Second (Blink Ends):
✅ Enemies return to normal (white color)
✅ Enemies stop chasing
✅ Enemies stay in their current position (no flying off)
✅ Console shows: "[EnemyName] returned to normal form"

### When Enemy Touches Player During Blink:
✅ Player dies
✅ Player respawns at checkpoint
✅ Enemies reset to spawn positions
✅ Enemies return to idle state
✅ World returns to Normal state

---

## Troubleshooting

### "WorldStateManager not found" in Console
**Problem:** WorldStateManager singleton doesn't exist in scene
**Solution:**
1. Create an empty GameObject named "Managers"
2. Add WorldStateManager component to it
3. Ensure it's in the scene hierarchy BEFORE pressing Play

### Enemies Still Chase on Load
**Problem:** Old Enemy_Movement component still attached to enemy GameObjects
**Solution:**
1. Select each enemy GameObject in hierarchy
2. In Inspector, find "Enemy_Movement" component
3. Click the gear icon → Remove Component
4. Add "Enemy Controller" component if not present

### Enemies Don't Respond to Blink
**Check Console for:**
- "[EnemyName] subscribed to WorldStateManager" ✅ Should appear on Play
- "WorldStateManager not found!" ❌ Means WorldStateManager missing

**Check WorldStateManager:**
1. Press E to blink
2. Console should show: "Entering Blink state (Count: 1)"
3. If not, WorldStateManager isn't working

**Check Enemy Inspector:**
- Enemy Controller component exists
- Player field is assigned (or left empty for auto-find)
- Speed Type is set (Slow/Medium/Fast)

---

## Testing Checklist

1. ✅ Start game - enemies idle, white color, not moving
2. ✅ Press E - enemies turn dark, start chasing
3. ✅ Wait 1 second - enemies return to white, stop chasing, stay in place
4. ✅ Press E again - enemies chase again
5. ✅ Let enemy touch player during blink - player dies, respawns, enemies reset
6. ✅ No flying enemies after player death

---

## Notes

- All enemies now use Kinematic Rigidbody2D at all times
- Enemies don't use physics forces (gravity, velocity, impulses)
- Movement is done via direct transform manipulation
- This prevents unwanted physics interactions while keeping collision detection
