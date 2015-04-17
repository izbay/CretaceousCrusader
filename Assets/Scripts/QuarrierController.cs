using UnityEngine;
using System.Collections;

public class QuarrierController : UnitController {

	protected override void Start(){
		
		base.Start();
		selectable = true;
	}

	public void registerClick(StoneController unit){

			Atarget = unit;
			target = unit.transform.position;


	}
}
