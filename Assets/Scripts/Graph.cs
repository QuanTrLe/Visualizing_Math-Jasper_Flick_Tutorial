using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {

    [SerializeField]
	Transform pointPrefab; //for dot with material

	Transform[] points; //all the dots beign used

	[SerializeField, Range(10, 100)]
	int resolution = 10; //how many dots are being used to represent the graph

	[SerializeField]
	FunctionLibrary.FunctionName function; //which functions to use and visualize the graph
	
    
	void Awake () {
        var position = Vector3.zero;
        float step = 2f / resolution;
		var scale = Vector3.one * step;

		points = new Transform[resolution * resolution];

		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
			if (x == resolution)
			{
				x = 0;
				z += 1;
			}

			Transform point = points[i] = Instantiate(pointPrefab);
            point.SetParent(transform, false); //for organization put it under Graph node

			position.x = (x + 0.5f) * step - 1f; //to set it in the right position in the midle of screen
			position.z = (z + 0.5f) * step - 1f; //same as above but for the z coordinate

			point.localPosition = position;
			point.localScale = scale;
		}
	}

	void Update()
	{
		FunctionLibrary.Function f = FunctionLibrary.GetFunction(function); //which function to use to graph
		float time = Time.time; //for sin wave function and avoid redundancy in loop
		float step = 2f / resolution;

		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
		{
			if (x == resolution)
			{
				x = 0;
				z += 1;
			}

			float u = (x + 0.5f) * step - 1f;
			float v = (z + 0.5f) * step - 1f;

			points[i].localPosition = f(u, v, time);
		}
	}
}