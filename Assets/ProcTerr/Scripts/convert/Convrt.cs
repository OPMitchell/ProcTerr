using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Convrt : MonoBehaviour 
{
	[SerializeField] Texture2D heightmapToConvert;
	[SerializeField] int width;
	[SerializeField] int height;

	public float[,] ClearHeightmap(float[,] heightmap)
	{
		for (int y = 0; y < heightmap.GetLength(1); y++) 
		{
			for (int x = 0; x < heightmap.GetLength(0); x++) 
			{
				heightmap [x, y] = 0.0f;
			}
		}
		return heightmap;
	}

	public float[,] Convert()
	{
		float[,] heightmap = new float[width, height];
		heightmap = ClearHeightmap (heightmap);
		Color[] pixels = heightmapToConvert.GetPixels ();
		for (int y = 0; y < heightmapToConvert.height; y++) 
		{
			for (int x = 0; x < heightmapToConvert.width; x++) 
			{
				Color c = pixels [(x+(y*heightmapToConvert.width))];
				float r = c.grayscale;
				heightmap [x, y] = r;
			}
		}
		return heightmap;
	}
}
