using UnityEngine;
using System;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance { get; private set; }
    public bool isBlinking { get; private set; }

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void enteringBlink()
    {
        if (isBlinking) return;
        isBlinking = true;
        Debug.Log("Entering blink world");
        
    }

    public void exitingBlink()
    {
        if (!isBlinking) return;
        isBlinking = false;
        Debug.Log("Exiting blink world");
    }
}
