using UnityEngine;
using static UnityEngine.Mathf;

[CreateAssetMenu(menuName =("FunctionLibrary/Function"))]
public class FunctionLibrary : ScriptableObject
{
    #region Fields

    public enum Functions
    {
        Wave, MultiWave, Ripple, Sphere, Torus
    };

    public Functions function;

    #endregion

    #region Public Get Method
    public Vector3? GetFunction(float u, float v,float t)
    {
        switch (function)
        {
            case Functions.Wave:
                return Wave(u, v, t);
            case Functions.MultiWave:
                return MultiWave(u, v, t);
            case Functions.Ripple:
                return Ripple(u, v, t);
            case Functions.Sphere:
                return Sphere(u, v, t);
            case Functions.Torus:
                return Torus(u, v, t);
            default:
                Debug.LogError("No function assigned.");
                return null;
        }
    }


    #endregion

    #region Private Functions

    private Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;

        return p;
    }

    private Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * PI * (v + t));
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= 1 / 2.5f;
        p.z = v;

        return p;
    }

    private Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(Pow(u, 2) + Pow(v, 2));

        Vector3 p;
        p.x = u;
        p. y = Sin(PI * (4f * d - t));
        p.y /= (1f + 10f * d);
        p.z = v;

        return p;
    }

    private Vector3 Sphere(float u, float v,float t)
    {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(PI * 0.5f * v);
        p.z = s * Cos(PI * u);

        return p;
    }

    private Vector3 Torus(float u, float v, float t)
    {
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);

        return p;
    }

    public Vector3 Morph(float u, float v, float t, float progress = 0f, Vector3? from = null , Vector3? to = null) =>
        Vector3.LerpUnclamped((Vector3)from, (Vector3)to, SmoothStep(0f, 1f, (float)progress));
    

    #endregion

    
}
