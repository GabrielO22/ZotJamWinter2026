# Blink Gauge UI Implementation Guide

## Overview
This guide details how to implement the forced blink gauge UI system that integrates with the WorldStateManager. The gauge visually represents the time remaining until a forced blink occurs, changing color from green to red and shaking more intensely as time runs out.

---

## System Architecture

### Core Components
1. **WorldStateManager** - Core blink system manager
2. **BlinkGaugeUI** - UI gauge component with visual effects
3. **ManaController** - Alternative visual gauge (sprite-based scaling)
4. **HUDManager** - Overall UI management and warnings

---

## Implementation Steps

### Step 1: Set Up WorldStateManager

The WorldStateManager should already be in your scene. Configure the forced blink settings:

**Inspector Settings:**
- `Enable Forced Blink`: `true`
- `Forced Blink Time`: `10` seconds (adjust to taste)
- `Reset Gauge On Manual Blink`: `true` (recommended)

**Location:** Attach to a GameObject named "WorldStateManager" or similar

---

### Step 2: Create Blink Gauge UI (Canvas-based)

#### A. Create UI Hierarchy

1. **Create Canvas** (if not exists)
   - Right-click in Hierarchy → UI → Canvas
   - Canvas Scaler: Scale with Screen Size
   - Reference Resolution: 1920x1080

2. **Create Gauge Container**
   - Add Empty GameObject under Canvas: "BlinkGaugeContainer"
   - RectTransform position: Bottom-left or top-right corner
   - Anchors: Set to corner position

3. **Create Background Image**
   - Add UI → Image under Container: "GaugeBackground"
   - Color: Dark gray or black
   - Size: 200x30 (width x height)

4. **Create Fill Image**
   - Add UI → Image under Container: "GaugeFill"
   - Image Type: **Filled**
   - Fill Method: **Horizontal**
   - Fill Origin: **Left**
   - Color: Green (will be controlled by script)
   - Anchor: Stretch horizontally

#### B. Attach BlinkGaugeUI Script

1. Add `BlinkGaugeUI` component to **BlinkGaugeContainer**
2. Configure in Inspector:

```
UI References:
  - Fill Image: Drag "GaugeFill" Image
  - Gauge Container: Drag "BlinkGaugeContainer" RectTransform

Color Settings:
  - Full Color: RGB(0, 255, 0) - Green
  - Empty Color: RGB(255, 0, 0) - Red
  - Color Gradient: (Optional) Create custom gradient

Shake Settings:
  - Enable Shake: ✓ (checked)
  - Max Shake Intensity: 10
  - Shake Frequency: 20
  - Shake Curve: Default linear curve (or customize)
    * Tip: Use exponential curve for more dramatic shake at low values

References:
  - World State Manager: Auto-finds or drag manually
```

---

### Step 3: Alternative - Sprite-based Gauge (ManaController)

If you prefer a simpler sprite-based gauge that scales:

#### Setup
1. Create a Sprite GameObject in your scene
2. Position it where you want the gauge (e.g., bottom of screen)
3. Add `ManaController` script

#### Inspector Settings:
```
Visual Settings:
  - Max Scale X: 3.0 (scale multiplier when full)

Color Settings (Optional):
  - Use Color Gradient: ✓ (if you want color change)
  - Full Color: Green
  - Empty Color: Red
```

**How it works:**
- The sprite scales on the X-axis based on time remaining
- Optionally changes color from green to red
- Simpler alternative to canvas-based UI

---

## Bug Fixes Applied

### Shake Intensity Bug (Fixed)

**Problem:**
The original implementation used `Mathf.Sin(Time.time * frequency)` which caused unbounded shake growth as `Time.time` increases infinitely.

**Solution:**
Replaced with Perlin noise for smooth, bounded random motion:

```csharp
// OLD (BUGGY):
float shakeX = Mathf.Sin(Time.time * shakeFrequency) * currentShakeIntensity;
float shakeY = Mathf.Cos(Time.time * shakeFrequency * 1.3f) * currentShakeIntensity;

// NEW (FIXED):
float time = Time.time * shakeFrequency * 0.1f;
float noiseX = Mathf.PerlinNoise(shakeOffsetX + time, 0f) * 2f - 1f; // -1 to 1
float noiseY = Mathf.PerlinNoise(shakeOffsetY + time, 0f) * 2f - 1f; // -1 to 1
float shakeX = noiseX * currentShakeIntensity;
float shakeY = noiseY * currentShakeIntensity;
```

**Key Changes:**
- Uses Perlin noise with random offsets (`shakeOffsetX`, `shakeOffsetY`)
- Noise values are normalized to -1 to 1 range
- Shake is always bounded by `currentShakeIntensity`
- Smooth, organic shake motion

