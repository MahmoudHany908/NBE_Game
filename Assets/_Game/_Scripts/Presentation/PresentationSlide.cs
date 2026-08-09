using UnityEngine;
using UnityEngine.Events;

public class PresentationSlide : MonoBehaviour
{
    [Tooltip("The camera transform to smoothly transition to when this slide is active.")]
    public Transform targetCameraTransform;
    
    public UnityEvent OnSlideEnter;
    public UnityEvent OnSlideExit;

    public virtual void EnterSlide()
    {
        gameObject.SetActive(true);
        OnSlideEnter?.Invoke();
    }

    public virtual void ExitSlide()
    {
        gameObject.SetActive(false);
        OnSlideExit?.Invoke();
    }

    /// <summary>
    /// Returns true if this slide consumed the "Next" input (e.g. for nested videos).
    /// </summary>
    public virtual bool TryNextInnerStep()
    {
        return false;
    }

    /// <summary>
    /// Returns true if this slide consumed the "Previous" input (e.g. for nested videos).
    /// </summary>
    public virtual bool TryPreviousInnerStep()
    {
        return false;
    }
}
