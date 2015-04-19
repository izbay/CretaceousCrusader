using UnityEngine;
using System.Collections;

public class PlayerUnitController : UnitController {

	public int maxCarry;
	public int currentCarry;
	public int energy;
	public float standingConsumption;
	public float walkingConsumption;
	public KeepManager Keep;

	protected override void Start ()
	{
		Keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		base.Start ();


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


}
