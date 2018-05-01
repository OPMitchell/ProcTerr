using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HydraulicAction : MonoBehaviour
{
	[Range(0,1000)] [SerializeField]
	private int strength; // How many times to run the algorithm (strength of erosion).

	[SerializeField]
	private ComputeShader shader; // Compute shader to run on the GPU

	[Range(0.0f,0.1f)][SerializeField]
	private float newWaterPerIteration;

	[Range(0.0f,0.5f)][SerializeField]
	private float solubilityCoefficient;

	[Range(0,1)][SerializeField]
	private float evaporationCoefficient;

	[Range(0,0.5f)][SerializeField]
	private float sedimentCapacityCoefficient;

	[Range(0,1.0f)][SerializeField]
	private float temperatureWeightCoefficient;

	[Range(0,1.0f)][SerializeField]
	private float rainWeightCoefficient;

	private int waterStepKernelHandle;
	private int transportStepKernelHandle;
	private int evaporationStepKernelHandle;

	private float[,] originalHeightmap;
	private TerrainLayers terrainLayers;

	public float[,] Erode(ChunkData chunk)
	{
		terrainLayers = chunk.terrainLayers;

		waterStepKernelHandle = shader.FindKernel("CSWater");
		transportStepKernelHandle = shader.FindKernel("CSTransport");
		evaporationStepKernelHandle = shader.FindKernel("CSEvaporation");

		originalHeightmap = chunk.GetHeightMap ();
			float[,] heightMap = chunk.GetHeightMap();
		int width = heightMap.GetLength (0);

		float[,] waterMap = new float[width, width];
		float[,] sedimentMap = new float[width, width];

		float[,] temperatureMap = chunk.temperatureMap;
		float[,] precipitationMap = chunk.precipitationMap;

		if (strength > 0) 
		{
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch ();
			stopWatch.Start ();

			for (int i = 0; i < strength; i++) 
			{
				RunOnGPU (heightMap, waterMap, sedimentMap, temperatureMap, precipitationMap);
			}

			stopWatch.Stop ();
			Debug.Log ("Hydraulic action of " + strength + " iterations took " + stopWatch.Elapsed);
			stopWatch.Reset ();
		}

		return heightMap;
	}

	private void RunOnGPU (float[,] heightMap, float[,] waterMap, float[,] sedimentMap, float[,] temperatureMap, float[,] precipitationMap) 
	{
		//Create a read/writable buffer that contains the heightmap data and sends it to the GPU.
		//We need to specify the length of the buffer and the size of a single element. 
		//The buffer needs to be the same length as the heightmap, and each element in the heightmap is a //single float which is 4 bytes long.
		ComputeBuffer b_heightMap = new ComputeBuffer (heightMap.Length, 4);
		//Set the initial data to be held in the buffer as the pre-generated heightmap
		b_heightMap.SetData (heightMap); 

		ComputeBuffer b_old_waterMap = new ComputeBuffer (waterMap.Length, 4);
		b_old_waterMap.SetData (waterMap); 
		ComputeBuffer b_waterMap = new ComputeBuffer (waterMap.Length, 4);
		b_waterMap.SetData (waterMap); 
		ComputeBuffer b_sedimentMap = new ComputeBuffer (sedimentMap.Length, 4);
		b_sedimentMap.SetData (sedimentMap);

		ComputeBuffer b_temperatureMap = new ComputeBuffer (temperatureMap.Length, 4);
		b_temperatureMap.SetData (temperatureMap);
		ComputeBuffer b_precipitationMap = new ComputeBuffer (precipitationMap.Length, 4);
		b_precipitationMap.SetData (precipitationMap);

		ComputeBuffer b_originalHeightMap = new ComputeBuffer (originalHeightmap.Length, 4);
		b_originalHeightMap.SetData (originalHeightmap);
		ComputeBuffer b_layerHeights = new ComputeBuffer (terrainLayers.GetLayerHeights().Length, 4);
		b_layerHeights.SetData (terrainLayers.GetLayerHeights());
		ComputeBuffer b_layerSolubilities = new ComputeBuffer (terrainLayers.GetLayerSolubilities().Length, 4);
		b_layerSolubilities.SetData (terrainLayers.GetLayerSolubilities());

		shader.SetInt ("width", heightMap.GetLength(0)); 
		shader.SetInt ("numberOfLayers", terrainLayers.GetNumberOfLayers ());
		shader.SetFloat ("waterStepCoef", newWaterPerIteration);
		shader.SetFloat ("solubilityConst", solubilityCoefficient);
		shader.SetFloat ("evaporationCoef", evaporationCoefficient);
		shader.SetFloat ("sedimentCapacityCoef", sedimentCapacityCoefficient);
		shader.SetFloat ("temperatureWeightCoef", temperatureWeightCoefficient);
		shader.SetFloat ("rainWeightCoef", rainWeightCoefficient);

		shader.SetBuffer(waterStepKernelHandle, "heightMap", b_heightMap);
		shader.SetBuffer(transportStepKernelHandle, "heightMap", b_heightMap);
		shader.SetBuffer(evaporationStepKernelHandle, "heightMap", b_heightMap);
		shader.SetBuffer(waterStepKernelHandle, "waterMap", b_waterMap);
		shader.SetBuffer(transportStepKernelHandle, "waterMap", b_waterMap);
		shader.SetBuffer(evaporationStepKernelHandle, "waterMap", b_waterMap);
		shader.SetBuffer(waterStepKernelHandle, "oldWaterMap", b_old_waterMap);
		shader.SetBuffer(transportStepKernelHandle, "oldWaterMap", b_old_waterMap);
		shader.SetBuffer(waterStepKernelHandle, "sedimentMap", b_sedimentMap);
		shader.SetBuffer(transportStepKernelHandle, "sedimentMap", b_sedimentMap);
		shader.SetBuffer(evaporationStepKernelHandle, "sedimentMap", b_sedimentMap);

		shader.SetBuffer(waterStepKernelHandle, "originalHeightMap", b_originalHeightMap);
		shader.SetBuffer(waterStepKernelHandle, "layerHeights", b_layerHeights);
		shader.SetBuffer(waterStepKernelHandle, "layerSolubilities", b_layerSolubilities);

		shader.SetBuffer(evaporationStepKernelHandle, "temperatureMap", b_temperatureMap);
		shader.SetBuffer(waterStepKernelHandle, "precipitationMap", b_precipitationMap);

		shader.Dispatch (waterStepKernelHandle, 57, 19, 1);
		shader.Dispatch (transportStepKernelHandle, 57, 19, 1);
		shader.Dispatch (evaporationStepKernelHandle, 57, 19, 1);

		b_heightMap.GetData(heightMap);
		b_heightMap.Dispose ();
		b_waterMap.GetData(waterMap);
		b_waterMap.Dispose ();
		b_sedimentMap.GetData (sedimentMap);
		b_sedimentMap.Dispose ();
		b_old_waterMap.Dispose ();
		b_originalHeightMap.Dispose ();
		b_layerHeights.Dispose ();
		b_layerSolubilities.Dispose ();
		b_temperatureMap.Dispose ();
		b_precipitationMap.Dispose ();
	}
}
