using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkFlashEffect : MonoBehaviour
{
    public float flashInTime = 0.05f;
    public float flashOutTime = 0.20f;
    public float maxAlpha = 0.8f;

    private Image img;
    private Coroutine flashRoutine;
    private BlinkController blink; // the specific instance we subscribed to

    void Awake()
    {
        img = GetComponent<Image>();
        SetAlpha(0f);
    }

    void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        // Wait until a BlinkController exists (handles script execution order)
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;
        blink.enterBlink += PlayFlash;
        blink.exitBlink += PlayFlash;

        // Debug to prove subscription happened
        Debug.Log("BlinkFlashEffect subscribed to enterBlink");
    }

    void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= PlayFlash;
            blink.exitBlink -= PlayFlash;
        }

    }

    void PlayFlash()
    {
        // Debug to prove event fired
        Debug.Log("PlayFlash called!");

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        yield return FadeTo(maxAlpha, flashInTime);
        yield return FadeTo(0f, flashOutTime);
        flashRoutine = null;
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = img.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, target, t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(target);
    }

    void SetAlpha(float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
