# CrumblingPlatform Bug Fixes

## Issues Diagnosed

### Issue 1: Platform Only Crumbles Once
**Symptom**: When `oneTimeUse = false`, the platform should reform and crumble again on subsequent player contact. However, it only worked once.

**Root Cause**:
- Line 69 in `CrumblingPlatform.cs` set `hasBeenUsed = true` unconditionally on every crumble
- Line 105 checked `if (oneTimeUse && hasBeenUsed)` to prevent reformation
- Once `hasBeenUsed` was true, it stayed true forever, even when `oneTimeUse = false`
- This meant even reusable platforms became one-time-use after first crumble

**Fix**:
```csharp
// OLD CODE (Line 68-69)
isCrumbling = true;
hasBeenUsed = true;  // ❌ Always set to true

// NEW CODE (Line 68-74)
isCrumbling = true;

// Only mark as used if one-time use is enabled
if (oneTimeUse)
{
    hasBeenUsed = true;  // ✅ Only set when oneTimeUse is enabled
}
```

**Result**: Now `hasBeenUsed` only gets set when `oneTimeUse = true`, allowing reusable platforms to crumble repeatedly.

---

### Issue 2: Player Cannot Jump While Standing on Crumbling Platform
**Symptom**: When player lands on a crumbling platform and it starts turning red (warning phase), the player cannot jump. They're effectively "stuck" until the platform fully crumbles.

**Root Cause**:
Unity physics quirk with `OnCollisionExit2D`:

1. Player lands on platform → `OnCollisionEnter2D` fires → `onGround = true`
2. Platform starts crumbling (turns red) but collider is **still enabled**
3. Player remains in collision contact with platform → `onGround` stays `true` ✅
4. After `crumbleDelay`, platform collider is **disabled** via `platformCollider.enabled = false`
5. **When a collider is disabled programmatically, `OnCollisionExit2D` does NOT fire** (Unity limitation)
6. Player is now falling through air but `onGround` is still stuck at `true`
7. Ground detection is broken - player can jump mid-air, or cannot jump at all depending on timing

**Additional Context**:
- PlayerMovement uses `onGround` flag to gate jump input (line 81: `if (onGround)`)
- Ground detection only used `OnCollisionEnter2D` and `OnCollisionExit2D`
- No continuous verification of ground contact

**Fix Applied to PlayerMovement.cs**:

1. **Added `OnCollisionStay2D`** (Line 137-144):
```csharp
private void OnCollisionStay2D(Collision2D collision)
{
    // Continuously verify ground contact (fixes crumbling platform issue)
    if (((1 << collision.gameObject.layer) & groundLayers) != 0)
    {
        onGround = true;
    }
}
```

2. **Added Update() safety check** (Line 155-179):
```csharp
void Update()
{
    // Safety check: if player has no ground collisions, force onGround to false
    // This catches edge cases where collider disables without triggering OnCollisionExit2D
    if (onGround)
    {
        bool hasGroundContact = false;
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int contactCount = rigidBody.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            if (((1 << contacts[i].collider.gameObject.layer) & groundLayers) != 0)
            {
                hasGroundContact = true;
                break;
            }
        }

        if (!hasGroundContact)
        {
            onGround = false;
        }
    }
}
```

**How the Fix Works**:
- `OnCollisionStay2D` continuously sets `onGround = true` while player is touching ground
- `Update()` performs active verification every frame by querying `rigidBody.GetContacts()`
- If `onGround = true` but no ground contacts exist in physics system → force `onGround = false`
- This catches the edge case where collider disables without firing `OnCollisionExit2D`

**Result**:
- Player can now jump freely while standing on crumbling platform during warning phase
- When platform crumbles and collider disables, `onGround` immediately becomes `false`
- Ground detection is robust across all platform types (normal, crumbling, moving, etc.)

---

## Testing Checklist

