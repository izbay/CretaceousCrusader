using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectionController : MonoBehaviour {

	private UnitController selected;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void registerClick(UnitController unit){
		if(selected != null){
			selected.transform.Find("Selection Projector").GetComponent<Projector>().enabled = false;
		}
		selected = unit;
		if(selected != null){
			selected.transform.Find("Selection Projector").GetComponent<Projector>().enabled = true;
		}
	}

	public UnitController getSelected(){
		return selected;
	}
}
