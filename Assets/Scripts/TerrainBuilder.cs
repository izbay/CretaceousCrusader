using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerrainBuilder : MonoBehaviour {

	private int offsetX, offsetY;
	private float tileSize1, tileSize2, scaleLevel;//, counter;
	private float[,,] alphaData;

	// Use this for initialization
	void Start () {
		//counter = 0;
		initTerrainSeed ();
		GenerateTerrain ();

	}

	void Update () {
		/**
		if(counter++ == 50){
			counter = 0;
			initTerrainSeed ();
			GenerateTerrain ();
		}
		**/
	}

	private void initTerrainSeed(){
		TerrainData td = transform.GetComponent<Terrain>().terrainData;
		alphaData =  td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);

		tileSize1 = UnityEngine.Random.Range (3,6); // Higher == More Noise
		tileSize2 = UnityEngine.Random.Range (4,8); // Higher == More Noise
		offsetX = UnityEngine.Random.Range (0,1000000);
		offsetY = UnityEngine.Random.Range (0,1000000);
		scaleLevel = UnityEngine.Random.Range (8.95f,8.05f); // Lower == Bigger Elevation Changes
	}

	private void GenerateTerrain(){
		Terrain t = transform.GetComponent<Terrain>();
		int width = t.terrainData.heightmapWidth;
		int depth = t.terrainData.heightmapHeight;
		float[,] height = new float[width, depth];
		float[] allHeights = new float[width*depth];

		for(int i=0; i < width; i++){
			for (int j=0; j < depth; j++){
				int x = (i + offsetX);
				int y = (j + offsetY);
				float noise1 = Mathf.PerlinNoise((float)x / (float)width * tileSize1, (float)y / (float)depth * tileSize1);
				float noise2 = Mathf.PerlinNoise((float)x / (float)width * tileSize2, (float)y / (float)depth * tileSize2);
				allHeights[width*i+j] = height[i,j] = (noise1 + noise2)/scaleLevel;
			}
		}








		// Assume a standard distribution of heights. (If it's skewed, the terrain may deviate from the planned percentages, but shouldn't by much)
		Array.Sort (allHeights);

		int fivePercent = Mathf.RoundToInt(width*depth*0.05f);
		int totalIdxs = width*depth;
		float[] biomeLevels = new float[6];

		biomeLevels[0]= allHeights[fivePercent*4]; 				// Water - 20%
		biomeLevels[1]= allHeights[totalIdxs - fivePercent*16];	// Swamp - 10% (Rounded down)
		biomeLevels[2]= allHeights[totalIdxs - fivePercent*14];	// Plains - 30%
		biomeLevels[3]= allHeights[totalIdxs - fivePercent*8];	// Forest - 25%
		biomeLevels[4]= allHeights[totalIdxs - fivePercent*3];	// Hill - 5%
		biomeLevels[5]= allHeights[totalIdxs - fivePercent*2];	// Mountain - 10%

		for(int i=0; i < width; i++){
			for(int j=0; j < depth; j++){

				if (height[i,j] <= biomeLevels[0]){
					height[i,j] = biomeLevels[0] - 0.0075f;
					setTexture(i,j,0);
				} else if (height[i,j] >= biomeLevels[5]){
					height[i,j] += 0.035f;
					setTexture(i,j,5);
				} else if (height[i,j] >= biomeLevels[4]){
					height[i,j] += 0.0015f;
					setTexture(i,j,4);
				} else if (height[i,j] >= biomeLevels[3]){
					height[i,j] += 0.00125f;
					setTexture(i,j,3);
				} else if (height[i,j] >= biomeLevels[2]){
					height[i,j] += 0.001f;
					setTexture(i,j,2);
				} else {
					setTexture(i,j,1);
				}
			}
		}

		t.terrainData.SetAlphamaps (0, 0, alphaData);
		t.terrainData.SetHeights (0,0,height);
	}

	private void setTexture(int x, int y, int id){
		for(int i=0;i<6;i++){
			try{
				if(i == id){
					alphaData[x,y,i] = 1f;
				} else {
					alphaData[x,y,i] = 0f;
				}
			} catch {
			
			}
		}
	}
}
