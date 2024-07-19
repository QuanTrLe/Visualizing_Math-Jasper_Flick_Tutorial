using UnityEngine;
using static UnityEngine.Mathf;


public static class FunctionLibrary
{
	public static float Wave(float x, float t)
	{
		return Sin(PI * (x + t)); //sin wave formula
	}

	public static float MultiWave(float x, float t)
	{
		float y = Sin(PI * (x + 0.5f * t)); // the normal sin wave

		//constant * for performance
		//to add more complexity to the wave
		y += Sin(2f * PI * (x + t)) * 0.5f;

		y = y * (2f / 3f); //due to the higher frequence, the range is now -1.5 x 1.5. Make it -1 x 1

		return y;
	}

	public static float Ripple (float x, float t) {
		float d = Abs(x);

		float y = Sin(PI * (4f * d - t));

		return y / (1f + 10f * d); //to reduce the amplitude so that it does not go off screen
	}

	public delegate float Function (float x, float t); //a normalized type of function for the delegate function to get others

	public enum FunctionName { Wave, MultiWave, Ripple } //enum for names of the functions

	static Function[] functions = { Wave, MultiWave, Ripple }; //list for the actual functions

	public static Function GetFunction(FunctionName name) //for other classes to get the functions
	{
		return functions[(int)name]; //cast bc enum cant implicitly cast to int
	}
}