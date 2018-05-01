using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SquareSquare {

	static int goalDimensionSize;
	static float roughnessDimension;

	public static float[,] GenerateHeightMap(int size, int seed, float roughness)
	{
		SetParameters (size, seed, roughness);
		float[,] heightMap = SquareSquare_Main();

		NormaliseHeightMap (heightMap); 

		float[,] nm = ResizeArray (heightMap, heightMap.GetLength (0) - 1, heightMap.GetLength (1) - 1);
		return nm;
	}

	static void SetParameters(int size, int seed, float roughness)
	{
		goalDimensionSize = (int)Mathf.Pow (2, size) + 1;
		roughnessDimension = roughness;
		Random.InitState(seed);
	}

	static float[,] SquareSquare_Main()
	{
		int currentDimensionSize = 3;
		int dimensionIncrease = 1;

		float[,] oldHeightmap = new float[currentDimensionSize, currentDimensionSize];
		Reset (oldHeightmap);

		float[,] newHeightMap = null;

		while (currentDimensionSize < goalDimensionSize) 
		{
			newHeightMap = new float[currentDimensionSize + dimensionIncrease, currentDimensionSize + dimensionIncrease];
			int squareDimension = currentDimensionSize - 1;


			for (int y = 0; y < squareDimension; y++) //cycle through squares
			{
				for (int x = 0; x < squareDimension; x++) //cycle through squares
				{
					newHeightMap [(x * 2), (y * 2)] = (((9.0f * oldHeightmap [x, y]) + (3.0f * oldHeightmap [x + 1, y]) + (3.0f * oldHeightmap [x, y + 1]) + (1.0f * oldHeightmap [x + 1, y + 1])) / 16.0f) + RandAroundZero (roughnessDimension); //TL
					newHeightMap [(x * 2) + 1, (y * 2)] = (((9.0f * oldHeightmap [x+1, y]) + (3.0f * oldHeightmap [x, y]) + (3.0f * oldHeightmap [x+1, y + 1]) + (1.0f * oldHeightmap [x, y + 1])) / 16.0f) + RandAroundZero (roughnessDimension); //TR
					newHeightMap [(x * 2), (y * 2) + 1] = (((9.0f * oldHeightmap [x, y+1]) + (3.0f * oldHeightmap [x, y]) + (3.0f * oldHeightmap [x+1, y + 1]) + (1.0f * oldHeightmap [x + 1, y])) / 16.0f) + RandAroundZero (roughnessDimension); //BL
					newHeightMap [(x * 2) + 1, (y * 2) + 1] = (((9.0f * oldHeightmap [x+1, y+1]) + (3.0f * oldHeightmap [x + 1, y]) + (3.0f * oldHeightmap [x, y + 1]) + (1.0f * oldHeightmap [x, y])) / 16.0f) + RandAroundZero (roughnessDimension); //BR
				}
			}
				
			currentDimensionSize += dimensionIncrease;
			dimensionIncrease *= 2;
			roughnessDimension = roughnessDimension / 2;

			oldHeightmap = newHeightMap;
		}
		return newHeightMap;
	}

	static void Reset(float[,] map)
	{
		for (int i = 0; i < map.GetLength (0); i++) 
		{
			for (int j = 0; j < map.GetLength (1); j++)
			{
				SetValue(map, i, j, Random.value);
			}
		}
	}

	static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
	{
		var newArray = new T[rows, cols];
		int minRows = Mathf.Min (rows, original.GetLength (0));
		int minCols = Mathf.Min (cols, original.GetLength (1));
		for (int i = 0; i < minRows; i++)
			for (int j = 0; j < minCols; j++)
				newArray [i, j] = original [i, j];
		return newArray;
	}

	static void SetValue(float[,] map, int x, int y, float val)
	{
		map [x, y] = val;
	}

	static float GetValue(float[,] map, int x, int y)
	{
		if(x > -1 && x < map.GetLength(0) && y > -1 && y < map.GetLength(1))
			return map [x, y];
		return 0.0f;
	}
		
	static float FindMax(float[,] heightMap)
	{
		float Max = 0.0f;
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float temp = GetValue (heightMap, i, j);
				if (temp > Max)
					Max = temp;
			}
		}
		return Max;
	}

	static float FindMin(float[,] heightMap)
	{
		float Min = 1.0f;
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float temp = GetValue (heightMap, i, j);
				if (temp < Min)
					Min = temp;
			}
		}
		return Min;
	}

	static float[,] NormaliseHeightMap(float[,] heightMap)
	{
		float max = FindMax (heightMap);
		float min = FindMin (heightMap);
		for (int i = 0; i < heightMap.GetLength (0); i++) 
		{
			for (int j = 0; j < heightMap.GetLength (1); j++)
			{
				float oldValue = GetValue(heightMap, i, j);
				float newValue = (oldValue - min)/(max - min);
				SetValue (heightMap, i, j, newValue);
			}
		}
		return heightMap;
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
		return (spread * r)/5;
	}
}
