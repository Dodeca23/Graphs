using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FractalPart
{
    public Vector3 direction;
    public Vector3 worldPosition;
    public Quaternion rotation;
    public Quaternion worldRotation;
    public float spinAngle;
}

public class Fractal : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private FractalData fractalData = default;

    private FractalPart[][] parts;
    private Matrix4x4[][] matrices;
    private ComputeBuffer[] matricesBuffer;

    private static readonly int matricesID = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock propertyBlock;

    #endregion

    #region MonoBehaviors

    private void OnEnable()
    {
        parts = new FractalPart[fractalData.depth][];
        matrices = new Matrix4x4[fractalData.depth][];
        matricesBuffer = new ComputeBuffer[fractalData.depth];

        int stride = 16 * 4;
        for(int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffer[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = fractalData.CreatePart(0);
        
        for(int li = 1; li < parts.Length; li++)
        {
            FractalPart[] levelParts = parts[li];
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
        float spinAngleDelta = 22.5f * Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = transform.rotation *
            (rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f));
        rootPart.worldPosition = transform.position;

        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        matrices[0][0] = Matrix4x4.TRS(
            rootPart.worldPosition, rootPart.worldRotation, objectScale * Vector3.one);

        float scale = 1f;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];

            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                FractalPart parent = parentParts[fpi / 5];
                FractalPart part = levelParts[fpi];
                part.spinAngle += spinAngleDelta;
                part.worldRotation = 
                    parent.worldRotation * 
                    (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f));
                part.worldPosition =
                    parent.worldPosition +
                    parent.worldRotation *
                    (1.5f * scale * part.direction);
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(
                    part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
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

