using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DiamondSquare
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
		if(x > -1 && x < heightMap.GetLength(0) && y > -1 && y < heightMap.GetLength(1))
			return heightMap [x, y];
		return 0.0f;
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

	static void SquareStep(int lx, int rx, int by, int ty, float spread)
	{
		int cx = Midpoint(lx, rx);
		int cy = Midpoint(by, ty);

		float bl = GetValue(lx, by);
		float br = GetValue(rx, by);
		float tl = GetValue(lx, ty);
		float tr = GetValue(rx, ty);

		float centre = AverageOf4 (bl, br, tl, tr);

		SetValue (cx, cy, centre);
	}

	static void DiamondStep(int lx, int rx, int by, int ty, float spread, int chunkWidth)
	{
		int cx = Midpoint(lx, rx);
		int cy = Midpoint(by, ty);

		float bl = GetValue(lx, by);
		float br = GetValue(rx, by);
		float tl = GetValue(lx, ty);
		float tr = GetValue(rx, ty);
		float centre = GetValue(cx, cy);

		float centreLeft = GetValue(cx - chunkWidth, cy);
		float centreTop = GetValue(cx, cy + chunkWidth);
		float centreRight = GetValue(cx + chunkWidth, cy);
		float centreBottom = GetValue(cx, cy - chunkWidth);

		float left = AverageOf4 (tl, bl, centre, centreLeft);
		float top = AverageOf4 (tl, tr, centre, centreTop);
		float right = AverageOf4 (tr, br, centre, centreRight);
		float bottom = AverageOf4 (bl, br, centre, centreBottom);

		SetValue (lx, cy, Offset (left, spread));
		SetValue (cx, ty, Offset (top, spread));
		SetValue (rx, cy, Offset (right, spread));
		SetValue (cx, by, Offset (bottom, spread));
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
					SquareStep (leftx, rightx, bottomy, topy, spread);
					DiamondStep (leftx, rightx, bottomy, topy, spread, chunkWidth);
				}
			}
			spread *= spreadReductionRate;
		}

		NormaliseHeightMap ();
	}

	static int Midpoint(int x, int y)
	{
		return (x + y) / 2;
	}

	static float AverageOf2(float a, float b)
	{
		return (a + b) / 2.0f;
	}

	static float AverageOf3(float a, float b, float c)
	{
		return (a + b + c) / 3.0f;
	}

	static float AverageOf4(float a, float b, float c, float d)
	{
		if (d == 0.0f)
			return AverageOf3 (a, b, c);
		return (a + b + c + d) / 4.0f;
	}

	static float Offset(float value, float spread)
	{
		return value + RandAroundZero(spread);
	}

	static float RandAroundZero(float spread)
	{
		int r = 0;
		while (r == 0)
			r = Random.Range (-1, 2);
		return (spread * r);
	}

	static float FindMax()
	{
		float Max = 0.0f;
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float temp = GetValue (i, j);
				if (temp > Max)
					Max = temp;
			}
		}
		return Max;
	}

	static float FindMin()
	{
		float Min = 1.0f;
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float temp = GetValue (i, j);
				if (temp < Min)
					Min = temp;
			}
		}
		return Min;
	}

	static float[,] NormaliseHeightMap()
	{
		float max = FindMax ();
		float min = FindMin ();
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float oldValue = GetValue(i, j);
				float newValue = (oldValue - min)/(max - min);
				SetValue (i, j, newValue);
			}
		}
		return heightMap;
	}

}
