using System.Collections.Generic;
using UnityEngine;

public class CPUGraph : MonoBehaviour
{
    #region Fields

    enum TransitionMode { Cycle, Random }

    private const int maxResolution = 1000;

    [Tooltip("Computeshader used by the CPU Graph.")]
    [SerializeField]
    private ComputeShader computeShader = default;
    [Tooltip("Material used for the mesh.")]
    [SerializeField]
    private Material material = default;
    [Tooltip("Mesh used to draw the objects.")]
    [SerializeField]
    private Mesh mesh = default;
    [Tooltip("Height and width of the graph.")]
    [SerializeField, Range(10, maxResolution)]
    private int resolution = 10;
    [Tooltip("Collection of the available functions.")]
    [SerializeField]
    private FunctionLibrary[] functions = default;
    [Tooltip("Display time of one function.")]
    [SerializeField]
    private float functionDuration = 1f;
    [Tooltip("Time to transform from one function to another.")]
    [SerializeField]
    private float transitionDuration = 1f;
    [Tooltip("Loops through the functions random or cycles through them.")]
    [SerializeField]
    private TransitionMode transitionMode = TransitionMode.Cycle;

    private static readonly int
        positionsID = Shader.PropertyToID("_Positions"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time"),
        transitionProgressiD = Shader.PropertyToID("_TransitionProgress");

    private ComputeBuffer positionsBuffer;

    private FunctionLibrary function;
    private FunctionLibrary previousFunction;
    private FunctionLibrary currentFunction;

    private float duration;
    private int currentIndex;
    private bool transitioning;

    #endregion

    #region MonoBehaviors

    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        
        duration += Time.deltaTime;
        if (transitioning)
        {
            if(duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            currentFunction = functions[currentIndex];
            previousFunction = currentFunction;

            duration -= functionDuration;
            transitioning = true;

            if (currentIndex == functions.Length - 1)
                currentIndex = 0;
            else
                currentIndex++;
            
            PickNextFunction();
            currentFunction = function;
        }

        UpdateFunctionOnGPU();
    }

    #endregion    

    #region Function Returns
    private FunctionLibrary GetFunction(int index) =>
        functions[index];

    private FunctionLibrary GetRandomFunction()
    {
        int choice = Random.Range(1, functions.Length);

        return choice == currentIndex ? functions[0] : functions[choice];
    }

    private void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            GetFunction(currentIndex) : GetRandomFunction();
        currentFunction = function;
    }

    #endregion

    #region Compute Shader

    private void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionID, resolution);
        computeShader.SetFloat(stepID, step);
        computeShader.SetFloat(timeID, Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(
                transitionProgressiD,
                Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }


        int kernelIndex = function.indexNumber + (int)(transitioning ?
        previousFunction.indexNumber : currentFunction.indexNumber) * function.GetFunctionCount();
        computeShader.SetBuffer(kernelIndex, positionsID, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionsID, positionsBuffer);
        material.SetFloat(stepID, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    #endregion

}
