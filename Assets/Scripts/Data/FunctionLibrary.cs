using UnityEngine;
using static UnityEngine.Mathf;

[CreateAssetMenu(menuName =("FunctionLibrary/Function"))]
public class FunctionLibrary : ScriptableObject
{
    public enum Functions
    {
        Wave, MultiWave, Ripple
    };

    public Functions function;

    public float? GetFunction(float x, float z,float t)
    {
        switch (function)
        {
            case Functions.Wave:
                return Wave(x, z, t);
            case Functions.MultiWave:
                return MultiWave(x, z, t);
            case Functions.Ripple:
                return Ripple(x, z, t);
            default:
                Debug.LogError("No function assigned.");
                return null;
        }
    }

    public float Wave(float x, float z, float t) =>
        Sin(PI * (x + z + t));

    public float MultiWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + 0.5f * t));
        y += 0.5f * Sin(2f * PI * (z + t));
        y += Sin(PI * (x + z + 0.25f * t));

        return y * (1f / 2.5f);
    }

    public float Ripple(float x, float z, float t)
    {
        float d = Sqrt(Pow(x, 2) + Pow(z, 2));
        float y = Sin(PI * (4f * d - t));

        return y / (1f + 10f * d);
    }
    
}
