using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public Transform player;
    public float EnemySpeed = 1f;


    // Update is called once per frame
    void Update()
    {
        Vector3 direction = player.position - transform.position;   // get directino to player
        direction.Normalize();
        transform.position += direction * EnemySpeed * Time.deltaTime;
    }
}
