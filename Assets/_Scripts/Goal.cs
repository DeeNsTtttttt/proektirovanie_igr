using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseOnWin = true;
    [SerializeField] private bool showCursorOnWin = true;

    private bool triggered;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player"))
        {
            return;
        }

        triggered = true;
        ShowWinPanel();
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (pauseOnWin)
        {
            Time.timeScale = 0f;
        }

        if (showCursorOnWin)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}