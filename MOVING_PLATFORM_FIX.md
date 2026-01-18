# MovingPlatform Not Working - Diagnosis and Fix

## Issue Report

**Symptom**: MovingPlatform component does not move a `NormalSolid` platform with `NormalOnly` visibility when entering blink state.

## Root Cause Analysis

### Primary Issue: BlinkObject Disabling GameObject

When using **MovingPlatform** + **BlinkObject** on the same GameObject:

1. **MovingPlatform** subscribes to `OnEnterBlink` event
2. **BlinkObject** with `VisibilityMode.NormalOnly` also subscribes to `OnEnterBlink`
3. When entering blink state:
   - Both components receive the event
   - **BlinkObject** determines platform should be hidden (NormalOnly → invisible in Blink)
   - If `disableOnHide = true`, **BlinkObject calls `gameObject.SetActive(false)`**
   - This triggers `OnDisable()` in **MovingPlatform**
   - **MovingPlatform** unsubscribes from events and stops all coroutines
   - Platform never moves because the GameObject is disabled

### Visual Representation

```
Blink State Entered
    ↓
┌───────────────────────────────┐
│  WorldStateManager            │
│  Fires: OnEnterBlink          │
└───────────────────────────────┘
         ↓                ↓
    ┌────────┐      ┌────────────┐
    │ Moving │      │ BlinkObject│
    │Platform│      │ (NormalOnly)│
    └────────┘      └────────────┘
         ↓                ↓
    Tries to move   Checks visibility
         ↓                ↓
         ↓          Should be hidden
         ↓                ↓
         ↓          gameObject.SetActive(false)
         ↓                ↓
         ↓           ┌────────────────┐
         ↓           │ GameObject     │
         ↓           │ DISABLED       │
         ↓           └────────────────┘
         ↓                ↓
    OnDisable() triggered
         ↓
    Unsubscribe events
         ↓
    Stop coroutines
         ↓
    ❌ Movement cancelled
```

### Secondary Issues Fixed

1. **`normalPosition == Vector3.zero` check was flawed**:
   - If platform is actually at world origin (0, 0, 0), this check incorrectly triggers
   - Now uses explicit `useStartingPositionAsNormal` boolean flag

2. **No race condition protection**:
   - Direct subscription in `OnEnable()` could fail if WorldStateManager not initialized
   - Now uses `DelayedSubscribe()` coroutine to wait one frame

3. **Silent failures**:
   - No debug logging to diagnose issues
   - Added comprehensive debug logging with `showDebugLogs` toggle

4. **No warning about BlinkObject conflict**:
   - Users wouldn't know why platform doesn't move
   - Added warning in `Awake()` when both components detected

## Solutions

### Solution 1: Don't Use BlinkObject with MovingPlatform (Recommended)

**MovingPlatform** is designed to work independently. It doesn't need **BlinkObject** because:
- Platform moves position (handles "blink behavior" itself)
- Visibility and collision are handled by layer assignment

**Setup**:
1. Remove **BlinkObject** component from moving platform
2. Set platform layer to `NormalSolid` or `BlinkSolid` based on when it should be solid
3. **MovingPlatform** will move the platform regardless of which layer it's on
4. Collision system will handle when player can/cannot stand on it

**Example**: Platform solid in Normal, phases through in Blink
- Layer: `NormalSolid`
- MovingPlatform: Moves from position A → B when blinking
- Result: Platform moves to new position AND becomes non-solid (physics layer rules)

### Solution 2: Use BlinkObject with `disableOnHide = false`

If you need **BlinkObject** for visual feedback:

1. Keep **BlinkObject** component
2. **IMPORTANT**: Set `Disable On Hide` to **FALSE** (unchecked)
3. This way:
   - BlinkObject only toggles renderer/collider
   - GameObject stays active
   - MovingPlatform continues running and can move
   - Sprite becomes invisible but platform still moves

**Limitation**: Platform will move even when invisible, which might look odd. Better to use Solution 1.

### Solution 3: Custom Visibility + Movement Component

For advanced users, create a custom component that combines both behaviors without conflicting.

## Changes Made to MovingPlatform.cs

### 1. Added Warning System

```csharp
void Awake()
{
    // ...existing code...

    // Warn if BlinkObject with disableOnHide is attached
    BlinkObject blinkObj = GetComponent<BlinkObject>();
    if (blinkObj != null)
    {
        Debug.LogWarning($"{gameObject.name}: MovingPlatform + BlinkObject detected. " +
            "If BlinkObject has 'Disable On Hide' enabled, the platform won't move when hidden. " +
            "Set 'Disable On Hide' to FALSE for proper movement.");
    }
}
```

### 2. Fixed normalPosition Initialization

```csharp
[Header("Movement Settings")]
[Tooltip("If true, uses the platform's starting position as normalPosition")]
[SerializeField] private bool useStartingPositionAsNormal = true;
[SerializeField] private Vector3 normalPosition = Vector3.zero;

void Awake()
{
    if (useStartingPositionAsNormal)
    {
        normalPosition = transform.position;
    }
}
```

### 3. Added Delayed Subscription

