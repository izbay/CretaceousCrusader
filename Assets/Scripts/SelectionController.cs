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
		selected = unit;
	}

	public UnitController getSelected(){
		return selected;
	}
}
