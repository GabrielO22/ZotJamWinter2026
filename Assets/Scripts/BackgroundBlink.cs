using System.Collections;
using UnityEngine;

public class BackgroundBlink : MonoBehaviour
{
    private SpriteChanger spriteChanger;
    private BlinkController blink;

    void Awake()
    {
        spriteChanger = GetComponent<SpriteChanger>();
    }

    void OnEnable()
    {
        StartCoroutine(HookBlinkEvents());
    }

    IEnumerator HookBlinkEvents()
    {
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;

        blink.enterBlink += OnBlinkStart;
        blink.exitBlink += OnBlinkEnd;
    }

    void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= OnBlinkStart;
            blink.exitBlink -= OnBlinkEnd;
        }
    }

    private void OnBlinkStart()
    {
        if (spriteChanger != null)
            spriteChanger.changeSprite();
    }

    private void OnBlinkEnd()
    {
        if (spriteChanger != null)
            spriteChanger.revertSprite();
    }
}
