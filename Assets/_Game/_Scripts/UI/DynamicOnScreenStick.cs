using UnityEngine;
using UnityEngine.EventSystems;


public class DynamicJoystickMover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("References")]
    [Tooltip("The visual background of the joystick that will be moved to the touch position.")]
    [SerializeField] private RectTransform joystickBackground;
    
    [Tooltip("The actual knob object that has your movement logic (e.g. OnScreenStick) attached.")]
    [SerializeField] private GameObject joystickKnob;

    private Vector2 originalAnchoredPosition;
    
    private void Start()
    {
        if (joystickBackground != null)
        {
            originalAnchoredPosition = joystickBackground.anchoredPosition;
        }
        else
        {
            Debug.LogWarning($"[{nameof(DynamicJoystickMover)}] {gameObject.name} is missing a reference to the Joystick Background.", gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystickBackground == null || joystickBackground.parent == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        joystickBackground.localPosition = localPoint;

        if (joystickKnob != null)
        {
            ExecuteEvents.Execute(joystickKnob, eventData, ExecuteEvents.pointerDownHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickKnob != null)
        {
            ExecuteEvents.Execute(joystickKnob, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (joystickKnob != null)
        {
            ExecuteEvents.Execute(joystickKnob, eventData, ExecuteEvents.pointerUpHandler);
        }

        if (joystickBackground != null)
        {
            joystickBackground.anchoredPosition = originalAnchoredPosition;
        }
    }
}
