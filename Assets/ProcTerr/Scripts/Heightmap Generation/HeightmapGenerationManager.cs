using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapGenerationManager : MonoBehaviour 
{
	[SerializeField]
	private GenerationMethod terrainGenerationMethod;
	private const int chunkSize = 10;
	[Range(0,1000)]
	public int terrainHeight;
	[Range(0,1)] [SerializeField]
	private float terrainRoughness;
	[SerializeField]
	private int terrainSeed;
	[SerializeField]
	private int heatMapSeed;
	[SerializeField]
	private int rainMapSeed;

	[Range(0,2)] [SerializeField]
	private float perlin_Lacunarity;
	[Range(0,1)] [SerializeField]
	private float perlin_Persistance;
	[Range(1,1000)] [SerializeField]
	private float perlin_Scale;

	[SerializeField]
	private AnimationCurve animationCurve;

	public enum GenerationMethod
	{
		MidpointDisplacement = 1,
		DiamondSquare = 2,
		SquareSquare = 3,
		//Perlin = 4
	};
			
	public AnimationCurve GetCurve()
	{
		return animationCurve;
	}

	public float[,] GenerateHeightMap(float offsetX, float offsetY)
	{
		return Generate (terrainSeed, (int)terrainGenerationMethod, terrainRoughness, offsetX, offsetY);
	}

	public float[,] GenerateHeatMap(float offsetX, float offsetY, int newGenerationMethod, float newRoughness)
	{
		return Generate (heatMapSeed, newGenerationMethod, newRoughness, offsetX, offsetY);
	}

	public float[,] GenerateRainMap(float offsetX, float offsetY, int newGenerationMethod, float newRoughness)
	{
		return Generate (rainMapSeed, newGenerationMethod, newRoughness, offsetX, offsetY);
	}

	private float[,] Generate(int chunkSeed, int generationMethod, float roughness, float offsetX, float offsetY)
	{
		float[,] heightMap;
		switch ((int)generationMethod) 
		{
		case 1:
			heightMap = MidpointDisplacement.GenerateHeightMap(chunkSize, chunkSeed, 0.5f, roughness);
			break;
		case 2:
			heightMap = DiamondSquare.GenerateHeightMap (chunkSize, chunkSeed, 0.5f, roughness);
			break;
		case 3:
			heightMap = SquareSquare.GenerateHeightMap (chunkSize, chunkSeed, roughness);
			break;
		/*
		case 4:
			heightMap = Perlin.GenerateHeightMap (chunkSize, chunkSeed, offsetX, offsetY, perlin_Lacunarity, perlin_Persistance, perlin_Scale);
			break;
		*/
		default:
			heightMap = null;
			break;
		}
		return heightMap;
	}
}
