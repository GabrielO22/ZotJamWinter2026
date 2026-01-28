using UnityEngine;

public class ManaController : MonoBehaviour
{
    public float maxMana = 100;
    public float currentMana;
    public float drainPerSecond = 10f;
    public BlinkController blinkController;

    [Header("Optional Eye Gauge")]
    [Tooltip("Optional reference to EyeBlinkGauge for visual feedback")]
    public EyeBlinkGauge eyeGauge;

    [Header("Drain Behavior")]
    [Tooltip("If true, mana will not drain while in blink state")]
    public bool pauseDrainDuringBlink = true;

    private bool isDraining = true;

    public void Awake()
    {
        currentMana = maxMana;
    }

    void OnEnable()
    {
        if (blinkController != null)
        {
            blinkController.enterBlink += OnEnterBlink;
            blinkController.exitBlink += OnExitBlink;
        }
    }

    void OnDisable()
    {
        if (blinkController != null)
        {
            blinkController.enterBlink -= OnEnterBlink;
            blinkController.exitBlink -= OnExitBlink;
        }
    }

    public void FixedUpdate()
    {
        // Only drain if draining is active
        if (isDraining)
        {
            currentMana -= drainPerSecond * Time.deltaTime;
        }

        // Old scaling behavior removed - now handled by EyeBlinkGauge
        // If you need the old bar behavior, uncomment the line below:
        // this.transform.localScale = new Vector3((currentMana / maxMana) * 3, this.transform.localScale.y, this.transform.localScale.z);

        if (currentMana <= 0f)
        {
            blinkController.forceBlink();
            refill();
        }
    }

    private void OnEnterBlink()
    {
        refill();

        // Pause draining during blink if enabled
        if (pauseDrainDuringBlink)
        {
            isDraining = false;
        }
    }

    private void OnExitBlink()
    {
        // Resume draining after blink
        isDraining = true;
    }

    public void refill()
    {
        currentMana = maxMana;

        // Reset eye gauge shake if available
        if (eyeGauge != null)
        {
            eyeGauge.ResetShake();
        }
    }
}
