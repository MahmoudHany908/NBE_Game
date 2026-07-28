using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayCurrentScene
{
    static PlayCurrentScene()
    {
        // Ensures Unity plays from whatever scene is currently open in the hierarchy
        EditorSceneManager.playModeStartScene = null;
    }

    [MenuItem("Tools/Play From Current Scene %&p")] // Shortcut: Ctrl + Alt + P (Cmd + Option + P on Mac)
    public static void PlayFromCurrent()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        // Save open scenes first so you don't lose unsaved changes
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // Clear any forced start scene override
            EditorSceneManager.playModeStartScene = null;
            
            // Enter Play Mode
            EditorApplication.isPlaying = true;
        }
    }
}