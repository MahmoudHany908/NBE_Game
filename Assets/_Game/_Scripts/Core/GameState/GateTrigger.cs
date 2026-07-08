using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "02_Arena_Sandbox";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            // Find the SettingsMenu in the scene and call the LoadAreana function
            SettingsMenu settingsMenu = FindFirstObjectByType<SettingsMenu>();
            if (settingsMenu != null)
            {
                settingsMenu.LoadAreana(targetSceneName);
            }
            else
            {
                Debug.LogError("SettingsMenu not found in the scene!");
            }
        }
    }
}
