using UnityEngine;
using UnityEngine.UI;

public class MinimapDot : MonoBehaviour
{
    private EnemyMovement enemy;
    private Image dotImage;

    private Color normalColor;
    private Color chasingColor;
    private float blinkSpeed;
    private float opacity;

    private bool isBlinking = false;
    private float blinkTimer = 0f;

    public void Initialize(EnemyMovement enemyRef, Color normal, Color chasing, float blink, float opacityMultiplier)
    {
        enemy = enemyRef;
        normalColor = normal;
        chasingColor = chasing;
        blinkSpeed = blink;
        opacity = opacityMultiplier;

        dotImage = GetComponent<Image>();

        if (dotImage != null)
        {
            UpdateColor();
        }
    }

    void Update()
    {
        if (enemy == null)
            return;

        UpdateColor();

        // Handle blinking animation if enemy is chasing
        if (enemy.isChasing)
        {
            if (!isBlinking)
            {
                isBlinking = true;
                blinkTimer = 0f;
            }

            blinkTimer += Time.deltaTime * blinkSpeed;

            // Oscillate between chasing color and transparent
            float alpha = (Mathf.Sin(blinkTimer * Mathf.PI * 2f) + 1f) / 2f; // 0 to 1 sine wave
            alpha = Mathf.Lerp(0.3f, 1f, alpha); // Keep minimum visibility at 0.3

            if (dotImage != null)
            {
                Color currentColor = chasingColor;
                currentColor.a = alpha * opacity;
                dotImage.color = currentColor;
            }
        }
        else
        {
            isBlinking = false;
            blinkTimer = 0f;

            // Set to normal color
            if (dotImage != null)
            {
                Color currentColor = normalColor;
                currentColor.a = opacity;
                dotImage.color = currentColor;
            }
        }
    }

    private void UpdateColor()
    {
        if (dotImage == null || enemy == null)
            return;

        // This is handled in Update with blinking logic
        // But we set initial color here for non-blinking state
        if (!enemy.isChasing && !isBlinking)
        {
            Color currentColor = normalColor;
            currentColor.a = opacity;
            dotImage.color = currentColor;
        }
    }
}
