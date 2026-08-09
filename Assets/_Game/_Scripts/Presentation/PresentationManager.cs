using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PresentationManager : MonoBehaviour
{
    [Header("Slides Setup")]
    public List<PresentationSlide> slides = new List<PresentationSlide>();
    
    [Header("Camera Setup")]
    [Tooltip("The main camera that will physically move to match the slide cameras.")]
    public Camera presentationCamera;
    public float cameraTransitionDuration = 0.75f;
    public AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int currentSlideIndex = 0;
    private Coroutine cameraTransitionCoroutine;

    private void Start()
    {
        if (presentationCamera == null)
        {
            presentationCamera = Camera.main;
        }

        // Initialize all slides to exit state initially, except the first one
        for (int i = 0; i < slides.Count; i++)
        {
            if (slides[i] != null)
                slides[i].ExitSlide();
        }

        if (slides.Count > 0 && slides[0] != null)
        {
            slides[0].EnterSlide();
            SnapCameraToSlide(slides[0]);
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            Debug.Log("[PresentationManager] Right Arrow pressed.");
            NextStep();
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            Debug.Log("[PresentationManager] Left Arrow pressed.");
            PreviousStep();
        }
    }

    public void NextStep()
    {
        Debug.Log($"[PresentationManager] NextStep called. Current index: {currentSlideIndex}. Total slides: {slides.Count}");
        if (slides.Count == 0 || currentSlideIndex >= slides.Count) return;

        PresentationSlide currentSlide = slides[currentSlideIndex];
        
        // Let the current slide try to handle the input (e.g. nested videos)
        if (currentSlide != null && currentSlide.TryNextInnerStep())
        {
            Debug.Log("[PresentationManager] Slide handled inner step. Not transitioning.");
            return; 
        }

        // Otherwise, move to the next slide
        if (currentSlideIndex < slides.Count - 1)
        {
            Debug.Log($"[PresentationManager] Transitioning from slide {currentSlideIndex} to {currentSlideIndex + 1}");
            if (currentSlide != null) currentSlide.ExitSlide();
            
            currentSlideIndex++;
            PresentationSlide nextSlide = slides[currentSlideIndex];
            
            if (nextSlide != null)
            {
                nextSlide.EnterSlide();
                TransitionCameraToSlide(nextSlide);
            }
        }
    }

    public void PreviousStep()
    {
        Debug.Log($"[PresentationManager] PreviousStep called. Current index: {currentSlideIndex}");
        if (slides.Count == 0 || currentSlideIndex < 0) return;

        PresentationSlide currentSlide = slides[currentSlideIndex];

        // Let the current slide try to handle the input (e.g. nested videos)
        if (currentSlide != null && currentSlide.TryPreviousInnerStep())
        {
            Debug.Log("[PresentationManager] Slide handled previous inner step.");
            return;
        }

        // Otherwise, move to the previous slide
        if (currentSlideIndex > 0)
        {
            Debug.Log($"[PresentationManager] Transitioning from slide {currentSlideIndex} to {currentSlideIndex - 1}");
            if (currentSlide != null) currentSlide.ExitSlide();

            currentSlideIndex--;
            PresentationSlide prevSlide = slides[currentSlideIndex];

            if (prevSlide != null)
            {
                prevSlide.EnterSlide();
                TransitionCameraToSlide(prevSlide);
            }
        }
    }

    private void SnapCameraToSlide(PresentationSlide slide)
    {
        if (presentationCamera == null || slide.targetCameraTransform == null) return;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        presentationCamera.transform.position = slide.targetCameraTransform.position;
        presentationCamera.transform.rotation = slide.targetCameraTransform.rotation;
    }

    private void TransitionCameraToSlide(PresentationSlide slide)
    {
        if (presentationCamera == null || slide.targetCameraTransform == null) return;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(CameraTransitionRoutine(slide.targetCameraTransform));
    }

    private IEnumerator CameraTransitionRoutine(Transform target)
    {
        Vector3 startPos = presentationCamera.transform.position;
        Quaternion startRot = presentationCamera.transform.rotation;
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / cameraTransitionDuration;
            float curveT = cameraTransitionCurve.Evaluate(Mathf.Clamp01(t));

            presentationCamera.transform.position = Vector3.Lerp(startPos, target.position, curveT);
            presentationCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, curveT);

            yield return null;
        }

        presentationCamera.transform.position = target.position;
        presentationCamera.transform.rotation = target.rotation;
    }
}
