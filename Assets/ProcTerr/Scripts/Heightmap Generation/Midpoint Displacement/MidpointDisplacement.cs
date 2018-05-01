using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MidpointDisplacement
{
	static float[,] heightMap;
	static int size;
	static int newSize;
	static float initialSpread;
	static float spreadReductionRate;

	public static float[,] GenerateHeightMap(int pSize, int seed, float pInitialSpread, float pSpreadReductionRate)
	{
		SetParameters (pSize, seed, pInitialSpread, pSpreadReductionRate);

		mpd_displace_main ();
		heightMap = NormaliseHeightmap (heightMap);

		return heightMap;
	}

	static void SetParameters(int pSize, int seed, float pInitialSpread, float pSpreadReductionRate)
	{
		size = pSize;
		initialSpread = pInitialSpread;
		spreadReductionRate = pSpreadReductionRate;
		Random.InitState(seed);
		newSize = (int)Mathf.Pow (2, size) + 1;
		heightMap = new float[newSize, newSize];
	}

	static void SetValue(int x, int y, float val)
	{
		heightMap [x, y] = val;
	}

	static float GetValue(int x, int y)
	{
		return heightMap [x, y];
	}

	static void Reset()
	{
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				SetValue(i, j, Random.value);
			}
		}
	}

	static void InitialiseCorners()
	{
		SetValue (0, 0, Random.value);
		SetValue (0, heightMap.GetLength (1)-1, Random.value);
		SetValue (heightMap.GetLength (0)-1, 0, Random.value);
		SetValue (heightMap.GetLength (0)-1, heightMap.GetLength (1)-1, Random.value);
	}

	static void mpd_displace(int lx, int rx, int by, int ty, float spread)
	{
		int cx = Midpoint(lx, rx);
		int cy = Midpoint(by, ty);

		float bl = GetValue(lx, by);
		float br = GetValue(rx, by);
		float tl = GetValue(lx, ty);
		float tr = GetValue(rx, ty);

		float top = AverageOf2 (tl, tr);
		float left = AverageOf2 (bl, tl);
		float bottom = AverageOf2 (bl, br);
		float right = AverageOf2 (br, tr);
		float center = AverageOf4 (top, left, bottom, right);

		SetValue (cx, by, Offset (bottom, spread));
		SetValue (cx, ty, Offset (top, spread));
		SetValue (lx, cy, Offset (left, spread));
		SetValue (rx, cy, Offset (right, spread));
		SetValue (cx, cy, Offset (center, spread));
	}

	static void mpd_displace_main()
	{
		Reset();
		InitialiseCorners ();
		float spread = initialSpread;

		for (int iteration = 0; iteration < size; iteration++) 
		{
			int chunks = (int)Mathf.Pow (2, iteration);
			int chunkWidth = (newSize - 1) / chunks;
			for (int chunkx = 0; chunkx < chunks; chunkx++)  
			{
				for (int chunky = 0; chunky < chunks; chunky++)
				{
					int leftx = chunkWidth * chunkx;
					int rightx = leftx + chunkWidth;
					int bottomy = chunkWidth * chunky;
					int topy = bottomy + chunkWidth;
					mpd_displace (leftx, rightx, bottomy, topy, spread);
				}
			}
			spread *= spreadReductionRate;
		}
	}

	static int Midpoint(int x, int y)
	{
		return (x + y) / 2;
	}

	static float AverageOf2(float a, float b)
	{
		return (a + b) / 2.0f;
	}

	static float AverageOf4(float a, float b, float c, float d)
	{
		return (a + b + c + d) / 4.0f;
	}

	static float Offset(float value, float spread)
	{
		return value + RandAroundZero(spread);
	}

	static float RandAroundZero(float spread)
	{
		return (spread * Random.Range(-1,2));
	}

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
				heightmap [x, y] = Mathf.InverseLerp (minHeight, maxHeight, heightmap[x,y]);
			}
		}
		return heightmap;
	}

}
