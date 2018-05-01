using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Normalise 
{
	public static float[,] NormaliseHeightmap(float[,] heightmap)
	{
		float dimension = heightmap.GetLength (0);
		float minHeight = float.MaxValue;
		float maxHeight = float.MinValue;

		for (int y = 0; y < dimension; y++) 
		{
			for (int x = 0; x < dimension; x++) 
			{
				float val = heightmap [x, y];
				if (val < minHeight)
					minHeight = val;
				else if (val > maxHeight)
					maxHeight = val;
			}
		}

		for (int y = 0; y < dimension; y++) 
		{
			for (int x = 0; x < dimension; x++) 
			{
				heightmap [x, y] = Mathf.Lerp (0.0f, 1.0f, heightmap[x,y]);
			}
		}
		return heightmap;
	}
}
