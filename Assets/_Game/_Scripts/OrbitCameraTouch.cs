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
    [Tooltip("Degrees rotated per pixel of touch drag.")]
    [SerializeField] private float rotationSpeed = 0.25f;

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
                        Rotate(delta.x);
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

    private void Rotate(float deltaX)
    {
        float newAngle = orbitalFollow.HorizontalAxis.Value + deltaX * rotationSpeed;

        // Keep it wrapped 0-360 so it doesn't grow unbounded.
        newAngle %= 360f;
        if (newAngle < 0f) newAngle += 360f;

        orbitalFollow.HorizontalAxis.Value = newAngle;
    }

    private bool IsInsideTouchArea(Vector2 screenPos)
    {
        if (touchArea == null) return true; // no restriction, whole screen rotates
        return RectTransformUtility.RectangleContainsScreenPoint(touchArea, screenPos, uiCamera);
    }
}