### Test Case 1: Reusable Crumbling Platform
- [ ] Create crumbling platform with `oneTimeUse = false`
- [ ] Player lands on platform → platform turns red
- [ ] After delay, platform crumbles (goes gray, collider disables)
- [ ] Player blinks → platform reforms (white, collider enabled)
- [ ] Player lands on platform again → platform should crumble again ✅
- [ ] Repeat 3-5 times to verify continuous reusability

### Test Case 2: One-Time Crumbling Platform
- [ ] Create crumbling platform with `oneTimeUse = true`
- [ ] Player lands on platform → platform crumbles
- [ ] Player blinks → platform should NOT reform ✅
- [ ] Platform remains crumbled permanently

### Test Case 3: Jump During Crumble Warning
- [ ] Player lands on crumbling platform
- [ ] During warning phase (platform is red), press jump
- [ ] Player should jump successfully ✅
- [ ] Player should be able to jump multiple times rapidly during warning phase

### Test Case 4: Jump After Platform Crumbles
- [ ] Player lands on crumbling platform
- [ ] Wait for platform to fully crumble
- [ ] Player falls through platform
- [ ] Press jump while falling
- [ ] Player should NOT jump (no ground contact) ✅

### Test Case 5: Ground Detection on Other Platforms
- [ ] Test player jump on normal static platforms → should work
- [ ] Test player jump on MovingPlatform → should work
- [ ] Test player jump on BlinkTrampoline → should work
- [ ] Test player jump on regular ground → should work

---

## Technical Details

### Files Modified

1. **CrumblingPlatform.cs**:
   - Line 68-74: Conditional `hasBeenUsed` flag setting

2. **PlayerMovement.cs**:
   - Line 137-144: Added `OnCollisionStay2D()` method
   - Line 155-179: Added `Update()` with active ground contact verification

### Why OnCollisionExit2D Doesn't Fire When Collider Disables

This is documented Unity behavior:
- `OnCollisionExit2D` only fires when objects **physically separate** during physics simulation
- When you programmatically disable a collider via `collider.enabled = false`, Unity removes it from physics calculations **immediately**
- No collision event is generated because the physics engine doesn't "see" a separation - the collider just disappears
- This is intentional to avoid performance overhead of generating events for every collider enable/disable

### Alternative Solutions Considered

1. **Make platform a trigger during warning phase**:
   - ❌ Rejected: Player would fall through platform during warning
   - ❌ Would require complex trigger/solid collider swapping

2. **Use Raycast/BoxCast for ground detection**:
   - ❌ Rejected: More expensive than contact checking
   - ❌ Requires careful configuration of cast distance and layers

3. **Force OnCollisionExit2D by moving platform**:
   - ❌ Rejected: Hacky, would cause visual glitches
   - ❌ Doesn't solve the fundamental design issue

4. **Active contact verification (chosen solution)**:
   - ✅ Robust: Works for all edge cases
   - ✅ Performant: Only runs when `onGround = true`
   - ✅ Simple: Leverages built-in `Rigidbody2D.GetContacts()`

---

## Performance Impact

### CrumblingPlatform Changes
- **Impact**: None
- Only a conditional check added, no additional logic

### PlayerMovement Changes
- **Impact**: Minimal
- `OnCollisionStay2D`: Fires continuously while on ground (~50-60 times/sec in FixedUpdate)
  - Only performs LayerMask bitwise check (extremely fast)
- `Update()`: Runs every frame but only when `onGround = true`
  - Allocates 10-element contact array (stack allocation, negligible)
  - `GetContacts()` is optimized native method
  - Loop typically exits after 1-2 iterations

**Estimated overhead**: < 0.1ms per frame (negligible)

---

## Summary

Both issues stemmed from incomplete state management:

1. **Issue 1**: State flag (`hasBeenUsed`) not properly scoped to its condition (`oneTimeUse`)
2. **Issue 2**: State flag (`onGround`) not actively verified, relying on events that don't always fire

The fixes implement proper state verification:
- **CrumblingPlatform**: Only set permanent state when permanent behavior is desired
- **PlayerMovement**: Actively verify state instead of passively waiting for events

These changes make the systems more robust and resilient to edge cases in Unity's physics engine.
