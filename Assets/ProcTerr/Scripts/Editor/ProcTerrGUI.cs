using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (ChunkGenerator))]
public class ProcTerrGUI : Editor 
{
	public override void OnInspectorGUI()
	{
		ChunkGenerator c = (ChunkGenerator)target;

		DrawDefaultInspector ();
		if (GUILayout.Button ("Generate Terrain")) 
		{
			c.Generate ();
		}
		if (GUILayout.Button ("Erode Terrain")) 
		{
			c.ApplyErosion ();
		}
		if (GUILayout.Button ("Generate and Erode Terrain")) 
		{
			c.Generate ();
			c.ApplyErosion ();
		}
		if (GUILayout.Button ("Retexture Terrain")) 
		{
			c.TextureAllChunks();
		}
	}
}

[CustomEditor (typeof (Convrt))]
public class Convert : Editor 
{
	public override void OnInspectorGUI()
	{
		Convrt c = (Convrt)target;

		DrawDefaultInspector ();
		if (GUILayout.Button ("Convert Heightmap")) 
		{
			c.Convert ();
		}
	}

}

