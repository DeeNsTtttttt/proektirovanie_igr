using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private bool hideCursor = false;

    private void Awake()
    {
        if (hideCursor)
        {
            Cursor.visible = false;
        }
    }
}