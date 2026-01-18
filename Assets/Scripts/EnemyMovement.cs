using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float EnemySpeed = 1f;
    private bool isChasing = false;

    // Update is called once per frame

    void OnEnable()
    {
        if (WorldState.Instance != null)
        {
            WorldState.Instance.enterBlink += startChasing;
            WorldState.Instance.exitBlink += stopChasing;
        }
    }

    void OnDisable()
    {
        if (WorldState.Instance != null)
        {
            WorldState.Instance.enterBlink -= startChasing;
            WorldState.Instance.exitBlink -= stopChasing;
        }
    }

    void startChasing()
    {
        isChasing = true;
    }

    void stopChasing()
    {
        isChasing = false;
    }
    void Update()
    {
        if (!isChasing) return;
        Vector3 direction = player.position - transform.position;   // get directino to player
        direction.Normalize();
        transform.position += direction * EnemySpeed * Time.deltaTime;
    }

    public bool chasingState()
    {
        return isChasing;
    }
}
