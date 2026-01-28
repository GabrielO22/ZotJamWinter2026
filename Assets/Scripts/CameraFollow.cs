using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Legacy continuous camera follow system.
/// NOTE: Consider using CellBasedCamera for a room-to-room transition system.
/// This script is kept for backward compatibility.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;        // target is player
    public float cameraSpeed = 2f;
    public float fixedX;            // x position should not change
    public float offsetY = -2f;
    public float offsetZ = -10f;

    [Header("Legacy Mode")]
    [Tooltip("Enable to use this legacy follow system. Disable to manually switch to CellBasedCamera.")]
    public bool useLegacyFollow = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fixedX = transform.position.x;      // remember the starting x point

        // Warn if CellBasedCamera exists
        if (GetComponent<CellBasedCamera>() != null)
        {
            Debug.LogWarning("CameraFollow: CellBasedCamera component detected. Consider disabling CameraFollow to use the new cell-based system.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!useLegacyFollow || player == null) return;

        float newY = player.position.y + offsetY;

        Vector3 CameraPosition = new Vector3(fixedX, newY, offsetZ);

        Vector3 NextPosition = Vector3.Lerp(transform.position, CameraPosition, cameraSpeed * Time.deltaTime);

        transform.position = NextPosition;
    }
}
