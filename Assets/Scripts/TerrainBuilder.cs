using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerrainBuilder : MonoBehaviour {
	public GameObject[] Objects;
	public GameObject[] Plants;
	public float[] PlantFreq;
	private float[] biomeCosts = new float[6] {10f,1.3f,0.85f,1f,2f,10f};
	private float[] biomeLevels;
	private Terrain terrain;
	private int offsetX, offsetY;
	private float tileSize1, tileSize2, scaleLevel;
	private float[,,] alphaData;
	private int nestAmt = 15;
	private int stoneAmt = 40;
	private int treeAmt = 25;
	private float spawnTick = 0f;
	private float spawnSpeed = 30f;
	private List<Vector3> KeepLocations = new List<Vector3>();
	private List<Vector3> NestLocations = new List<Vector3>();
	private List<Vector3> RockLocations = new List<Vector3>();

	// Use this for initialization
	void Start () {
		initTerrainSeed ();
		GenerateTerrain ();
	}

	void Update () {
		if (spawnTick > spawnSpeed) {
			if (GameObject.FindGameObjectsWithTag ("nest").Length < nestAmt) {
				StartCoroutine (Place (Objects [1], 1, NestLocations));
			}
			if (GameObject.FindGameObjectsWithTag ("stone").Length < stoneAmt) {
				StartCoroutine (Place (Objects [2], 1, RockLocations));
			}
			spawnTick = 0f;
		} else {
			spawnTick += Time.deltaTime;
		}
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
		biomeLevels = new float[6];

		biomeLevels[0]= allHeights[fivePercent*4]; 				// Water - 20%
		biomeLevels[1]= allHeights[totalIdxs - fivePercent*16];	// Swamp - 10% (Rounded down)
		biomeLevels[2]= allHeights[totalIdxs - fivePercent*14];	// Plains - 45%
		biomeLevels[3]= allHeights[totalIdxs - fivePercent*5];	// Forest - 10%
		biomeLevels[4]= allHeights[totalIdxs - fivePercent*3];	// Hill - 5%
		biomeLevels[5]= allHeights[totalIdxs - fivePercent*2];	// Mountain - 10%

		for(int i=0; i < width; i++){
			for(int j=0; j < depth; j++){
			// Water
				if (height[i,j] < biomeLevels[0]){
					height[i,j] = biomeLevels[0] - 0.0035f;
					setTexture(i,j,0,1f);
			// Mountain	
				} else if (height[i,j] > biomeLevels[5]){
					height[i,j] += 0.035f;
					setTexture(i,j,5,1f);
			// Hill
				} else if (height[i,j] > biomeLevels[4]){
					height[i,j] += 0.0015f;
					setTexture(i,j,4,((height[i,j]-biomeLevels[4])/biomeLevels[4])*16);
					RockLocations.Add (getWorldCoordFromTerrainCoord(i,j,height[i,j]));
			// Forest
				} else if (height[i,j] > biomeLevels[3]){
					height[i,j] += 0.00125f;
					setTexture(i,j,3,((height[i,j]-biomeLevels[3])/biomeLevels[3])*32);
					if(UnityEngine.Random.Range(0,10) < 7){
						NestLocations.Add (getWorldCoordFromTerrainCoord(i,j,height[i,j]));
					} else {
						RockLocations.Add (getWorldCoordFromTerrainCoord(i,j,height[i,j]));
					}
			// Plains
				} else if (height[i,j] > biomeLevels[2]){
					height[i,j] += 0.001f;
					setTexture(i,j,2,((height[i,j]-biomeLevels[2])/biomeLevels[2])*4);
					KeepLocations.Add (getWorldCoordFromTerrainCoord(i,j,height[i,j]));//+0.0383f));
			// Swamp
				} else {
					setTexture(i,j,1,((height[i,j]-biomeLevels[1])/biomeLevels[1])*64);
					if(UnityEngine.Random.Range(0,10) < 3){
						RockLocations.Add (getWorldCoordFromTerrainCoord(i,j,height[i,j]));
					}
				}

				// Place Nav Nodes (normally k=0;k<6;k++)
				for(int k=2; k < 6; k+=2){
					if(height[i,j] <= biomeLevels[k]+0.0001f && height[i,j] >= biomeLevels[k]-0.0001f){
						GameObject node = Instantiate (Objects[0], getWorldCoordFromTerrainCoord(i,j, height[i,j]), new Quaternion()) as GameObject;
						node.transform.parent = transform;
					}
				}
			}
		}
		NestLocations = Shuffle(NestLocations, 200);
		RockLocations = Shuffle(RockLocations, 200);
		KeepLocations = Shuffle(KeepLocations, 800);

		PlaceKeep ();
		PlaceBigDino ();
		
		terrain.terrainData.SetAlphamaps (0, 0, alphaData);
		terrain.terrainData.SetHeights (0,0,height);
		BuildBaseBoard();

		StartCoroutine(Place (Objects[1], nestAmt, NestLocations));
		StartCoroutine(Place (Objects[2], stoneAmt, RockLocations));

		for(int i=0; i<Plants.Length; i++){
			StartCoroutine(Place (Plants[i], Mathf.RoundToInt(PlantFreq[i]*treeAmt), NestLocations));
		}
	}

	private void PlaceKeep(){
		// TODO: (Stretch Goal) Further Pruning and Score Locations.
		GameObject keep = GameObject.FindGameObjectWithTag("Player");
		keep.transform.position = KeepLocations[0];
		keep.transform.rotation = Quaternion.Euler (0, 180, 0);//UnityEngine.Random.Range(0, 360), 0);
		//keep.transform.parent = transform;
		keep.GetComponent<KeepManager>().Spawn(new int[]{1,0,3,0,1});
	}

	private void PlaceBigDino(){
		GameObject dino = GameObject.Find ("L_Dino");
		int biggestIndex = 1;
		float biggestDistance = 0f;

		for(int i=1; i<KeepLocations.Count; i++){
			float distance = Vector3.Distance (KeepLocations[0],KeepLocations[i]);
			if(distance > 4000f){ // Good enough.
				dino.transform.position = KeepLocations[i];
				return;
			} else if(distance > biggestDistance){
				biggestDistance = distance;
				biggestIndex = i;
			}
		}
		dino.transform.position = KeepLocations[biggestIndex];
	}

	IEnumerator Place(GameObject obj, int num, List<Vector3> list){
		List<Vector3> usedLocations = new List<Vector3>();
		for(int k=0; k < num; k++){
			try{	
				bool foundFlag;
				Vector3 proposedLocation = Vector3.zero;
				do {
					foundFlag = true;
					proposedLocation = list[list.Count-1];
					foreach (Vector3 v in usedLocations){
						if (Vector3.Distance (v, proposedLocation) < 300f){
							foundFlag = false;
						}
					}
					list.RemoveAt (list.Count-1);
				} while (!foundFlag && list.Count >= 1);
				if (foundFlag){
					GameObject node = Instantiate (obj, proposedLocation, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0)) as GameObject;
					node.transform.parent = transform;
					usedLocations.Add (proposedLocation);
				}
			} catch {
				// Ignore problems due to indexing.
			}
			yield return null;
		}
		
	}

	private List<Vector3> Shuffle(List<Vector3> list, int cullingRadius){  
		list.RemoveAll (v => v.x < cullingRadius || v.x > terrain.terrainData.size.x-cullingRadius || v.z < cullingRadius || v.z > terrain.terrainData.size.z-cullingRadius);
		int idx = list.Count;  
		while (idx > 1) {  
			idx--;  
			int i = UnityEngine.Random.Range(0,idx+1);  
			// Swap
			Vector3 value = list[i];  
			list[i] = list[idx];  
			list[idx] = value;  
		}
		return list;
	}
	
	private Vector3 getWorldCoordFromTerrainCoord(int i, int j, float k){
		TerrainData td = terrain.terrainData;
		float x = ((j*1.0f)/td.heightmapHeight) * td.size.x;
		float y = k * td.size.y;
		float z = ((i*1.0f)/td.heightmapWidth) * td.size.z;
		return new Vector3(x, y, z);
	}

	public Vector3 toGroundLevel(Vector3 position){
		position.y = getHeightAtWorldCoord (position);
		return position;
	}

	public float getHeightAtWorldCoord(Vector3 position){
		TerrainData td = terrain.terrainData;
		int j = Mathf.RoundToInt (td.heightmapWidth * position.z / td.size.z);
		int i = Mathf.RoundToInt (td.heightmapHeight * position.x / td.size.x);
		return td.GetHeight (i, j);
	}

	public int getBiomeAtWorldCoord(Vector3 position){
		float height = getHeightAtWorldCoord (position) / terrain.terrainData.size.y;
		if (height <= biomeLevels [0] - 0.00348f) {
			return 0;
		} else if (height >= biomeLevels[5] + 0.035f){
			return 5;
		} else if (height >= biomeLevels[4] + 0.0015f){
			return 4;
		} else if (height >= biomeLevels[3] + 0.00125f){
			return 3;
		} else if (height >= biomeLevels[2] + 0.001f){
			return 2;
		} else {
			return 1;
		}
	}

	public float getHungerWeight(Vector3 location){
		return biomeCosts [getBiomeAtWorldCoord (location)];
	}

	private void setTexture(int x, int y, int id, float percent){
		for(int i=0;i<6;i++){
			try{
				if(i == id){
					alphaData[x,y,i] = percent;
				} else if (percent < 1f && i == id-1){
					alphaData[x,y,i] = 1 - percent;
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
	void BuildBaseBoard(){
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
