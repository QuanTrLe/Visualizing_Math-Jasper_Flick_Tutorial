using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {

    [SerializeField]
	Transform pointPrefab; //for dot with material

	Transform[] points; //all the dots beign used

	[SerializeField, Range(10, 100)]
	int resolution = 10; //how many dots are being used to represent the curve
	
    
	void Awake () {
        var position = Vector3.zero;
        float step = 2f / resolution;
		var scale = Vector3.one * step;

		points = new Transform[resolution];

		for (int i = 0; i < points.Length; i++) {
			Transform point = points[i] = Instantiate(pointPrefab);
            point.SetParent(transform, false); //for organization put it under Graph node

			position.x = (i + 0.5f) * step - 1f; //to set it in the right position in the midle of screen

			point.localPosition = position;
			point.localScale = scale;
		}
	}

	void Update()
	{
		float time = Time.time; //for sin wave function and avoid redundancy in loop

		for (int i = 0; i < points.Length; i++)
		{
			Transform point = points[i];
			Vector3 position = point.localPosition; 

			position.y = FunctionLibrary.MultiWave(position.x, time);
			point.localPosition = position;
		}
	}
}