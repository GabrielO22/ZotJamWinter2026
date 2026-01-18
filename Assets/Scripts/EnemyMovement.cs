using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float EnemySpeed = 1f;
    public bool isChasing = false;

    private BlinkController blink;

    private void OnEnable()
    {
        StartCoroutine(HookBlinkEvents());
    }

    private IEnumerator HookBlinkEvents()
    {
        // wait until BlinkController.Instance exists
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;

        blink.enterBlink += StartChasing;
        blink.exitBlink += StopChasing;

        Debug.Log($"Enemy subscribed to BlinkController on {blink.gameObject.name}");
    }

    private void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= StartChasing;
            blink.exitBlink -= StopChasing;
        }
    }

    void Update()
    {
        if (!isChasing || player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * EnemySpeed * Time.deltaTime;
    }

    private void StartChasing()
    {
        isChasing = true;
        Debug.Log("Enemy: start chasing");
    }

    private void StopChasing()
    {
        isChasing = false;
        Debug.Log("Enemy: stop chasing");
    }
}
