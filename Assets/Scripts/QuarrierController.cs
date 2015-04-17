using UnityEngine;
using System.Collections;

public class QuarrierController : UnitController {

	public bool returning;
	public Vector3 KeepTransform;
	protected override void Start(){
		
		base.Start();
		selectable = true;
		returning = false;
		KeepTransform = Keep.transform.position;
	}
	protected override void Update(){
		base.Update ();
		if (currentCarry == maxCarry) {
			if(!returning){
				ReturnResources();
			}
			if(Vector3.Distance(curr_pos, Keep.transform.position)<20f){
				UnloadResources();
			}
		}
	}
	public void registerClick(StoneController unit){
			Atarget = unit;
			target = unit.transform.position;
	}
	//Perform Attack
	public override void Attack(){
		if (currentCarry < maxCarry) {
			attackCharge = 0;
			Atarget.Hit (gameObject.GetComponent<UnitController> ());
			currentCarry += attackStr;
		}
	}

	public void ReturnResources(){
		target= Keep.transform.position;
		setRails (true);
		returning = true;
	}
	public void UnloadResources(){
		target = Vector3.zero;
		setRails (false);
		returning = false;
		Keep.addRock(currentCarry);
		currentCarry = 0;

	}
}
