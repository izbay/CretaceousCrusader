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

	public KeepManager keep;
	public Vector3 keepTransform;
	
	public bool returning;
	public bool changing;
	
	private TerrainBuilder tb;

	protected override void Start ()
	{
		keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		keepTransform = keep.transform.position+(keep.transform.forward*5);
		tb = GameObject.Find ("Terrain").GetComponent<TerrainBuilder> ();
		base.Start ();
	}
	
	public override void SavePosition ()
	{
		base.SavePosition ();
		// This is rudimentary food consumption as a proof of concept. This will be modified to use 'energy' later.
		if(curr_pos != Vector3.zero){
			float hunger = (Vector3.Distance (transform.position,curr_pos) * walkingConsumption) / 40f;
			hunger += (standingConsumption * Time.deltaTime) / 80f;
			hunger *= tb.getHungerWeight(transform.position);
			if(keep.requestFood (hunger) < hunger){
				health -= 0.1f;
			};
		}
	}

	protected override void Update(){
		base.Update ();
		if (changing) {
			if(Vector3.Distance(curr_pos, keep.transform.position)<20f){
				if(changeID!=classID){
					keep.changeUnit(this,changeID);
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
		navigationController.registerClick(this, keepTransform);
	}
}
