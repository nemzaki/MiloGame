using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TouchInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private KeyCode keyToSimulate = KeyCode.Space; // Default to Space key, can be changed in Inspector
    private bool isPressed = false;

    private void Update()
    {
        // Check if the key is pressed down and perform action
        if (isPressed && Input.GetKey(keyToSimulate))
        {
            PerformAction();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        Debug.Log("Button Pressed: Action Started");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        Debug.Log("Button Released: Action Stopped");
    }

    private void PerformAction()
    {
        // Perform the action that should happen when the button is held down
        Debug.Log("Performing action while holding the button down");
    }
}