using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// Rotates a CinemachineCamera's CinemachineOrbitalFollow around the target
// horizontally via touch drag, keeping the vertical/height fixed.
[RequireComponent(typeof(CinemachineCamera))]
public class OrbitCameraTouch : MonoBehaviour
{
    private CinemachineCamera vcam;
    private CinemachineOrbitalFollow orbitalFollow;

    [Header("Rotation")]
    [Tooltip("Degrees rotated horizontally per pixel of touch drag.")]
    [SerializeField] private float rotationSpeed = 0.25f;

    [Tooltip("Degrees rotated vertically per pixel of touch drag.")]
    [SerializeField] private float verticalRotationSpeed = 0.15f;

    [Tooltip("Minimum vertical angle (e.g. -20).")]
    [SerializeField] private float minVerticalAngle = -20f;

    [Tooltip("Maximum vertical angle (e.g. 60).")]
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Touch Area")]
    [Tooltip("Only touches that BEGIN inside this UI Image will rotate the camera.")]
    [SerializeField] private RectTransform touchArea;

    [Tooltip("Canvas' camera. Leave null if Canvas Render Mode is Screen Space - Overlay.")]
    [SerializeField] private Camera uiCamera;

    private int activeTouchId = -1;
    private Vector2 lastTouchPos;

    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        if (orbitalFollow == null)
        {
            Debug.LogWarning("OrbitCameraTouch: no CinemachineOrbitalFollow found on " + name + ". Add one as the Position component (Body) in the CM Camera's inspector.");
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (orbitalFollow == null) return;
        HandleTouchInput();
#if UNITY_EDITOR
        HandleMouseInput();
#endif
    }

    private void HandleTouchInput()
    {
        foreach (Touch touch in Touch.activeTouches)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    if (activeTouchId == -1 && IsInsideTouchArea(touch.screenPosition))
                    {
                        activeTouchId = touch.touchId;
                        lastTouchPos = touch.screenPosition;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    if (touch.touchId == activeTouchId)
                    {
                        Vector2 delta = touch.screenPosition - lastTouchPos;
                        Rotate(delta.x, delta.y);
                        lastTouchPos = touch.screenPosition;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    if (touch.touchId == activeTouchId)
                    {
                        activeTouchId = -1;
                    }
                    break;
            }
        }
    }

#if UNITY_EDITOR
    private bool isDraggingMouse = false;
    private void HandleMouseInput()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsInsideTouchArea(mousePos))
            {
                isDraggingMouse = true;
                lastTouchPos = mousePos;
            }
        }
        else if (mouse.leftButton.isPressed && isDraggingMouse)
        {
            Vector2 delta = mousePos - lastTouchPos;
            Rotate(delta.x, delta.y);
            lastTouchPos = mousePos;
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDraggingMouse = false;
        }
    }
#endif

    private void Rotate(float deltaX, float deltaY)
    {
        // Horizontal Rotation
        float newAngleX = orbitalFollow.HorizontalAxis.Value + deltaX * rotationSpeed;
        newAngleX %= 360f;
        if (newAngleX < 0f) newAngleX += 360f;
        orbitalFollow.HorizontalAxis.Value = newAngleX;

        // Vertical Rotation
        // We subtract deltaY so dragging your finger UP pushes the camera UP (which means looking down at the player)
        float newAngleY = orbitalFollow.VerticalAxis.Value - deltaY * verticalRotationSpeed;
        newAngleY = Mathf.Clamp(newAngleY, minVerticalAngle, maxVerticalAngle);
        orbitalFollow.VerticalAxis.Value = newAngleY;
    }

    private bool IsInsideTouchArea(Vector2 screenPos)
    {
        if (touchArea == null) return true; // no restriction, whole screen rotates
        return RectTransformUtility.RectangleContainsScreenPoint(touchArea, screenPos, uiCamera);
    }
}