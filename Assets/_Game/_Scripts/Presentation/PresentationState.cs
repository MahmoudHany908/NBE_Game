namespace VaultSurvivors.Presentation
{
    /// <summary>
    /// Global persistent state to track presentation progression across Unity scene loads.
    /// </summary>
    public static class PresentationState
    {
        /// <summary>
        /// True if we just returned from a sub-presentation scene (like the Arena) 
        /// and need to instantly snap to the saved slide index in the Hub.
        /// </summary>
        public static bool IsReturningFromSubscene = false;

        /// <summary>
        /// The name of the scene that the sub-presentation should return to when finished.
        /// </summary>
        public static string ReturnSceneName = "";

        /// <summary>
        /// The index of the slide that the main presentation was on when it left for the sub-scene.
        /// </summary>
        public static int ReturnSlideIndex = 0;

        /// <summary>
        /// If true, the new scene should start completely black and fade in.
        /// </summary>
        public static bool NeedsFadeIn = false;

        /// <summary>
        /// The duration of the fade in effect when loading the new scene.
        /// </summary>
        public static float FadeInDuration = 1f;
    }
}
