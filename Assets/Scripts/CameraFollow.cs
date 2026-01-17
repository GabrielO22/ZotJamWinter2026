using JetBrains.Annotations;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform player;        // target is player
    public float cameraSpeed = 2f;
    public float fixedX;            // x position should not change
    public float offsetZ = -10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        fixedX = transform.position.x;      // remember the starting x point

    }

    // Update is called once per frame
    void Update()
    {
        float newY = player.position.y;

        Vector3 CameraPosition = new Vector3(fixedX, newY,  offsetZ);

        Vector3 NextPosition = Vector3.Lerp(transform.position, CameraPosition, cameraSpeed * Time.deltaTime);

        transform.position = NextPosition;
    }
}
