using UnityEngine;

public class ManaController : MonoBehaviour
{
    public float maxMana = 100;
    public float currentMana;
    public float drainPerSecond = 10f;
    public BlinkController blinkController;

    public void Awake()
    {
        currentMana = maxMana;
    }
    public void FixedUpdate()
    {
        currentMana -= drainPerSecond * Time.deltaTime;
        this.transform.localScale = new Vector3((currentMana / maxMana) * 3, this.transform.localScale.y, this.transform.localScale.z);

        if (currentMana <= 0f)
        {
            blinkController.forceBlink();
            refill();
        }
    }

    public void refill()
    {
        currentMana = maxMana;
    }
}
