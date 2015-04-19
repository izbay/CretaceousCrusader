using UnityEngine;
using System.Collections;

public class QuarrierController : PlayerUnitController {


	public void registerClick(StoneController unit){
			Atarget = unit;
			target = unit.transform.position;
	}
	//Perform Attack
	public override void Attack(){
		if (Atarget is StoneController) {
			if (currentCarry < maxCarry) {
				attackCharge = 0;
				Atarget.Hit (gameObject.GetComponent<UnitController> ());
				currentCarry += attackStr;
			}
		} else {
			base.Attack();
		}
	}
	protected override void Update(){
		base.Update ();
		if (currentCarry == maxCarry) {
			if(!returning){
				ReturnResources();
			}
		}
		if (returning) {
			if(Vector3.Distance(curr_pos, Keep.transform.position)<20f){
				UnloadResources();
			}
		}
	}
	public void ReturnResources(){
		target= Keep.transform.position;
		navigationController.registerClick(Keep.transform.position);
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
