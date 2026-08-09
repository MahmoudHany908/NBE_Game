namespace VaultSurvivors.LoadingSystem
{
    /// <summary>
    /// Thin static wrapper. Delegates all async work to LoadingScreenController
    /// (a DontDestroyOnLoad MonoBehaviour) so scene transitions never kill the async pipeline.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>Full loading screen with hints. Use for Hub ↔ Arena transitions.</summary>
        public static void Load(string targetScene, IGameState targetState)
        {
            RunLoad(targetScene, targetState, showHints: true);
        }

        /// <summary>Loading screen without hints. Use for Title → Hub transition.</summary>
        public static void LoadNoHints(string targetScene, IGameState targetState)
        {
            RunLoad(targetScene, targetState, showHints: false);
        }

        private static void RunLoad(string targetScene, IGameState targetState, bool showHints)
        {
            if (LoadingScreenController.Instance == null)
            {
                UnityEngine.Debug.LogError(
                    "[SceneLoader] LoadingScreenController not found! " +
                    "Make sure the LoadingScreen prefab exists in the Boot scene.");
                return;
            }

            // Kick off the sequence on the persistent MonoBehaviour — safe across scene loads
            _ = LoadingScreenController.Instance.RunLoad(targetScene, targetState, showHints);
        }
    }
}
