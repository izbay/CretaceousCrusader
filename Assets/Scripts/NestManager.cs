using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NestManager : MonoBehaviour {
	public GameObject[] dinos;
	private int type;
	private int count;
	private List<GameObject> spawned;

	private float spawnTick = 0f;
	private float spawnSpeed = 30f;

	// Use this for initialization
	void Start () {
		float random = Random.Range (0, 1f);
		if (random < 0.65f) {
			//Small
			type = 0;
			count = 5;
		} else {
			//Medium
			type = 1;
			count = 3;
			transform.localScale = new Vector3(2.5f,2.5f,2.5f);
		}

		// Initial Spawn
		spawned = new List<GameObject>();
		for (int i=0; i<count; i++) {
			//spawned.Add (Instantiate (dinos[type], Vector3.zero, new Quaternion()) as GameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Handle Respawns. If we lost everyone, remove nest.
		int alive = spawned.Count;
		if (alive == 0) {
			//remove nest
		}/** else if (alive < count) {
			//spawn
			if(spawnTick >= spawnSpeed){
				spawned.Add (Instantiate (dinos[type], Vector3.zero, new Quaternion()) as GameObject);
				spawnTick = 0f;
			} else {
				spawnTick += Time.deltaTime;
			}
		}*/
	}

	void pruneTheDead(){
		foreach (GameObject g in spawned) {
			if (g == null)
				spawned.Remove (g);
		}
	}
}
