using JetBrains.Annotations;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform camera;        // target is player
    public float CameraSpeed = 2f;
    public float FixedX;            // x position should not change
    public float OffsetZ = -10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        FixedX = transform.position.x;      // remember the starting x point

    }

    // Update is called once per frame
    void Update()
    {
        float newY = camera.position.y; 

        Vector3 CameraPosition = new Vector3(FixedX, newY,  OffsetZ);

        Vector3 NextPosition = Vector3.Lerp(transform.position, CameraPosition, CameraSpeed * Time.deltaTime);

        transform.position = NextPosition;
    }
}
