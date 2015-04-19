using UnityEngine;
using System.Collections;

public class PlayerUnitController : UnitController {

	public int maxCarry;
	public int currentCarry;
	public int energy;
	//To be used when changing clases
	public int classID;
	public int changeID;
	//Food Consumption Stuff
	public float standingConsumption;
	public float walkingConsumption;

	public KeepManager Keep;
	public bool returning;
	public bool changing;
	public Vector3 KeepTransform;

	protected override void Start ()
	{
		Keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		KeepTransform = Keep.transform.position+(Keep.transform.forward*5);
		base.Start ();
		selectable = true;
		returning = false;
		changing = false;

	}
	public override void SavePosition ()
	{
		base.SavePosition ();
		// This is rudimentary food consumption as a proof of concept. This will be modified to use 'energy' later.
		if(curr_pos != Vector3.zero){
			float hunger = (Vector3.Distance (transform.position,curr_pos) * walkingConsumption) / 40f;
			hunger += (standingConsumption * Time.deltaTime) / 80f;
			if(Keep.requestFood (hunger) < hunger){
				health -= 0.1f;
			};
		}
	}

	protected override void Update(){
		base.Update ();
		if (changing) {
			if(Vector3.Distance(curr_pos, Keep.transform.position)<20f){
				if(changeID!=classID){
					Keep.changeUnit(this,changeID);
					changing=false;
				}
			}
		}
	}

	public void changeClass(int id){
		if (id != classID) {
			changeID=id;
			changing=true;
			returnToKeep();
		}
	}
	public void returnToKeep(){
		navigationController.registerClick(KeepTransform);
			
	}


}
