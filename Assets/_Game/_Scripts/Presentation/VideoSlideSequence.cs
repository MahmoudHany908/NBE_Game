using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VideoSlideSequence : PresentationSlide
{
    public List<VideoPlayer> sequenceVideos;
    private int currentVideoIndex = -1;
    private Coroutine playCoroutine;

    public override void EnterSlide()
    {
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
        
        // THE FIX: Do not use RenderTextures at all. 
        // Force the VideoPlayer to decode directly to an internal texture (APIOnly)
        // This avoids ALL URP RenderTexture sharing and memory leak issues.
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
            Debug.LogWarning($"[VideoSlideSequence] Video {vp.name} failed to prepare after {timeout}s!");
            yield break;
        }
        
        // Get the RawImage
        RawImage rawImg = vp.GetComponentInParent<RawImage>();
        if (rawImg == null) rawImg = vp.GetComponentInChildren<RawImage>(true); 
        
        if (rawImg != null)
        {
            rawImg.enabled = true;
            // vp.texture is the internal decoded frame buffer (available after Prepare)
            rawImg.texture = vp.texture; 
            
            // Force Canvas to redraw with the new texture
            rawImg.SetMaterialDirty();
            rawImg.SetAllDirty();
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
    }

    private void StopAllVideos()
    {
        foreach (var vp in sequenceVideos)
        {
            if (vp != null)
            {
                vp.Stop();
                
                // Clear texture to avoid stuck frames
                RawImage rawImg = vp.GetComponentInParent<RawImage>();
                if (rawImg == null) rawImg = vp.GetComponentInChildren<RawImage>(true); 
                if (rawImg != null && rawImg.texture == vp.texture)
                {
                    rawImg.texture = null;
                }
            }
        }
    }
}
