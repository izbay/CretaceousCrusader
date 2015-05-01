using UnityEngine;
using System.Collections;

public class StatTracker : MonoBehaviour {

	public float time;
	public float foodConsumed;
	public float LdinoHealth;
	public int dinosKilled;
	public int unitsKilled;
	public int maxUnits;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.transform.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] duplicates = GameObject.FindGameObjectsWithTag("stat_tracker");
		if(duplicates.Length > 1){
			for(int i=0; i<duplicates.Length; i++){
				if(duplicates[i].transform != this.transform){
					Destroy (duplicates[i]);
				}
			}
		}

		if(Application.loadedLevelName=="Title"){
			time = foodConsumed = 0f;
			LdinoHealth = 100f;
			dinosKilled = unitsKilled = maxUnits = 0;
		}
	}
}
