using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CyberpunkNeonFlicker : MonoBehaviour
{
    private Image img;
    
    [Header("Neon Settings")]
    [Tooltip("Default color is #35B7D0 as requested")]
    public Color neonColor = new Color(0.2078f, 0.7176f, 0.8156f, 1f);
    public float dimAlpha = 0.1f;
    public float brightAlpha = 1.0f;
    
    [Header("Flicker Settings")]
    public float flickerSpeed = 15f;
    [Range(0f, 1f)] 
    public float flickerThreshold = 0.5f;
    
    [Header("Audio")]
    [Tooltip("Assign your unstable electricity SFX AudioClip here. The AudioSource is automatically managed.")]
    public AudioSource electricitySFX;
    
    void Start()
    {
        img = GetComponent<Image>();
        img.color = neonColor;

        // If no AudioSource is assigned, try to get one on this object
        if (electricitySFX == null)
        {
            electricitySFX = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Use Perlin noise to create a randomized, organic flickering effect like an old lamp
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        
        if (noise > flickerThreshold)
        {
            // Light ON
            SetAlpha(brightAlpha);
            
            if (electricitySFX != null && !electricitySFX.isPlaying)
            {
                electricitySFX.Play();
            }
        }
        else
        {
            // Light OFF (or very dim)
            SetAlpha(dimAlpha);
            
            if (electricitySFX != null && electricitySFX.isPlaying)
            {
                electricitySFX.Pause();
            }
        }
    }
    
    private void SetAlpha(float alpha)
    {
        Color c = neonColor;
        c.a = alpha;
        img.color = c;
    }
}
