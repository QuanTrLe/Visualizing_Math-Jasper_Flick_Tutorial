using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {

    [SerializeField]
	Transform pointPrefab;

	Transform[] points;

	[SerializeField, Range(10, 100)]
	int resolution = 10;
	
    
	void Awake () {
        var position = Vector3.zero;
        float step = 2f / resolution;
		var scale = Vector3.one * step;

		points = new Transform[resolution];

		for (int i = 0; i < points.Length; i++) {
			Transform point = points[i] = Instantiate(pointPrefab);
            point.SetParent(transform, false);

			position.x = (i + 0.5f) * step - 1f;

			point.localPosition = position;
			point.localScale = scale;
		}
	}

	void Update()
	{
		float time = Time.time;

		for (int i = 0; i < points.Length; i++)
		{
			Transform point = points[i];
			Vector3 position = point.localPosition;

			position.y = Mathf.Sin(Mathf.PI * (position.x + time));
			point.localPosition = position;
		}
	}
}