using UnityEngine;
using static UnityEngine.Mathf;


public static class FunctionLibrary
{
	public static FunctionName GetNextFunctionName (FunctionName name) {
		if ((int)name < functions.Length - 1) {
			return name + 1;
		}
		else {
			return 0;
		}
	}

	public static FunctionName GetRandomFunctionNameOtherThan (FunctionName name) {
		var choice = (FunctionName)Random.Range(1, functions.Length);
		return choice == name ? 0 : choice;
	}

	public static Vector3 Morph (
		float u, float v, float t, Function from, Function to, float progress
	) 
	{
		return Vector3.LerpUnclamped(from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress));
	}

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

	public static Vector3 Sphere(float u, float v, float t)
	{
		Vector3 p;

		float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
		float s = r * Cos(0.5f * PI * v);

		p.x = s * Sin(PI * u);
		p.y = r * Sin(0.5f * PI * v);
		p.z = s * Cos(PI * u);

		return p;
	}

	public static Vector3 Torus(float u, float v, float t)
	{
		Vector3 p;

		float r1 = 1f + 0.1f * Sin(PI * (6f * u + 0.5f * t)); //the major radius, radius of the whole thing
		float r2 = 0.35f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t)); //the minor radius, radius of the minor circle/sphere
		float s = r1 + r2 * Cos(PI * v);

		p.x = s * Sin(PI * u);
		p.y = r2 * Sin(PI * v);
		p.z = s * Cos(PI * u);

		return p;
	}

	//a override function for the delegate function to get others
	public delegate Vector3 Function (float u, float v, float t);

	public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus } //enum for names of the functions

	static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Torus }; //list for the actual functions

	public static Function GetFunction(FunctionName name) //for other classes to get the functions
	{
		return functions[(int)name]; //cast bc enum cant implicitly cast to int
	}
}