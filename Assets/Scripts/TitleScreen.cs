using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.anyKey || Input.GetMouseButton (0) || Input.GetMouseButton (1)){
			Application.LoadLevel ("Terrain");
		}
	}
}
