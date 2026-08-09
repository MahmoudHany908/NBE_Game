using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VideoSlideSequence : PresentationSlide
{
    public List<VideoPlayer> sequenceVideos;

    [Header("Diagnostics")]
    [Tooltip("Set to None to test whether audio is stalling video playback (buffer overflow / clock-sync theory).")]
    public VideoAudioOutputMode audioMode = VideoAudioOutputMode.AudioSource;
    [Tooltip("Seconds after Play() to check whether the frame counter is actually advancing.")]
    public float stallCheckDelay = 1.5f;

    private int currentVideoIndex = -1;
    private Coroutine playCoroutine;
    private Coroutine stallCheckCoroutine;
    private readonly Dictionary<VideoPlayer, RenderTexture> dynamicallyCreatedTextures = new Dictionary<VideoPlayer, RenderTexture>();

    public override void EnterSlide()
    {
        Time.timeScale = 1f; // Ensure time is running normally when entering the slide
        base.EnterSlide();
        currentVideoIndex = 0;
        PlayCurrentVideo();
    }

    public override void ExitSlide()
    {
        StopAllVideos();
        currentVideoIndex = -1;

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        if (stallCheckCoroutine != null)
        {
            StopCoroutine(stallCheckCoroutine);
            stallCheckCoroutine = null;
        }

        base.ExitSlide();
    }

    public override bool TryNextInnerStep()
    {
        if (currentVideoIndex < sequenceVideos.Count - 1)
        {
            currentVideoIndex++;
            PlayCurrentVideo();
            return true;
        }
        return false;
    }

    public override bool TryPreviousInnerStep()
    {
        if (currentVideoIndex > 0)
        {
            currentVideoIndex--;
            PlayCurrentVideo();
            return true;
        }
        return false;
    }

    private void PlayCurrentVideo()
    {
        StopAllVideos();

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        if (stallCheckCoroutine != null)
        {
            StopCoroutine(stallCheckCoroutine);
            stallCheckCoroutine = null;
        }

        if (currentVideoIndex >= 0 && currentVideoIndex < sequenceVideos.Count)
        {
            var vp = sequenceVideos[currentVideoIndex];
            if (vp != null)
            {
                playCoroutine = StartCoroutine(PrepareAndPlay(vp));
            }
        }
    }

    private IEnumerator PrepareAndPlay(VideoPlayer vp)
    {
        Debug.Log($"[VideoSlideSequence] Preparing video: {vp.name} on {gameObject.name}");

        vp.enabled = true;
        vp.gameObject.SetActive(true);
        yield return null; // Wait for active state to register

        // RenderTexture must be assigned BEFORE Prepare() or it silently fails to render.
        if (vp.renderMode == VideoRenderMode.RenderTexture && vp.targetTexture == null)
        {
            Debug.LogWarning($"[VideoSlideSequence] Video {vp.name} has RenderTexture mode but no targetTexture assigned! Creating one.");
            RenderTexture rt = new RenderTexture((int)vp.width, (int)vp.height, 0);
            rt.name = $"DynRT_{vp.name}";
            vp.targetTexture = rt;
            dynamicallyCreatedTextures[vp] = rt;
        }

        // Always wire audio explicitly, regardless of what mode was previously set.
        if (audioMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource aSource = vp.gameObject.GetComponent<AudioSource>();
            if (aSource == null)
            {
                aSource = vp.gameObject.AddComponent<AudioSource>();
                aSource.playOnAwake = false;
            }
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.SetTargetAudioSource(0, aSource);
        }
        else
        {
            vp.audioOutputMode = audioMode; // e.g. None, for stall diagnostics
        }

        vp.waitForFirstFrame = true;

        bool prepared = false;
        void OnPrepared(VideoPlayer source) => prepared = true;
        vp.prepareCompleted += OnPrepared;

        vp.Prepare();

        float timeout = 10f;
        float elapsed = 0f;
        while (!prepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        vp.prepareCompleted -= OnPrepared;

        if (!prepared)
        {
            Debug.LogWarning($"[VideoSlideSequence] Video {vp.name} failed to prepare after {timeout}s on {gameObject.name}");
            yield break;
        }

        // Get the RawImage from the parent hierarchy (where the Videos canvas lives)
        RawImage rawImg = vp.GetComponentInParent<RawImage>();
        if (rawImg == null) rawImg = vp.GetComponentInChildren<RawImage>(true);

        if (rawImg != null)
        {
            rawImg.gameObject.SetActive(true);
            rawImg.enabled = true;
            rawImg.texture = vp.targetTexture;
            rawImg.SetMaterialDirty();
            rawImg.SetAllDirty();

            Debug.Log($"[VideoSlideSequence] RawImage '{rawImg.gameObject.name}' texture set to '{(vp.targetTexture != null ? vp.targetTexture.name : "null")}'");
        }
        else
        {
            Debug.LogWarning($"[VideoSlideSequence] No RawImage found for video {vp.name} on {gameObject.name}!");
        }

        // Hide other raw images to avoid overlap
        foreach (var otherVp in sequenceVideos)
        {
            if (otherVp != vp && otherVp != null)
            {
                RawImage otherImg = otherVp.GetComponentInParent<RawImage>();
                if (otherImg != null && otherImg != rawImg)
                {
                    otherImg.enabled = false;
                }
            }
        }

        Debug.Log($"[VideoSlideSequence] Playing video: {vp.name}");
        vp.Play();
        Debug.Log($"[VideoSlideSequence] Play() called. isPlaying={vp.isPlaying}, isPrepared={vp.isPrepared}, texture={vp.texture != null}, targetTexture={vp.targetTexture != null}, audioOutputMode={vp.audioOutputMode}");

        stallCheckCoroutine = StartCoroutine(CheckForStall(vp));
    }

    // Diagnostic: confirms whether the decode clock is actually advancing.
    // If frame stays at 0/1 after stallCheckDelay while isPlaying is true,
    // playback is stalled — commonly caused by an unhealthy audio clock (WMF audio buffer overflow)
    // that the video decode loop is waiting to sync against.
    private IEnumerator CheckForStall(VideoPlayer vp)
    {
        long startFrame = vp.frame;
        yield return new WaitForSeconds(stallCheckDelay);

        if (vp == null) yield break;

        long currentFrame = vp.frame;
        if (vp.isPlaying && currentFrame <= startFrame + 1)
        {
            Debug.LogWarning($"[VideoSlideSequence] STALL DETECTED on {vp.name}: frame stuck at {currentFrame} after {stallCheckDelay}s. " +
                $"timeScale={Time.timeScale}, playbackSpeed={vp.playbackSpeed}, frameRate={vp.frameRate}, frameCount={vp.frameCount}, " +
                $"length={vp.length}, time={vp.time}, canSetTime={vp.canSetTime}, source={vp.source}, url={vp.url}");
        }
        else
        {
            Debug.Log($"[VideoSlideSequence] {vp.name} playback confirmed advancing: frame {startFrame} -> {currentFrame}");
        }
    }

    private void StopAllVideos()
    {
        foreach (var vp in sequenceVideos)
        {
            if (vp != null)
            {
                vp.Stop();

                if (dynamicallyCreatedTextures.ContainsKey(vp))
                {
                    RenderTexture rt = dynamicallyCreatedTextures[vp];
                    if (rt != null)
                    {
                        rt.Release();
                        Destroy(rt);
                    }
                    dynamicallyCreatedTextures.Remove(vp);
                    vp.targetTexture = null;
                }
            }
        }
    }
}