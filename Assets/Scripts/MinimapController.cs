using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    [Tooltip("The RectTransform of the minimap container")]
    public RectTransform minimapContainer;

    [Tooltip("Background image of the minimap")]
    public Image minimapBackground;

    [Tooltip("Background color of the minimap")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);

    [Tooltip("Overall opacity of the minimap (0-1)")]
    [Range(0f, 1f)]
    public float minimapOpacity = 0.7f;

    [Tooltip("World radius around player to show on minimap")]
    public float scanRadius = 20f;

    [Header("Player Settings")]
    [Tooltip("The player transform to center the minimap on")]
    public Transform player;

    [Tooltip("Prefab for player dot (should have Image component)")]
    public GameObject playerDotPrefab;

    [Tooltip("Color of the player dot")]
    public Color playerDotColor = Color.white;

    [Header("Enemy Settings")]
    [Tooltip("Prefab for enemy dot (should have Image component)")]
    public GameObject enemyDotPrefab;

    [Tooltip("Color of enemy dot in normal state")]
    public Color enemyNormalColor = Color.yellow;

    [Tooltip("Color of enemy dot in chase/ghost state")]
    public Color enemyChasingColor = Color.red;

    [Tooltip("Blink speed for chasing enemies")]
    public float enemyBlinkSpeed = 2f;

    [Header("Icon Customization")]
    [Tooltip("Optional custom sprite for player dot")]
    public Sprite playerDotSprite;

    [Tooltip("Optional custom sprite for enemy dot")]
    public Sprite enemyDotSprite;

    [Tooltip("Optional border/frame sprite for minimap")]
    public Sprite minimapBorderSprite;

    [Tooltip("Optional Image component for minimap border")]
    public Image minimapBorder;

    // Internal tracking
    private GameObject playerDot;
    private Dictionary<EnemyMovement, MinimapDot> enemyDots = new Dictionary<EnemyMovement, MinimapDot>();

    void Start()
    {
        InitializeMinimap();
        CreatePlayerDot();
        FindAndTrackEnemies();
    }

    void Update()
    {
        UpdateDotPositions();
    }

    private void InitializeMinimap()
    {
        if (minimapBackground != null)
        {
            Color bgColor = backgroundColor;
            bgColor.a *= minimapOpacity;
            minimapBackground.color = bgColor;
        }

        if (minimapBorder != null && minimapBorderSprite != null)
        {
            minimapBorder.sprite = minimapBorderSprite;
        }
    }

    private void CreatePlayerDot()
    {
        if (playerDotPrefab == null || minimapContainer == null)
        {
            Debug.LogWarning("MinimapController: Missing player dot prefab or minimap container");
            return;
        }

        playerDot = Instantiate(playerDotPrefab, minimapContainer);
        Image dotImage = playerDot.GetComponent<Image>();

        if (dotImage != null)
        {
            Color color = playerDotColor;
            color.a *= minimapOpacity;
            dotImage.color = color;

            if (playerDotSprite != null)
            {
                dotImage.sprite = playerDotSprite;
            }
        }

        // Center player dot
        RectTransform rt = playerDot.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
        }
    }

    private void FindAndTrackEnemies()
    {
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);

        foreach (EnemyMovement enemy in enemies)
        {
            CreateEnemyDot(enemy);
        }

        Debug.Log($"Minimap tracking {enemies.Length} enemies");
    }

    private void CreateEnemyDot(EnemyMovement enemy)
    {
        if (enemyDotPrefab == null || minimapContainer == null || enemy == null)
            return;

        GameObject dotObj = Instantiate(enemyDotPrefab, minimapContainer);
        Image dotImage = dotObj.GetComponent<Image>();

        if (dotImage != null)
        {
            if (enemyDotSprite != null)
            {
                dotImage.sprite = enemyDotSprite;
            }
        }

        MinimapDot dot = dotObj.AddComponent<MinimapDot>();
        dot.Initialize(enemy, enemyNormalColor, enemyChasingColor, enemyBlinkSpeed, minimapOpacity);

        enemyDots[enemy] = dot;
    }

    private void UpdateDotPositions()
    {
        if (player == null || minimapContainer == null)
            return;

        // Update enemy dots
        List<EnemyMovement> toRemove = new List<EnemyMovement>();

        foreach (var kvp in enemyDots)
        {
            EnemyMovement enemy = kvp.Key;
            MinimapDot dot = kvp.Value;

            if (enemy == null || dot == null)
            {
                toRemove.Add(enemy);
                continue;
            }

            // Calculate world distance from player
            float distance = Vector2.Distance(player.position, enemy.transform.position);

            // Only show if within scan radius
            if (distance <= scanRadius)
            {
                dot.gameObject.SetActive(true);

                // Calculate relative position
                Vector2 relativePos = enemy.transform.position - player.position;

                // Convert to minimap coordinates
                Vector2 minimapPos = WorldToMinimapPosition(relativePos);

                // Update dot position
                RectTransform rt = dot.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = minimapPos;
                }
            }
            else
            {
                dot.gameObject.SetActive(false);
            }
        }

        // Clean up destroyed enemies
        foreach (var enemy in toRemove)
        {
            if (enemyDots[enemy] != null)
            {
                Destroy(enemyDots[enemy].gameObject);
            }
            enemyDots.Remove(enemy);
        }
    }

    private Vector2 WorldToMinimapPosition(Vector2 worldOffset)
    {
        if (minimapContainer == null)
            return Vector2.zero;

        // Get minimap size
        float minimapWidth = minimapContainer.rect.width;
        float minimapHeight = minimapContainer.rect.height;

        // Scale world position to minimap size
        float scaleX = minimapWidth / (scanRadius * 2f);
        float scaleY = minimapHeight / (scanRadius * 2f);

        float minimapX = worldOffset.x * scaleX;
        float minimapY = worldOffset.y * scaleY;

        return new Vector2(minimapX, minimapY);
    }

    // Call this if new enemies spawn during gameplay
    public void RefreshEnemies()
    {
        FindAndTrackEnemies();
    }

    void OnValidate()
    {
        // Update colors in editor
        if (minimapBackground != null)
        {
            Color bgColor = backgroundColor;
            bgColor.a *= minimapOpacity;
            minimapBackground.color = bgColor;
        }
    }
}
