﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerrainBuilder : MonoBehaviour {

	private Terrain terrain;
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
		terrain = transform.GetComponent<Terrain>();
		TerrainData td = terrain.terrainData;
		alphaData =  td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);

		tileSize1 = UnityEngine.Random.Range (3,6); // Higher == More Noise
		tileSize2 = UnityEngine.Random.Range (4,8); // Higher == More Noise
		offsetX = UnityEngine.Random.Range (0,1000000);
		offsetY = UnityEngine.Random.Range (0,1000000);
		scaleLevel = UnityEngine.Random.Range (8.95f,8.05f); // Lower == Bigger Elevation Changes
	}

	private void GenerateTerrain(){
		int width = terrain.terrainData.heightmapWidth;
		int depth = terrain.terrainData.heightmapHeight;
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
		biomeLevels[2]= allHeights[totalIdxs - fivePercent*14];	// Plains - 40%
		biomeLevels[3]= allHeights[totalIdxs - fivePercent*6];	// Forest - 15%
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

		terrain.terrainData.SetAlphamaps (0, 0, alphaData);
		terrain.terrainData.SetHeights (0,0,height);
		OnWizardCreate();
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

/** START IMPORTED CODE
 * Code acquired from http://wiki.unity3d.com/index.php/CreateTerrainBaseboards
 * Author: Max70 */
	// PUBLIC DATA
	public float detail = 2.0f;
	public float globalH = -40.0f;
	public Color selectColor = Color.white;
	public float UVFactor = 8.0f;
	public Texture selectTexture;
	// PRIVATE DATA
	private Vector3 posTerrain;
	private GameObject edgeG;
	private MeshFilter meshFilter;
	private Vector3[] vertices;
	private Vector2[] uvs;
	private int[] triangles;
	// BEGIN METHODS
	void OnWizardCreate(){
		posTerrain = terrain.transform.position;
		int sizeTab = (int)( terrain.terrainData.size.x / detail )+1;
		float[] hh = new float[sizeTab];
		Material newMat = new Material( Shader.Find("Diffuse"));
		newMat.color = selectColor;
		newMat.mainTexture = selectTexture;
		newMat.name = "BaseboardsMaterial";	
		int id=0;
		Vector2 terPos = new Vector2(0,0.001f);
		for(float x=0; x<=terrain.terrainData.size.x; x+=detail){
			terPos.x = x / terrain.terrainData.size.x;
			hh[id++] = terrain.terrainData.GetInterpolatedHeight(terPos.x, terPos.y);
		}
		Mesh m = CreateEdgeObject("BBorderX0");
		int numVertices = sizeTab*2;
		int numTriangles = (sizeTab-1)*2*3;
		vertices = new Vector3[numVertices];
		uvs = new Vector2[numVertices];
		triangles = new int[numTriangles];		
		float uvFactor=1.0f/UVFactor;
		int index=0;
		for(int i=0; i<sizeTab; i++){
			float xx = i*detail;
			vertices[index] = new Vector3(xx,hh[i],0) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 1);
			vertices[index] = new Vector3(xx,globalH,0) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 0);
		}
		for(int i=0; i<numTriangles/3; i++){
			if( i-((i/2)*2)==0){
				triangles[i*3]=i+1;
				triangles[i*3+1]=i;
				triangles[i*3+2]=i+2;				
			}else{
				triangles[i*3]=i;
				triangles[i*3+1]=i+1;
				triangles[i*3+2]=i+2;
			}
		}
		FinalizeEdgeObject(m, newMat);	
		id=0;
		terPos.y = 0.9990f;
		for(float x=0; x<=terrain.terrainData.size.x; x+=detail){
			terPos.x = x / terrain.terrainData.size.x;
			hh[id++] = terrain.terrainData.GetInterpolatedHeight(terPos.x, terPos.y);
		}
		m = CreateEdgeObject("BBorderXZ");
		index=0;
		for(int i=0; i<sizeTab; i++){
			float xx = i*detail;
			vertices[index] = new Vector3(xx,hh[i],terrain.terrainData.size.z) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 1);
			vertices[index] = new Vector3(xx,globalH,terrain.terrainData.size.z) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 0);
		}
		for(int i=0; i<numTriangles/3; i++){
			if( i-((i/2)*2)==0){
				triangles[i*3]=i+1;
				triangles[i*3+1]=i+2;
				triangles[i*3+2]=i;				
			}else{
				triangles[i*3]=i;
				triangles[i*3+1]=i+2;
				triangles[i*3+2]=i+1;
			}
		}
		FinalizeEdgeObject(m, newMat);
		id=0;
		terPos.x = 0.001f;
		for(float x=0; x<=terrain.terrainData.size.z; x+=detail){
			terPos.y = x / terrain.terrainData.size.z;
			hh[id++] = terrain.terrainData.GetInterpolatedHeight(terPos.x, terPos.y);
		}		
		m = CreateEdgeObject("BBorderZ0");
		index=0;
		for(int i=0; i<sizeTab; i++){
			float xx = i*detail;
			vertices[index] = new Vector3(0,hh[i],xx) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 1);
			vertices[index] = new Vector3(0,globalH,xx) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 0);
		}
		for(int i=0; i<numTriangles/3; i++){
			if( i-((i/2)*2)==0){
				triangles[i*3]=i+1;
				triangles[i*3+1]=i+2;
				triangles[i*3+2]=i;				
			}else{
				triangles[i*3]=i;
				triangles[i*3+1]=i+2;
				triangles[i*3+2]=i+1;
			}
		}
		FinalizeEdgeObject(m, newMat);
		id=0;
		terPos.x = 0.999f;
		for(float x=0; x<=terrain.terrainData.size.z; x+=detail){
			terPos.y = x / terrain.terrainData.size.z;
			hh[id++] = terrain.terrainData.GetInterpolatedHeight(terPos.x, terPos.y);
		}
		m = CreateEdgeObject("BBorderZX");
		index=0;
		for(int i=0; i<sizeTab; i++){
			float xx = i*detail;
			vertices[index] = new Vector3(terrain.terrainData.size.x,hh[i],xx) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 1);
			vertices[index] = new Vector3(terrain.terrainData.size.x,globalH,xx) + posTerrain;
			uvs[index++] = new Vector2(i*uvFactor, 0);
		}
		for(int i=0; i<numTriangles/3; i++)
		{	if( i-((i/2)*2)==0){
				triangles[i*3]=i+1;
				triangles[i*3+1]=i;
				triangles[i*3+2]=i+2;				
			}else{
				triangles[i*3]=i;
				triangles[i*3+1]=i+1;
				triangles[i*3+2]=i+2;
			}
		}
		FinalizeEdgeObject(m, newMat);		
	}
	Mesh CreateEdgeObject(string name)
	{
		edgeG = new GameObject();
		edgeG.name = name;		
		meshFilter = (MeshFilter)edgeG.AddComponent(typeof(MeshFilter));
		edgeG.AddComponent(typeof(MeshRenderer));		
		Mesh m = new Mesh();
		m.name = edgeG.name;	
		return m;
	}
	void FinalizeEdgeObject(Mesh m, Material mat)
	{
		m.vertices = vertices;
		m.uv = uvs;
		m.triangles = triangles;
		m.RecalculateNormals();
		meshFilter.sharedMesh = m;
		m.RecalculateBounds();
		meshFilter.renderer.material = mat;
		edgeG.transform.parent = terrain.transform;	
		edgeG.isStatic = true;
	}
/** End of Imported Code */

}