---

## Advanced Customization

### Custom Shake Curves

Create custom shake intensity curves in the Inspector:

**Linear (Default):**
- Time 0.0 → Value 0.0
- Time 1.0 → Value 1.0
- Result: Steady increase in shake

**Exponential (Recommended):**
- Time 0.0 → Value 0.0
- Time 0.5 → Value 0.1
- Time 0.8 → Value 0.4
- Time 1.0 → Value 1.0
- Result: Dramatic shake increase at low values

**Constant Warning:**
- Time 0.0 → Value 0.0
- Time 0.0 → Value 0.8
- Time 1.0 → Value 0.8
- Result: Immediate strong shake when below threshold

### Custom Color Gradients

Instead of simple lerp, create Gradient in Inspector:

1. Click "Color Gradient" field
2. Add color keys:
   - 0% (Full): Green
   - 50%: Yellow
   - 100% (Empty): Red

### Warning Text Integration

Add warning text through HUDManager:

**Setup:**
1. Create TextMeshProUGUI: "ForcedBlinkWarning"
2. Position at top-center of screen
3. Assign to HUDManager → `forcedBlinkWarningText`

**Behavior:**
- Shows "BLINK SOON!" when gauge < 25%
- Shows "FORCED BLINK!" when forced blink triggers
- Auto-hides after 2 seconds

---

## Integration with Existing Systems

### Player Input
Player can manually blink using the Blink button (default: E key):
```csharp
inputActions.Player.Blink.performed += OnBlink;

private void OnBlink(InputAction.CallbackContext context)
{
    WorldStateManager.Instance.TryBlink();
}
```

### Enemies
Enemies automatically respond to blink state changes via events:
```csharp
WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
WorldStateManager.Instance.OnExitBlink += HandleExitBlink;
```

### Camera Effects
Camera shake on blink transitions is handled automatically by WorldStateManager.

---

## Testing Checklist

- [ ] Gauge fills from green to red correctly
- [ ] Shake increases smoothly as gauge depletes
- [ ] Shake never exceeds max intensity (no runaway growth)
- [ ] Forced blink triggers when gauge reaches 0
- [ ] Manual blink resets the gauge timer
- [ ] Gauge resets after forced blink completes
- [ ] Warning text appears at 25% threshold
- [ ] Color gradient transitions smoothly

---

## Troubleshooting

### Gauge not updating
- Ensure WorldStateManager exists in scene
- Check "Enable Forced Blink" is checked
- Verify BlinkGaugeUI has WorldStateManager reference

### Shake too intense or too weak
- Adjust "Max Shake Intensity" (default: 10)
- Modify "Shake Frequency" (default: 20)
- Edit "Shake Curve" for non-linear behavior

### Gauge resets immediately after forced blink
- This is correct behavior
- Adjust "Forced Blink Time" for longer intervals

### Colors not changing
- Ensure "Fill Image" is assigned
- Check "Color Gradient" or use default colors
- Verify Image component exists on Fill Image

### Shake doesn't stop at full gauge
- Bug fixed in current implementation
- If still occurring, check Perlin noise implementation
- Verify `currentShakeIntensity > 0.01f` condition

---

## Performance Notes

- **Perlin Noise:** Very efficient, runs in Update() with negligible cost
- **Gradient Evaluation:** Called once per frame, negligible cost
- **RectTransform Updates:** More expensive, but only when shaking

**Optimization Tips:**
- Disable shake on low-end devices if needed
- Use ManaController instead of BlinkGaugeUI for better performance
- Reduce shake frequency if experiencing frame drops

---

## File Locations

```
Assets/Scripts/
├── Managers/
│   ├── WorldStateManager.cs    (Core blink system)
│   └── WorldState.cs            (Enum definition)
├── UI/
│   ├── BlinkGaugeUI.cs          (Canvas-based gauge)
│   └── HUDManager.cs            (Overall UI management)
├── ManaController.cs            (Alternative sprite gauge)
└── PlayerMovement.cs            (Player blink input)
```

---

## Credits

**Implementation:** Your blink system with forced gauge mechanic
**Bug Fix:** Perlin noise shake implementation
**Integration:** WorldStateManager event-based architecture

---

## Version History

**v1.0** - Initial implementation with linear shake
**v1.1** - Fixed unbounded shake intensity bug
**v1.2** - Added Perlin noise for smooth shake
**v2.0** - Revamped system, removed colleague's implementation

---

## Additional Resources

- Unity UI Documentation: https://docs.unity3d.com/Manual/UISystem.html
- Perlin Noise Reference: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
- Animation Curves: https://docs.unity3d.com/Manual/animeditor-AnimationCurves.html
