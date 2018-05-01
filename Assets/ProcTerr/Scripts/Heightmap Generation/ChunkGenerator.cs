using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour 
{
	private HeightmapGenerationManager manager;
	private TerrainLayers layers;

	public Transform terrainHolder { get; set; }
	public ChunkData[,] chunks { get; set; }

	private const int numberOfChunks = 1;
	private const int genMethodForTempAndRainfall = 1; //gen method to use for temp and rainfall maps
	private const float RoughnessForTempAndRainfall = 0.5f; //Roughness to use for temp and rainfall maps

	public void Generate()
	{
		DeleteOldTerrain ();
		manager = GetComponent<HeightmapGenerationManager> ();
		layers = GetComponent<TerrainLayers> ();

		int dimension = (int)Mathf.Sqrt (numberOfChunks);

		chunks = new ChunkData[dimension, dimension];
		for (int y = 0; y < dimension; y++) 
		{
			for (int x = 0; x < dimension; x++) 
			{
				int chunkDimension = 1025;
				chunks [x, y] = new ChunkData (chunkDimension, manager.terrainHeight, layers);
				float xOffset = (float)x * chunkDimension;
				float yOffset = (float)y * chunkDimension;
				chunks [x,y].terrain.transform.SetParent (terrainHolder);

				float[,] terrainMap = Normalise.NormaliseHeightmap (ApplyHeightCurve(manager.GenerateHeightMap(xOffset, yOffset)));
				float[,] temperatureMap = Normalise.NormaliseHeightmap (manager.GenerateHeatMap(xOffset, yOffset, genMethodForTempAndRainfall, RoughnessForTempAndRainfall));
				float[,] precipitationMap = Normalise.NormaliseHeightmap (manager.GenerateRainMap(xOffset, yOffset, genMethodForTempAndRainfall, RoughnessForTempAndRainfall));

				//chunks [x, y].SetHeightMap (Copy2DArray (terrainMap));
				chunks [x, y].initialHeightMap = Copy2DArray(terrainMap);
				chunks [x, y].temperatureMap = temperatureMap;
				chunks [x, y].precipitationMap = precipitationMap;

				chunks [x, y].SetHeightMap (GetComponent<Convrt> ().Convert ());

				chunks[x,y].OffsetChunk(x,y);

				GetComponent<TextureManager> ().TextureChunk(chunks[x,y]);
			}
		}
	}

	public void ApplyErosion()
	{
		int dimension = (int)Mathf.Sqrt (numberOfChunks);
		for (int y = 0; y < dimension; y++) 		{
			for (int x = 0; x < dimension; x++) 
			{
				ErosionManager.ApplyErosion(this, chunks[x,y]);
				GetComponent<TextureManager> ().TextureChunk(chunks[x,y]);
			}
		}
	}

	private static float[,] Copy2DArray(float[,] old)
	{
		int width = old.GetLength (0);
		float[,] newarray = new float[width, width];
		for(int y = 0; y < width; y++)
		{
			for(int x = 0; x < width; x++)
			{
				newarray[x,y] = old[x,y];
			}
		}
		return newarray;
	}

	private void DeleteOldTerrain()
	{
		DestroyImmediate (GameObject.Find ("Terrain"));
		terrainHolder = new GameObject ("Terrain").transform;
	}

	private float[,] ApplyHeightCurve(float[,] heightMap)
	{
		AnimationCurve curve = GetComponent<HeightmapGenerationManager> ().GetCurve ();
		for (int y = 0; y < heightMap.GetLength(1); y++) 
		{
			for (int x = 0; x < heightMap.GetLength(0); x++) 
			{
				heightMap[x,y] = curve.Evaluate (heightMap [x, y]);
			}
		}
		return heightMap;
	}

	public void TextureAllChunks()
	{
		foreach (ChunkData c in chunks) 
		{
			GetComponent<TextureManager> ().TextureChunk (c);
		}
	}

}
