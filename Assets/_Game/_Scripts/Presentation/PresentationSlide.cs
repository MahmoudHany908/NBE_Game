using UnityEngine;
using UnityEngine.Events;

public enum SlideTransitionType 
{ 
    SmoothCamera, 
    FadeToBlack,
    Cut
}

public class PresentationSlide : MonoBehaviour
{
    [Tooltip("How the presentation should transition INTO this slide.")]
    public SlideTransitionType enterTransitionType = SlideTransitionType.SmoothCamera;

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
    /// Called when the user presses Next while this slide is active.
    /// Return true if the slide consumed the input (e.g., played an inner animation).
    /// Return false if the slide is completely done and the manager should move to the next slide.
    /// </summary>
    public virtual bool TryNextInnerStep()
    {
        return false;
    }

    /// <summary>
    /// Called when the user presses Previous while this slide is active.
    /// Return true if the slide consumed the input (e.g., went back an inner step).
    /// Return false if the slide is at its beginning and the manager should move to the previous slide.
    /// </summary>
    public virtual bool TryPreviousInnerStep()
    {
        return false;
    }
}
