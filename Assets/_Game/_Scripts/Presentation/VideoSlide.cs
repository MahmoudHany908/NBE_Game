using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Dead-simple component: plays a single VideoPlayer when this slide enters,
/// stops it when the slide exits. No coroutines, no fancy logic.
/// Each video should be on its own canvas/slide.
/// </summary>
public class VideoSlide : PresentationSlide
{
    [Tooltip("The VideoPlayer to control. Auto-detected if not assigned.")]
    public VideoPlayer videoPlayer;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
    }

    public override void EnterSlide()
    {
        gameObject.SetActive(true);
        OnSlideEnter?.Invoke();

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.playOnAwake = false;
            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.Prepare();
            Debug.Log($"[VideoSlide] Preparing video: {videoPlayer.name} on {gameObject.name}");
        }
    }

    private void OnPrepareCompleted(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepareCompleted;
        Debug.Log($"[VideoSlide] Playing video: {vp.name}");
        vp.Play();
    }

    public override void ExitSlide()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.Stop();
        }
        gameObject.SetActive(false);
        OnSlideExit?.Invoke();
    }
}
