using System.Runtime.CompilerServices;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private Vector2 respawnPoint = new Vector2(0, 0);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyMovement enemy = collision.gameObject.GetComponent<EnemyMovement>();
            if (enemy != null && enemy.chasingState())
            {
                rigidBody.linearVelocity = Vector2.zero;
                transform.position = respawnPoint;
            }
            
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            respawnPoint = other.transform.position;
            other.gameObject.SetActive(false);
        }

    }

    
}

