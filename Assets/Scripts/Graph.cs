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

	[SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f; //the duration before cycling to next function

	float duration;
	bool transitioning;

	FunctionLibrary.FunctionName transitionFunction;

	public enum TransitionMode { Cycle, Random } //for choosing which display mode

	[SerializeField]
	TransitionMode transitionMode;
	
    
	void Awake () {
        float step = 2f / resolution;
		var scale = Vector3.one * step;

		points = new Transform[resolution * resolution];

		for (int i = 0; i < points.Length; i++) {
			Transform point = points[i] = Instantiate(pointPrefab);
			point.localScale = scale;
			point.SetParent(transform, false); //for organization put it under Graph node
		}
	}

	void Update()
	{
		duration += Time.deltaTime;

		if (transitioning)
		{
			if (duration >= transitionDuration) 
			{
				duration -= transitionDuration;
				transitioning = false;
			}
		}
		else if (duration >= functionDuration) {
			duration -= functionDuration;

			transitioning = true;
			transitionFunction = function;

			PickNextFunction();
		}

		if (transitioning) {
			UpdateFunctionTransition();
		}
		else {
			UpdateFunction();
		}
	}

	void PickNextFunction () {
		if (transitionMode == TransitionMode.Cycle)
		{
			function = FunctionLibrary.GetNextFunctionName(function);
		}
		else
		{
			function = FunctionLibrary.GetRandomFunctionNameOtherThan(function);
		}
	}

	void UpdateFunctionTransition () {
		FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction);
		FunctionLibrary.Function to = FunctionLibrary.GetFunction(function);

		float progress = duration / transitionDuration;
		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;

		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
			if (x == resolution)
			{
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}

			float u = (x + 0.5f) * step - 1f;

			points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
		}
	}

	void UpdateFunction()
	{
		FunctionLibrary.Function f = FunctionLibrary.GetFunction(function); //which function to use to graph

		float time = Time.time; //for sin wave function and avoid redundancy in loop
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

			points[i].localPosition = f(u, v, time);
		}
	}
}