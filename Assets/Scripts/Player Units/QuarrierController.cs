using UnityEngine;
using System.Collections;

public class QuarrierController : PlayerUnitController {

	protected override void Start ()
	{
		base.Start ();
		classID = 1;
	}

	public void registerClick(StoneController unit){
			attackTarget = unit;
		AdjustPosition ();
	}
	//Perform Attack
	public override void Attack(){
		if (attackTarget is StoneController) {
			if (currentCarry < maxCarry) {
				attackCharge = 0;
				attackTarget.Hit (gameObject.GetComponent<UnitController> ());
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
			if(Vector3.Distance(curr_pos,keepTransform)<20f){
				UnloadResources();
			}
		}
	}
	public void ReturnResources(){
		returnToKeep ();
		returning = true;
	}
	public void UnloadResources(){
		navTarget = Vector3.zero;
		setRails (false);
		returning = false;
		keep.addRock(currentCarry);
		currentCarry = 0;

	}
}
