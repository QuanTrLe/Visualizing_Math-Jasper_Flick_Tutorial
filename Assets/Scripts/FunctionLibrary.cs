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
		float y = Sin(PI * (x + t)); // the normal sin wave

		//constant * for performance
		//to add more complexity to the wave
		y += Sin(2f * PI * (x + t)) * 0.5f;

		y = y * (2f / 3f); //due to the higher frequence, the range is now -1.5 x 1.5. Make it -1 x 1

		return y;
	}
}