using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class VideoSlideSequence : PresentationSlide
{
    public List<VideoPlayer> sequenceVideos;
    private int currentVideoIndex = -1;

    public override void EnterSlide()
    {
        base.EnterSlide();
        currentVideoIndex = 0;
        PlayVideo(currentVideoIndex);
    }

    public override void ExitSlide()
    {
        base.ExitSlide();
        StopAllVideos();
        currentVideoIndex = -1;
    }

    public override bool TryNextInnerStep()
    {
        if (currentVideoIndex < sequenceVideos.Count - 1)
        {
            currentVideoIndex++;
            PlayVideo(currentVideoIndex);
            return true; // We handled the input, don't change slide yet
        }
        return false; // Reached the end, let Manager change slide
    }

    public override bool TryPreviousInnerStep()
    {
        if (currentVideoIndex > 0)
        {
            currentVideoIndex--;
            PlayVideo(currentVideoIndex);
            return true; // We handled the input, don't change slide yet
        }
        return false; // Reached the beginning, let Manager change slide
    }

    private void PlayVideo(int index)
    {
        StopAllVideos();
        if (index >= 0 && index < sequenceVideos.Count)
        {
            var vp = sequenceVideos[index];
            if (vp != null)
            {
                vp.gameObject.SetActive(true);
                vp.Play();
            }
        }
    }

    private void StopAllVideos()
    {
        foreach (var vp in sequenceVideos)
        {
            if (vp != null)
            {
                vp.Stop();
                vp.gameObject.SetActive(false); // Hide the video plane/UI when not playing
            }
        }
    }
}
