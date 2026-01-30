using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckpointUI : MonoBehaviour
{
    [Header("UI References")]
    public Image checkpointIcon;
    public TextMeshProUGUI checkpointText;

    void Start()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointUpdated += UpdateCheckpointDisplay;
            // Initialize display with current values
            UpdateCheckpointDisplay(
                CheckpointManager.Instance.GetCheckpointsPassed(),
                CheckpointManager.Instance.GetTotalCheckpoints()
            );
        }
        else
        {
            Debug.LogWarning("CheckpointManager not found in scene!");
        }
    }

    void OnDestroy()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointUpdated -= UpdateCheckpointDisplay;
        }
    }

    private void UpdateCheckpointDisplay(int passed, int total)
    {
        if (checkpointText != null)
        {
            checkpointText.text = passed + " / " + total;
        }
    }
}
