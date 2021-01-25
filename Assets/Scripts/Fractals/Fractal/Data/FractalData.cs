using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =("Fractal/FractalData"))]
public class FractalData : ScriptableObject
{
    [Range(1, 8)]
    public int depth;
    public Mesh mesh;
    public Material material;

    Vector3[] directions = {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

    Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
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
