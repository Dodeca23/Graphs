using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [Tooltip("Prefab of a single point of a graph.")]
    [SerializeField]
    private Transform pointPrefab = default;
    [Tooltip("Height and width of the graph.")]
    [SerializeField, Range(10, 100)]
    private int resolution = 10;
    [SerializeField]
    private FunctionLibrary function = default;

    private Transform[] points;

    private void OnEnable()
    {
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
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if(x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = (Vector3)function.GetFunction(u, v, time);
        }
    }
}
