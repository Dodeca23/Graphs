using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

#region FractalPart Struct

public struct FractalPart
{
    public float3 direction;
    public float3 worldPosition;
    public quaternion rotation;
    public quaternion worldRotation;
    public float spinAngle;
}

#endregion

public class Fractal : MonoBehaviour
{
    #region Fields

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {
        public float spinAngleDelta;
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
            part.spinAngle += spinAngleDelta;
            part.worldRotation =
                mul(parent.worldRotation,
                mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition =
                parent.worldPosition +
                mul(parent.worldRotation, 1.5f * scale * part.direction);
            parts[i] = part;
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    private static readonly int matricesID = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock propertyBlock;

    [SerializeField]
    private FractalData fractalData = default;

    private NativeArray<FractalPart>[] parts;
    private NativeArray<float3x4>[] matrices;
    private ComputeBuffer[] matricesBuffer;


    #endregion

    #region MonoBehaviors

    private void OnEnable()
    {
        parts = new NativeArray<FractalPart>[fractalData.depth];
        matrices = new NativeArray<float3x4>[fractalData.depth];
        matricesBuffer = new ComputeBuffer[fractalData.depth];

        int stride = 12 * 4;
        for(int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffer[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = fractalData.CreatePart(0);
        
        for(int li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi+=5)
            {
                for(int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = fractalData.CreatePart(ci);
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
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
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
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 1, jobHandle);

            jobHandle.Complete();
        }

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for (int i = 0; i < matricesBuffer.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffer[i];
            buffer.SetData(matrices[i]);
            propertyBlock.SetBuffer(matricesID, buffer);
            Graphics.DrawMeshInstancedProcedural(
                fractalData.mesh, 0, fractalData.material, bounds, buffer.count, propertyBlock);
        }
    }

    #endregion
}

