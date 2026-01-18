using UnityEngine;
using System.Collections;

public class PlatformSwitch : MonoBehaviour
{
    public GameObject[] normalPlatforms;
    public GameObject[] blinkPlatforms;

    private BlinkController blink;

    private void OnEnable()
    {
        StartCoroutine(HookBlinkEvents());
    }

    IEnumerator HookBlinkEvents()
    {
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;

        blink.enterBlink += onblinkStart;
        blink.exitBlink += onblinkEnd;

        setnormalWorld();

    }

    private void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= onblinkStart;
            blink.exitBlink -= onblinkEnd;
        }
    }

    void onblinkStart()
    {
        setblinkWorld();
    }

    void onblinkEnd()
    {
        setnormalWorld();
   
    }

    void setnormalWorld()
    {
        foreach (GameObject gameObject in normalPlatforms)
        {
            if (gameObject != null) gameObject.SetActive(true);  
        }

        foreach (GameObject gameObject in blinkPlatforms)
        {
            if (gameObject != null) gameObject?.SetActive(false);
        }
    }

    void setblinkWorld()
    {
        foreach(GameObject gameObject in normalPlatforms)
        {
            if (gameObject != null) gameObject.SetActive(false);
        }

        foreach (GameObject gameObject in blinkPlatforms)
        {
            if (gameObject != null) gameObject?.SetActive(true);
        }
    }
}
