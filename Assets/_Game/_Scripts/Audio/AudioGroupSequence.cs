using System.Collections;
using UnityEngine;

namespace VaultSurvivors.Audio
{
    [System.Serializable]
    public class AudioClipEntry
    {
        public AudioClip clip;
        [Range(0f, 2f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
    }

    [System.Serializable]
    public class AudioGroup
    {
        [Tooltip("These clips will all play at the exact same time.")]
        public AudioClipEntry[] clips;
        
        [Tooltip("Optional delay after this group finishes playing before the next group starts.")]
        public float delayAfterGroup = 0f;
    }

    public class AudioGroupSequence : MonoBehaviour
    {
        [Header("Sequence Settings")]
        [Tooltip("The list of groups to play in order. Each group plays its clips simultaneously.")]
        public AudioGroup[] sequenceGroups;
        
        public bool playOnStart = true;
        public bool loopSequence = false;

        private Coroutine _sequenceRoutine;
        private System.Collections.Generic.List<AudioSource> _activeSources = new System.Collections.Generic.List<AudioSource>();

        private void Start()
        {
            if (playOnStart)
            {
                PlaySequence();
            }
        }

        public void PlaySequence()
        {
            StopSequence();
            if (gameObject.activeInHierarchy)
            {
                _sequenceRoutine = StartCoroutine(SequenceRoutine());
            }
        }

        public void StopSequence()
        {
            if (_sequenceRoutine != null)
            {
                StopCoroutine(_sequenceRoutine);
                _sequenceRoutine = null;
            }

            // Clean up any active audio sources
            foreach (var src in _activeSources)
            {
                if (src != null)
                {
                    src.Stop();
                    Destroy(src);
                }
            }
            _activeSources.Clear();
        }

        private IEnumerator SequenceRoutine()
        {
            do
            {
                for (int i = 0; i < sequenceGroups.Length; i++)
                {
                    AudioGroup currentGroup = sequenceGroups[i];
                    float maxClipDuration = 0f;

                    // Clean up nulls from the list just in case
                    _activeSources.RemoveAll(s => s == null);

                    // Play all clips in the current group simultaneously
                    if (currentGroup.clips != null)
                    {
                        foreach (AudioClipEntry entry in currentGroup.clips)
                        {
                            if (entry != null && entry.clip != null)
                            {
                                // Dynamically create an AudioSource for this specific clip to support per-clip settings
                                AudioSource src = gameObject.AddComponent<AudioSource>();
                                src.clip = entry.clip;
                                src.volume = entry.volume;
                                src.pitch = entry.pitch;
                                src.playOnAwake = false;
                                src.Play();

                                _activeSources.Add(src);

                                // Automatically destroy the AudioSource component after it finishes playing
                                float actualDuration = entry.clip.length;
                                if (Mathf.Abs(entry.pitch) > 0.01f)
                                {
                                    actualDuration /= Mathf.Abs(entry.pitch);
                                }
                                
                                Destroy(src, actualDuration);

                                // Find the longest clip to know when this group is officially "done"
                                if (actualDuration > maxClipDuration)
                                {
                                    maxClipDuration = actualDuration;
                                }
                            }
                        }
                    }

                    // Wait for the longest clip to finish playing
                    if (maxClipDuration > 0f)
                    {
                        yield return new WaitForSeconds(maxClipDuration);
                    }

                    // Optional gap between groups
                    if (currentGroup.delayAfterGroup > 0f)
                    {
                        yield return new WaitForSeconds(currentGroup.delayAfterGroup);
                    }
                }

            } while (loopSequence);
        }
    }
}