```csharp
void OnEnable()
{
    StartCoroutine(DelayedSubscribe());
}

private System.Collections.IEnumerator DelayedSubscribe()
{
    yield return null; // Wait one frame for WorldStateManager to initialize

    if (WorldStateManager.Instance != null)
    {
        WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
        WorldStateManager.Instance.OnExitBlink += HandleExitBlink;
        UpdatePosition(WorldStateManager.Instance.CurrentState, true);
    }
}
```

### 4. Added Comprehensive Debug Logging

```csharp
[Header("Debug")]
[SerializeField] private bool showDebugLogs = true;

private void HandleEnterBlink()
{
    if (showDebugLogs)
    {
        Debug.Log($"{gameObject.name}: HandleEnterBlink called");
    }
    UpdatePosition(WorldState.Blink, false);
}
```

### 5. Added Initialization Check

```csharp
private bool isInitialized = false;

void Awake()
{
    // ... initialization code ...
    isInitialized = true;
}

private void UpdatePosition(WorldState state, bool instant)
{
    if (!isInitialized)
    {
        Debug.LogWarning($"{gameObject.name}: UpdatePosition called before initialization!");
        return;
    }
    // ... rest of method ...
}
```

## Testing Instructions

### Test Case 1: MovingPlatform Without BlinkObject

1. Create platform GameObject
2. Add **MovingPlatform** component only (no BlinkObject)
3. Set layer to `NormalSolid`
4. Configure:
   - Use Starting Position As Normal: ✓
   - Blink Offset: (5, 0, 0)
   - Movement Type: Instant
   - Show Debug Logs: ✓
5. Play scene and press E to blink
6. **Expected**: Console shows:
   ```
   Platform: Set normalPosition to starting position (x, y, z)
   Platform: Subscribed to WorldStateManager, current state: Normal
   Platform: HandleEnterBlink called
   Platform: Instantly moved to (x+5, y, z) (state: Blink)
   ```
7. Platform should move 5 units to the right ✅

### Test Case 2: MovingPlatform + BlinkObject (disableOnHide = false)

1. Create platform GameObject
2. Add **MovingPlatform** component
3. Add **BlinkObject** component
4. Configure BlinkObject:
   - Visibility Mode: NormalOnly
   - **Disable On Hide: ✗ (UNCHECKED)**
5. Configure MovingPlatform:
   - Blink Offset: (5, 0, 0)
   - Show Debug Logs: ✓
6. Play and blink
7. **Expected**:
   - Warning in console about BlinkObject conflict
   - Platform moves 5 units right
   - Platform sprite disappears (renderer disabled)
   - Platform collider disabled
   - MovingPlatform logs show movement occurred ✅

### Test Case 3: MovingPlatform + BlinkObject (disableOnHide = true) - FAILS

1. Same as Test Case 2, but set `Disable On Hide: ✓ (CHECKED)`
2. Play and blink
3. **Expected**:
   - Warning in console about BlinkObject conflict
   - Platform disappears immediately
   - Console shows "OnDisable called"
   - NO movement logs appear
   - Platform does NOT move ❌
4. This demonstrates the issue

### Test Case 4: Smooth Movement

1. Create platform with MovingPlatform only
2. Configure:
   - Movement Type: **Smooth**
   - Transition Speed: 3
   - Blink Offset: (10, 0, 0)
3. Play and blink
4. **Expected**: Platform smoothly lerps from position A to B over ~3 seconds ✅

## Debug Checklist

If MovingPlatform still doesn't work:

- [ ] Check console for warnings/errors
- [ ] Verify `Show Debug Logs` is enabled
- [ ] Confirm `WorldStateManager` exists in scene
- [ ] Check platform has correct layer assigned
- [ ] If using BlinkObject, verify `Disable On Hide = false`
- [ ] Verify `Use Starting Position As Normal` matches your setup
- [ ] Check that `Blink Offset` is not (0, 0, 0)
- [ ] Ensure `Movement Type` is set correctly

## Console Output Guide

### Successful MovingPlatform Execution

```
Platform: Set normalPosition to starting position (0, 0, 0)
Platform: Subscribed to WorldStateManager, current state: Normal
Platform: HandleEnterBlink called
Platform: Instantly moved to (5, 0, 0) (state: Blink)
Platform: HandleExitBlink called
Platform: Instantly moved to (0, 0, 0) (state: Normal)
```

### Failed Execution (BlinkObject Conflict)

```
Platform: Set normalPosition to starting position (0, 0, 0)
Platform: MovingPlatform + BlinkObject detected. If BlinkObject has 'Disable On Hide' enabled...
Platform: Subscribed to WorldStateManager, current state: Normal
Platform: OnDisable called, unsubscribed from events
[No movement logs - platform never received HandleEnterBlink]
```

### Failed Execution (WorldStateManager Missing)

```
Platform: Set normalPosition to starting position (0, 0, 0)
Platform: WorldStateManager not found! MovingPlatform won't work.
```

## Summary

The issue was caused by **component execution order conflict**. When **BlinkObject** disables the GameObject, it interrupts **MovingPlatform**'s operation before movement can occur.

**Recommended Solution**: Remove **BlinkObject** from moving platforms. Use layer assignment and physics collision matrix to control when platforms are solid/non-solid.

**Alternative Solution**: If you must use both components, set `Disable On Hide = false` in BlinkObject, but be aware the platform will move even when invisible.

The updated **MovingPlatform** script now includes warnings, better initialization, debug logging, and delayed subscription to prevent race conditions.
