using UnityEngine;
using static UnityEngine.Mathf;


public static class FunctionLibrary
{
	public static Vector3 Wave(float u, float v, float t)
	{
		Vector3 p;

		p.x = u;
		p.y = Sin(PI * (u + v + t));
		p.z = v;

		return p; //sin wave formula
	}

	public static Vector3 MultiWave(float u, float v, float t)
	{
		Vector3 p;

		p.x = u;

		//constant * for performance
		//to add more complexity to the wave
		p.y = Sin(PI * (u + 0.5f * t)); // the normal sin wave
		p.y += Sin(2f * PI * (v + t)) * 0.5f; //to add on a wave independent for the z dimension
		p.y += Sin(PI * (u + v + 0.25f * t));
		p.y = p.y * (1f / 2.5f); //due to the higher frequence, the range is now -2.5 x 2.5. Make it -1 x 1

		p.z = v;

		return p;
	}

	public static Vector3 Ripple (float u, float v, float t) {
		//old equation float d = Abs(x);
		float d = Sqrt(u * u + v * v);
		Vector3 p;

		p.x = u;

		p.y = Sin(PI * (4f * d - t));
		p.y = p.y / (1f + 10f * d); //to reduce the amplitude so that it does not go off screen 

		p.z = v;

		return p; //to reduce the amplitude so that it does not go off screen
	}

	//a override function for the delegate function to get others
	public delegate Vector3 Function (float u, float v, float t);

	public enum FunctionName { Wave, MultiWave, Ripple } //enum for names of the functions

	static Function[] functions = { Wave, MultiWave, Ripple }; //list for the actual functions

	public static Function GetFunction(FunctionName name) //for other classes to get the functions
	{
		return functions[(int)name]; //cast bc enum cant implicitly cast to int
	}
}