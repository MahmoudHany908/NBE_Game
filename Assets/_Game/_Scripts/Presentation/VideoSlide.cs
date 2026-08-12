using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoSlide : PresentationSlide
{
    [Tooltip("The Video Player that this slide will play.")]
    public VideoPlayer videoPlayer;
    
    private Coroutine playCoroutine;

    public override void EnterSlide()
    {
        base.EnterSlide();
        if (videoPlayer != null)
        {
            playCoroutine = StartCoroutine(PrepareAndPlay(videoPlayer));
        }
    }

    public override void ExitSlide()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            
            // Clear texture to avoid stuck frames
            RawImage rawImg = videoPlayer.GetComponentInParent<RawImage>();
            if (rawImg == null) rawImg = videoPlayer.GetComponentInChildren<RawImage>(true); 
            if (rawImg != null && rawImg.texture == videoPlayer.texture)
            {
                rawImg.texture = null;
            }
        }
        
        base.ExitSlide();
    }

    private IEnumerator PrepareAndPlay(VideoPlayer vp)
    {
        vp.enabled = true;
        vp.gameObject.SetActive(true);
        yield return null; 
        
        vp.renderMode = VideoRenderMode.APIOnly;
        vp.targetTexture = null; 
        
        vp.Prepare();
        
        float timeout = 10f;
        float elapsed = 0f;
        while (!vp.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!vp.isPrepared)
        {
            Debug.LogWarning($"[VideoSlide] Video {vp.name} failed to prepare after {timeout}s!");
            yield break;
        }
        
        RawImage rawImg = vp.GetComponentInParent<RawImage>();
        if (rawImg == null) rawImg = vp.GetComponentInChildren<RawImage>(true); 
        
        if (rawImg != null)
        {
            rawImg.enabled = true;
            rawImg.texture = vp.texture; 
            rawImg.SetMaterialDirty();
            rawImg.SetAllDirty();
        }

        vp.Play();
    }
}
