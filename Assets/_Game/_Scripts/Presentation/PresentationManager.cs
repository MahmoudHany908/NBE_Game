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
    public int CurrentSlideIndex => currentSlideIndex;
    
    private Coroutine cameraTransitionCoroutine;

    private void Start()
    {
        if (presentationCamera == null)
        {
            presentationCamera = Camera.main;
        }

        // Initialize all slides to exit state initially
        for (int i = 0; i < slides.Count; i++)
        {
            if (slides[i] != null)
                slides[i].ExitSlide();
        }

        if (VaultSurvivors.Presentation.PresentationState.IsReturningFromSubscene)
        {
            // Clear the flag
            VaultSurvivors.Presentation.PresentationState.IsReturningFromSubscene = false;
            
            // Resume at the saved index + 1 (the slide immediately following the transition slide)
            currentSlideIndex = VaultSurvivors.Presentation.PresentationState.ReturnSlideIndex + 1;
            
            // Safety bound check
            if (currentSlideIndex >= slides.Count) currentSlideIndex = slides.Count - 1;
            if (currentSlideIndex < 0) currentSlideIndex = 0;
        }

        if (slides.Count > 0 && slides[currentSlideIndex] != null)
        {
            slides[currentSlideIndex].EnterSlide();
            SnapCameraToSlide(slides[currentSlideIndex]);
        }

        // Check if we should fade in from black (from a scene transition)
        if (VaultSurvivors.Presentation.PresentationState.NeedsFadeIn)
        {
            VaultSurvivors.Presentation.PresentationState.NeedsFadeIn = false;
            StartCoroutine(FadeInFromBlackRoutine(VaultSurvivors.Presentation.PresentationState.FadeInDuration));
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            // Debug.Log("[PresentationManager] Right Arrow pressed.");
            NextStep();
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            // Debug.Log("[PresentationManager] Left Arrow pressed.");
            PreviousStep();
        }
    }

    private bool isTransitioning = false;
    public float fadeTransitionDuration = 0.5f;
    private Canvas fadeCanvas;
    private UnityEngine.UI.Image fadeImage;

    private void EnsureFadeUI()
    {
        if (fadeCanvas != null) return;
        
        GameObject go = new GameObject("PresentationFadeOverlay");
        go.transform.SetParent(this.transform);
        
        fadeCanvas = go.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 32767;
        
        GameObject imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(go.transform, false);
        fadeImage = imgGo.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        var rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        fadeImage.raycastTarget = true;
    }

    public void NextStep()
    {
        if (isTransitioning || slides.Count == 0 || currentSlideIndex >= slides.Count) return;

        if (currentSlideIndex < slides.Count - 1)
        {
            int targetIndex = currentSlideIndex + 1;
            HandleSlideChange(targetIndex);
        }
    }

    public void PreviousStep()
    {
        if (isTransitioning || slides.Count == 0 || currentSlideIndex < 0) return;

        if (currentSlideIndex > 0)
        {
            int targetIndex = currentSlideIndex - 1;
            HandleSlideChange(targetIndex);
        }
    }

    private void HandleSlideChange(int targetIndex)
    {
        PresentationSlide nextSlide = slides[targetIndex];
        SlideTransitionType transitionType = SlideTransitionType.SmoothCamera;
        if (nextSlide != null) transitionType = nextSlide.enterTransitionType;

        if (transitionType == SlideTransitionType.FadeToBlack)
        {
            StartCoroutine(FadeToSlideRoutine(targetIndex));
        }
        else if (transitionType == SlideTransitionType.Cut)
        {
            if (slides[currentSlideIndex] != null) slides[currentSlideIndex].ExitSlide();
            currentSlideIndex = targetIndex;
            
            if (nextSlide != null)
            {
                nextSlide.EnterSlide();
                SnapCameraToSlide(nextSlide);
            }
        }
        else
        {
            // Normal smooth transition
            if (slides[currentSlideIndex] != null) slides[currentSlideIndex].ExitSlide();
            currentSlideIndex = targetIndex;
            
            if (nextSlide != null)
            {
                nextSlide.EnterSlide();
                TransitionCameraToSlide(nextSlide);
            }
        }
    }

    private IEnumerator FadeToSlideRoutine(int targetIndex)
    {
        isTransitioning = true;
        EnsureFadeUI();

        // Fade to black
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / (fadeTransitionDuration / 2f);
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }
        fadeImage.color = Color.black;

        // Change slide instantly under the cover of black
        if (slides[currentSlideIndex] != null) slides[currentSlideIndex].ExitSlide();
        currentSlideIndex = targetIndex;
        
        PresentationSlide nextSlide = slides[currentSlideIndex];
        if (nextSlide != null)
        {
            nextSlide.EnterSlide();
            SnapCameraToSlide(nextSlide);
        }

        // Fade back in
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / (fadeTransitionDuration / 2f);
            fadeImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);

        isTransitioning = false;
    }

    private IEnumerator FadeInFromBlackRoutine(float duration)
    {
        isTransitioning = true;
        EnsureFadeUI();
        
        fadeImage.color = Color.black;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            fadeImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        
        isTransitioning = false;
    }

    public void TransitionToScene(string sceneName, float duration)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoadSceneRoutine(sceneName, duration));
    }

    private IEnumerator FadeAndLoadSceneRoutine(string sceneName, float duration)
    {
        isTransitioning = true;
        EnsureFadeUI();

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }
        fadeImage.color = Color.black;

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    private class CameraState
    {
        public Camera cam;
        public Vector3 targetPos;
        public Quaternion targetRot;
    }
    
    private List<CameraState> camerasInTransition = new List<CameraState>();

    private void CleanupPreviousTransition()
    {
        foreach(var state in camerasInTransition)
        {
            if (state.cam != null)
            {
                state.cam.transform.position = state.targetPos;
                state.cam.transform.rotation = state.targetRot;
            }
        }
        camerasInTransition.Clear();
    }

    private void SnapCameraToSlide(PresentationSlide slide)
    {
        if (presentationCamera == null || slide.targetCameraTransform == null) return;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);
            
        CleanupPreviousTransition();

        presentationCamera.transform.position = slide.targetCameraTransform.position;
        presentationCamera.transform.rotation = slide.targetCameraTransform.rotation;
    }

    private void TransitionCameraToSlide(PresentationSlide slide)
    {
        if (presentationCamera == null || slide.targetCameraTransform == null) return;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);
            
        CleanupPreviousTransition();

        cameraTransitionCoroutine = StartCoroutine(CameraTransitionRoutine(slide));
    }

    private IEnumerator CameraTransitionRoutine(PresentationSlide slide)
    {
        Transform target = slide.targetCameraTransform;
        if (target == null) yield break;

        Vector3 startPos = presentationCamera.transform.position;
        Quaternion startRot = presentationCamera.transform.rotation;
        
        // CACHE target positions BEFORE modifying anything to prevent instant cut bugs
        Vector3 finalMainTargetPos = target.position;
        Quaternion finalMainTargetRot = target.rotation;
        
        // Find all active cameras in the incoming slide
        Camera[] allSlideCameras = slide.GetComponentsInChildren<Camera>(true);

        foreach (var c in allSlideCameras)
        {
            // Only animate cameras that are actively contributing to the view
            if (c.gameObject.activeInHierarchy && c.enabled)
            {
                camerasInTransition.Add(new CameraState 
                { 
                    cam = c, 
                    targetPos = c.transform.position, 
                    targetRot = c.transform.rotation 
                });
                
                // Snap them to the start position immediately so they don't flash at their target position
                c.transform.position = startPos;
                c.transform.rotation = startRot;
            }
        }

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / cameraTransitionDuration;
            float curveT = cameraTransitionCurve.Evaluate(Mathf.Clamp01(t));

            // Move main presentation camera
            presentationCamera.transform.position = Vector3.Lerp(startPos, finalMainTargetPos, curveT);
            presentationCamera.transform.rotation = Quaternion.Slerp(startRot, finalMainTargetRot, curveT);
            
            // Move slide's internal cameras
            foreach (var state in camerasInTransition)
            {
                if (state.cam != null)
                {
                    state.cam.transform.position = Vector3.Lerp(startPos, state.targetPos, curveT);
                    state.cam.transform.rotation = Quaternion.Slerp(startRot, state.targetRot, curveT);
                }
            }

            yield return null;
        }

        // Final snap for exact precision
        presentationCamera.transform.position = finalMainTargetPos;
        presentationCamera.transform.rotation = finalMainTargetRot;
        
        CleanupPreviousTransition();
    }
}
