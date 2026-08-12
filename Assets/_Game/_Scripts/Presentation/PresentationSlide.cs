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

}
