using UnityEngine;
using System.Collections;

public class QuarrierController : PlayerUnitController {

	protected override void Start ()
	{
		base.Start ();
		classID = 1;
	}

	public void registerClick(StoneController unit){
			Atarget = unit;
		AdjustPosition ();
	}
	//Perform Attack
	public override void Attack(){
		if (Atarget is StoneController) {
			if (currentCarry < maxCarry) {
				attackCharge = 0;
				Atarget.Hit (gameObject.GetComponent<UnitController> ());
				currentCarry += attackStr;
				if (currentCarry >= maxCarry) {
					if(!returning){
						ReturnResources();
					}
				}
			}
		} else {
			base.Attack();
		}
	}
	protected override void Update(){
		base.Update ();
		if (returning) {
			if(Vector3.Distance(curr_pos,KeepTransform)<20f){
				UnloadResources();
			}
		}
	}
	public void ReturnResources(){
		returnToKeep ();
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
