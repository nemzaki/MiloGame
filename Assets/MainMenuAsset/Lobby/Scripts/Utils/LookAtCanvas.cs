using UnityEngine;

public class LookAtCanvas : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform; // Get the main camera transform
    }

    void Update()
    {
        Vector3 lookDirection = camTransform.position - transform.position;
        lookDirection.y = 0; // Lock rotation to only Y-axis
        transform.rotation = Quaternion.LookRotation(-lookDirection);
    }
}
