using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class FreeLookCamTouch : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static FreeLookCamTouch Instance { set; get; }

    public CinemachineOrbitalFollow _cinemachineOrbitalFollow;   // Reference to the Cinemachine FreeLook Camera
    public RectTransform uiImage;              // Reference to the UI Image (interactive region)

    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    public float rotationSensitivityX = 0.1f;  // Sensitivity for horizontal (X-axis) rotation
    public float rotationSensitivityY = 0.05f; // Sensitivity for vertical (Y-axis) rotation
    
    private void Awake()
    {
        Instance = this;
    }



    void Start()
    {
        // Initialize camera control values, in case you want to sync it with initial position of the UI image
        lastTouchPosition = new Vector2(uiImage.position.x, uiImage.position.y);
    }

    // Called when the user starts dragging on the Image
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastTouchPosition = eventData.position; // Track the initial touch position
    }

    // Called when the user stops dragging on the Image
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    // Called as the user drags on the Image
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Calculate the difference in touch position (delta)
        Vector2 delta = eventData.position - lastTouchPosition;

        // Update the camera's horizontal and vertical axis based on the delta position
        _cinemachineOrbitalFollow.HorizontalAxis.Value += delta.x * rotationSensitivityX;
        _cinemachineOrbitalFollow.VerticalAxis.Value -= delta.y * rotationSensitivityY;  // Negative to reverse the vertical direction
        
        // Store the current touch position for the next drag event
        lastTouchPosition = eventData.position;
    }
}