using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FractalPart
{
    public Vector3 direction;
    public Quaternion rotation;
    public Transform transform;
}

public class Fractal : MonoBehaviour
{   

    [SerializeField]
    private FractalData fractalData = default;

    private FractalPart[][] parts;
    
    private void OnEnable()
    {
        parts = new FractalPart[fractalData.depth][];
        int size = 1;
        for(int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
        }

        float scale = 1f;
        parts[0][0] = fractalData.CreatePart(transform, 0, 0, scale);

        for(int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            for(int fpi = 0; fpi < levelParts.Length; fpi+=5)
            {
                for(int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = fractalData.CreatePart(transform, li, ci, scale);
                }
            }
        }
    }

    private void Update()
    {
        for (int li = 1; li < parts.Length; li++)
        {
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                Transform parentTransform = parentParts[fpi / 5].transform;
                FractalPart part = levelParts[fpi];
                part.transform.localRotation = 
                    parentTransform.localRotation * part.rotation;
                part.transform.localPosition =
                    parentTransform.localPosition +
                    parentTransform.localRotation *
                    (1.5f * part.transform.localScale.x * part.direction);
            }
        }
    }
}

