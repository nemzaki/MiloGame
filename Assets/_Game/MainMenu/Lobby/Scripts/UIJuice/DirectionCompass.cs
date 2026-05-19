using UnityEngine;

public class DirectionCompass : MonoBehaviour
{
    public Transform viewDirection;
    public RectTransform compassElement;
    public float compassSize;


    private void LateUpdate()
    {
        var forwardVector = Vector3.ProjectOnPlane(
            viewDirection.forward, Vector3.up).normalized;

        var forwardSignedAngle = Vector3.SignedAngle(
            forwardVector, Vector3.forward, Vector3.up);
        
        var compassOffset = (forwardSignedAngle / 180f) * compassSize;
        compassElement.anchoredPosition = new Vector3(compassOffset, 0);
    }
}
