using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Checkpoint Tracking")]
    private int checkpointsPassed = 0;
    private int totalCheckpoints = 0;

    public delegate void CheckpointUpdated(int passed, int total);
    public event CheckpointUpdated OnCheckpointUpdated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CountTotalCheckpoints();
        UpdateUI();
    }

    private void CountTotalCheckpoints()
    {
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        totalCheckpoints = checkpoints.Length;
        Debug.Log("Total checkpoints in scene: " + totalCheckpoints);
    }

    public void CheckpointReached()
    {
        checkpointsPassed++;
        Debug.Log("Checkpoint passed: " + checkpointsPassed + " / " + totalCheckpoints);
        UpdateUI();
    }

    private void UpdateUI()
    {
        OnCheckpointUpdated?.Invoke(checkpointsPassed, totalCheckpoints);
    }

    public bool AllCheckpointsPassed()
    {
        return checkpointsPassed >= totalCheckpoints;
    }

    public int GetCheckpointsPassed()
    {
        return checkpointsPassed;
    }

    public int GetTotalCheckpoints()
    {
        return totalCheckpoints;
    }
}
