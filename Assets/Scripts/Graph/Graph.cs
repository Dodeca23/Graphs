using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    #region Fields

    enum TransitionMode { Cycle, Random }

    [Tooltip("Prefab of a single point of a graph.")]
    [SerializeField]
    private Transform pointPrefab = default;
    [Tooltip("Height and width of the graph.")]
    [SerializeField, Range(10, 100)]
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


    private FunctionLibrary function;
    private FunctionLibrary previousFunction;
    private FunctionLibrary currentFunction;

    private Transform[] points;
    private float duration;
    private int currentIndex;
    private bool transitioning;

    #endregion

    #region MonoBehaviors

    private void OnEnable()
    {
        currentIndex = 0;
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab) as Transform;
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
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

        if(function != null)
        {
            if (transitioning)
                UpdateFunctionTransition();
            else
                UpdateFunction();
        }

    }

    #endregion

    #region Updating
    private void UpdateFunction()
    {
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = (Vector3)function.GetFunction(u, v, time);
        }
    }

    private void UpdateFunctionTransition()
    {
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            Vector3 fromFunction = (Vector3)previousFunction.GetFunction(u, v, time);
            Vector3 toFunction = (Vector3)currentFunction.GetFunction(u, v, time);
            points[i].localPosition = (Vector3)function.Morph(u, v, time, progress, fromFunction, toFunction);
        }
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
        //Debug.Log("Before picking new: " + currentFunction);
        function = transitionMode == TransitionMode.Cycle ?
            GetFunction(currentIndex) : GetRandomFunction();
        currentFunction = function;
        //Debug.Log("After picking: " + currentFunction);
    }

    #endregion

}
