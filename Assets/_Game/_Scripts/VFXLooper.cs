using UnityEngine;
using UnityEngine.VFX;

public class VFXLooper : MonoBehaviour
{
    private VisualEffect vfx;

    [Tooltip("Time in seconds between each VFX play")]
    [Range(0.1f, 5.0f)]
    public float loopInterval = 0.5f;
    
    private float timer;

    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        timer = loopInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (vfx != null)
            {
                vfx.Play();
            }
            timer = loopInterval;
        }
    }
}
