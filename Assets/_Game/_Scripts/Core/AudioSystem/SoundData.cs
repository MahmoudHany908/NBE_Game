using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundData", menuName = "Scriptable Objects/SoundData", order = 50)]
public class SoundData : ScriptableObject
{
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    public bool loop;

    public AudioClip GetClip() => clips[Random.Range(0, clips.Length)];
}