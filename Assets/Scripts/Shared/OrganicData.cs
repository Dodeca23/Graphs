using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName =("Fractal/OrganicData"))]
public class OrganicData : FractalData
{
    public Gradient gradientA;
    public Gradient gradientB;
    public Color leafColorA;
    public Color leafColorB;
    [Range(0f, 90f)]
    public float maxSagAngleA, maxSagAngleB;
    [Range(0f, 90f)]
    public float spinSpeedA, spinSpeedB;
    [Range(0f, 1f)]
    public float reversedSpinChance;

    public override FractalPart CreatePart(int childIndex)
    {
        return new FractalPart
        {
            maxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
            rotation = rotations[childIndex],
            spinVelocity = 
                (Random.value < reversedSpinChance ? -1f : 1f) *
                radians(Random.Range(spinSpeedA, spinSpeedB))
        };
    }

}
