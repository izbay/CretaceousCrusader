using UnityEngine;
using System.Collections;

public class UnitSpawner : MonoBehaviour {


	public GameObject Lancer; 
	public float TimetoSpawn = 2f;
	public Transform[] spawnPoint;


	// Use this for initialization
	void Start () {
		InvokeRepeating ("Spawn", TimetoSpawn, TimetoSpawn);
		
	}

	void Spawn(){

		int spawnPointArray = Random.Range (0, spawnPoint.Length);

		Instantiate (Lancer, spawnPoint[spawnPointArray].position,spawnPoint[spawnPointArray].rotation);
	}
	
	// Update is called once per frame
	void Update () {
	

	
	}
}
