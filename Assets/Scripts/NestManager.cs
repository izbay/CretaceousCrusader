using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NestManager : MonoBehaviour {
	
    public GameObject[] dinos;
    public List<GameObject> spawnedDinos; 
	private int type;
	private int maxDino;

	private float spawnTick = 0f;
	private float spawnSpeed = 30f;

	// Use this for initialization
	void Start () {
		float random = Random.Range (0, 1f);
		if (random < 0.65f) {
			//Small
			type = 0;
            maxDino = 3;
		} else {
			//Medium
			type = 1;
            maxDino = 2;
			transform.localScale = new Vector3(2f,2f,2f);
		}

		// Initial Spawn
        spawnedDinos = new List<GameObject>();
        for (int i = 0; i < maxDino; i++)
        {
            Spawn();
        }

	}


    // Update is called once per frame
	void Update () {
		// Handle Respawns. If we lost everyone, remove nest.
		pruneTheDead();
		int alive = spawnedDinos.Count;
		if (alive == 0) {
            GameObject.Destroy(gameObject);
		} else if (spawnedDinos.Count < maxDino) {
			if(spawnTick >= spawnSpeed)
			{
			    Spawn();
				spawnTick = 0f;
			} else {
				spawnTick += Time.deltaTime;
			}
		}
	}
    public void Spawn()
    {
        Spawn(new int[1] { type });
    }
    public void Spawn(int[] unitList)
    {
        Vector3[] positions = new Vector3[unitList.Length];
        Vector3 basePos = transform.position + transform.forward.normalized * 5f;
        for (int i = 0; i < unitList.Length; i++)
        {
            Vector3 horz = transform.right.normalized * (i - (unitList.Length / 2.0f)) * 10.0f;
            positions[i] = basePos + horz;
        }
        // Spawn the units
        for (int i = 0; i < unitList.Length; i++)
        {
			GameObject newDino = Instantiate(dinos[unitList[i]], positions[i], transform.rotation) as GameObject;
			newDino.GetComponent<DinoController>().Nest = this;
            spawnedDinos.Add(newDino);
        }
    }

	void pruneTheDead(){
        // apparently we can't do this more efficiently as enumeration over an altered list throws errors
		for(int i=spawnedDinos.Count-1; i>=0; i--)
        {
            if(spawnedDinos[i]==null){
				spawnedDinos.RemoveAt (i);
			}
		}
	}
}
