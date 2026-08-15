using UnityEngine;
using UnityEngine.SceneManagement;
using VaultSurvivors.Presentation;

public class SceneTransitionSlide : PresentationSlide
{
    [Header("Scene Transition Settings")]
    [Tooltip("Check this if this slide is at the END of the sub-presentation (Arena) and should return you to the Hub.")]
    public bool isReturnSlide = false;
    
    [Tooltip("The duration (in seconds) of the fade to black transition when leaving this scene.")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("The duration (in seconds) of the fade in from black transition when arriving in the new scene.")]
    public float fadeInDuration = 0.5f;

    [Tooltip("The exact name of the Unity Scene to load (e.g., 'Presentation2'). This is ignored if isReturnSlide is true.")]
    public string targetSceneName = "Presentation2";
    
    public override void EnterSlide()
    {
        base.EnterSlide();
        
        var pm = Object.FindAnyObjectByType<PresentationManager>();

        if (isReturnSlide)
        {
            // We are at the end of the sub-presentation, go back to where we came from
            if (string.IsNullOrEmpty(PresentationState.ReturnSceneName))
            {
                Debug.LogError("[SceneTransitionSlide] Return scene name is empty! Make sure you didn't start the game directly in this scene.");
                return;
            }
            
            PresentationState.IsReturningFromSubscene = true;
            PresentationState.NeedsFadeIn = true;
            PresentationState.FadeInDuration = fadeInDuration;

            if (pm != null)
                pm.TransitionToScene(PresentationState.ReturnSceneName, fadeOutDuration);
            else
                SceneManager.LoadSceneAsync(PresentationState.ReturnSceneName);
        }
        else
        {
            // We are leaving the main presentation to go to a sub-scene
            if (pm != null)
            {
                PresentationState.ReturnSlideIndex = pm.CurrentSlideIndex;
            }
            else
            {
                Debug.LogWarning("[SceneTransitionSlide] Could not find PresentationManager to save slide index.");
            }
            
            // Save current scene so the sub-scene knows where to return to
            PresentationState.ReturnSceneName = SceneManager.GetActiveScene().name;
            
            PresentationState.NeedsFadeIn = true;
            PresentationState.FadeInDuration = fadeInDuration;

            // Load the target scene with fade
            if (pm != null)
                pm.TransitionToScene(targetSceneName, fadeOutDuration);
            else
                SceneManager.LoadSceneAsync(targetSceneName);
        }
    }
}
