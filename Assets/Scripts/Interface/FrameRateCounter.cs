using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour
{
    [Tooltip("Text component of the framerate display")]
    [SerializeField]
    private TextMeshProUGUI display = default;
    [SerializeField, Range(0.1f, 2f)]
    private float sampleDuration = 1f;

    private int frames;
    private float duration;
    private float bestDuration = float.MaxValue;
    private float worstDuration;

    private void Update()
    {
        float frameDuration = Time.unscaledDeltaTime;
        frames += 1;
        duration += frameDuration;

        if (frameDuration < bestDuration)
            bestDuration = frameDuration;
        if (frameDuration > worstDuration)
            worstDuration = frameDuration;

        if(duration >= sampleDuration)
        {
            display.SetText("FPS\n{0:0}\n{1:0}\n{2:0}", 
                1f / bestDuration, frames / duration, 1f / worstDuration);
            frames = 0;
            duration = 0;
            bestDuration = float.MaxValue;
            worstDuration = 0;
        }
    }
}
