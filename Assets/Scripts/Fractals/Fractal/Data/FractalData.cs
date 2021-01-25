using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

[CreateAssetMenu(menuName =("Fractal/FractalData"))]
public class FractalData : ScriptableObject
{
    [Range(1, 8)]
    public int depth;
    public Mesh mesh;
    public Material material;

    float3[] directions = {
        up(), right(), left(), forward(), back()
    };

    quaternion[] rotations = {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    public FractalPart CreatePart(int childIndex)
    {
        return new FractalPart
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex]
        };
    }
}
