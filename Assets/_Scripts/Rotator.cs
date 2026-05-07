using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 120f, 0f);
    [SerializeField] private Space rotationSpace = Space.Self;

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }
}