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
	protected Vector3 last_pos;

	public Vector3 keepOffsetPos;
	
	public bool returning;
	public bool changing;
	
	private TerrainBuilder tb;

	protected override void Start ()
	{
		keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		keepOffsetPos = keep.transform.position+(keep.transform.forward*5);
		tb = GameObject.Find ("Terrain").GetComponent<TerrainBuilder> ();
		base.Start ();
	}

	protected override void Update()
	{
		base.Update ();
		
//		Debug.Log (stateDelegate.Method.Name);
		
		 CheckHunger();
		
		// TODO turn this into a state Method
		if (changing)
		{
			if(Vector3.Distance(transform.position, keep.transform.position)<20f)
			{
				if(changeID != classID)
				{
					keep.changeUnit(this, changeID);
					changing = false;
				}
			}
		}
	}
	
	// TODO change to coroutine?
	protected void CheckHunger()
	{
		// This is rudimentary food consumption as a proof of concept. This will be modified to use 'energy' later.
		if(last_pos != Vector3.zero)
		{
//			float hunger = (Vector3.Distance (last_pos, transform.position) * walkingConsumption) / 40f;
			float hunger = (Vector3.Distance (transform.position, transform.position) * walkingConsumption) / 40f; // preserves previous functionality
			hunger += (standingConsumption * Time.deltaTime) / 80f;
			
			if(tb != null)
			{
				hunger *= tb.getHungerWeight(transform.position);
			}
			
			if(keep.requestFood (hunger) < hunger)
			{
				health -= 0.1f; // TODO scale health loss based on amount of food received
			};
		}
		
		last_pos = transform.position;
	}

	public void ChangeClass(int id)
	{
		if (id != classID)
		{
			changeID=id;
			changing=true;
			ReturnToKeep();
		}
	}
	
	public void ReturnToKeep()
	{
		navigationController.registerClick(this, keepOffsetPos);
	}
	
	protected override void OnDestroy()
	{
		// TODO there is probably a better way to handle this
		foreach (DinoController a in attackers)
		{
			if (a.playerUnitsNearby.Contains(this))
			{
				a.playerUnitsNearby.Remove(this);
			}
		}
		
		base.OnDestroy();
	}
}
