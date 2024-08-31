using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour {

	[SerializeField]
	ComputeShader computeShader;

	[SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

	[SerializeField, Range(10, 200)]
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

	ComputeBuffer positionsBuffer;
	static readonly int 
		positionsId = Shader.PropertyToID("_Positions"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
		timeId = Shader.PropertyToID("_Time");


	void OnEnable()
	{
		positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
	}

	void OnDisable () {
		positionsBuffer.Release();
		positionsBuffer = null;
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

		UpdateFunctionOnGPU();
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

	void UpdateFunctionOnGPU()
	{
		float step = 2f / resolution;
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetFloat(timeId, Time.time);

		computeShader.SetBuffer(0, positionsId, positionsBuffer);
		
		int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(0, groups, groups, 1);

		material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);
		
		var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
		Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
	}
}