using UnityEngine;

namespace VaultSurvivors.LoadingSystem
{
    public static class UIFadeUtility
    {
        public static async Awaitable FadeIn(CanvasGroup group, float duration)
        {
            if (group == null) return;

            group.blocksRaycasts = true;
            group.interactable = true;

            float elapsedTime = 0f;
            float startAlpha = group.alpha;

            while (elapsedTime < duration)
            {
                if (group == null) return; // Check if destroyed during await

                elapsedTime += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / duration);
                await Awaitable.NextFrameAsync();
            }

            if (group != null)
            {
                group.alpha = 1f;
            }
        }

        public static async Awaitable FadeOut(CanvasGroup group, float duration)
        {
            if (group == null) return;

            group.blocksRaycasts = false;
            group.interactable = false;

            float elapsedTime = 0f;
            float startAlpha = group.alpha;

            while (elapsedTime < duration)
            {
                if (group == null) return; // Check if destroyed during await

                elapsedTime += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
                await Awaitable.NextFrameAsync();
            }

            if (group != null)
            {
                group.alpha = 0f;
            }
        }
    }
}
