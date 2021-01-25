using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

using Random = UnityEngine.Random;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public class OrganicFractal : MonoBehaviour
{
    #region Fields

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {
        public float deltaTime;
        public float scale;
        [ReadOnly]
        public NativeArray<FractalPart> parents;
        public NativeArray<FractalPart> parts;
        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i)
        {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
            part.spinAngle += part.spinVelocity * deltaTime;

            float3 upAxis =
                mul(mul(parent.worldRotation, part.rotation), up());
            float3 sagAxis = cross(up(), upAxis);
            float sagMagnitude = length(sagAxis);
            quaternion baseRotation;
            if (sagMagnitude > 0f)
            {
                sagAxis /= sagMagnitude;
                quaternion sagRotation = quaternion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitude);
                baseRotation = mul(sagRotation, parent.worldRotation);
            }
            else
                baseRotation = parent.worldRotation;

            part.worldRotation =
                mul(baseRotation,
                mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition =
                parent.worldPosition +
                mul(part.worldRotation, float3(0f, 1.5f * scale, 0f));
            parts[i] = part;
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    private static readonly int colorAID = Shader.PropertyToID("_ColorA");
    private static readonly int colorBID = Shader.PropertyToID("_ColorB");
    private static readonly int matricesID = Shader.PropertyToID("_Matrices");
    private static readonly int sequenceNumbersID = Shader.PropertyToID("_SequenceNumbers");
    private static MaterialPropertyBlock propertyBlock;

    [SerializeField]
    private OrganicData organicData = default;

    private Vector4[] sequenceNumbers;

    private NativeArray<FractalPart>[] parts;
    private NativeArray<float3x4>[] matrices;
    private ComputeBuffer[] matricesBuffer;


    #endregion

    #region MonoBehaviors

    private void OnEnable()
    {
        if (organicData.depth < 3)
            organicData.depth = 3;
        parts = new NativeArray<FractalPart>[organicData.depth];
        matrices = new NativeArray<float3x4>[organicData.depth];
        matricesBuffer = new ComputeBuffer[organicData.depth];
        sequenceNumbers = new Vector4[organicData.depth];

        int stride = 12 * 4;
        for(int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffer[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }

        parts[0][0] = organicData.CreatePart(0);
        
        for(int li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi+=5)
            {
                for(int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = organicData.CreatePart(ci);
                }
            }
        }

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffer.Length; i++)
        {
            matricesBuffer[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }

        parts = null;
        matrices = null;
        matricesBuffer = null;
        sequenceNumbers = null;
        organicData.depth = 6;
    }

    private void OnValidate()
    {
        if(parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += rootPart.spinVelocity * deltaTime;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle))
        );
        rootPart.worldPosition = transform.position;

        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            jobHandle = new UpdateFractalLevelJob
            {
                deltaTime = deltaTime,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 1, jobHandle);

            jobHandle.Complete();
        }

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        int leafIndex = matricesBuffer.Length - 1;
        for (int i = 0; i < matricesBuffer.Length; i++)
        {
            Color colorA, colorB;
            Mesh instancedMesh;
            if(i == leafIndex)
            {
                colorA = organicData.leafColorA;
                colorB = organicData.leafColorB;
                instancedMesh = organicData.leafMesh;
            }
            else
            {
                float gradientInterpolator = i / (matricesBuffer.Length - 2f);
                colorA = organicData.gradientA.Evaluate(gradientInterpolator);
                colorB = organicData.gradientB.Evaluate(gradientInterpolator);
                instancedMesh = organicData.mesh;
            }
            ComputeBuffer buffer = matricesBuffer[i];
            propertyBlock.SetColor(colorAID, colorA);
            propertyBlock.SetColor(colorBID, colorB);
            buffer.SetData(matrices[i]);
            propertyBlock.SetBuffer(matricesID, buffer);
            propertyBlock.SetVector(sequenceNumbersID, sequenceNumbers[i]);
            Graphics.DrawMeshInstancedProcedural(
                instancedMesh, 0, organicData.material, bounds, buffer.count, propertyBlock);
        }
    }

    #endregion
}

