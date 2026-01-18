using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    public Sprite newSprite;
    private Sprite oldSprite;
    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        oldSprite = spriteRenderer.sprite;
    }

    public void changeSprite()
    {
        spriteRenderer.sprite = newSprite;

    }

    // Update is called once per frame
    public void revertSprite()
    {
        spriteRenderer.sprite = oldSprite;
    }
}
