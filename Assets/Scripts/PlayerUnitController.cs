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
	public Material[] targetMaterials;
	protected Vector3 last_pos;

	public Vector3 keepOffsetPos;
	
	public bool returning;
	public bool changing;
	
	protected TerrainBuilder tb;
	protected bool isSelected = false;
	protected GameObject selectionIndicator;
	protected GameObject targetIndicator;

	public void toggleSelected(){
		isSelected = !isSelected;	
		if(selectionIndicator != null){
			selectionIndicator.GetComponent<Projector>().enabled = isSelected;
			if(!isSelected){
				targetIndicator.GetComponent<Projector>().enabled = isSelected;
			}
		} else {
			transform.FindChild ("Selection Projector").gameObject.GetComponent<Projector>().enabled = true;
		}
	}

	protected void updateTargetIndicator(){
		Projector targetProjector = targetIndicator.GetComponent<Projector>();
		targetIndicator.transform.rotation = Quaternion.Euler (90f,0f,0f);
		if(attackTarget != null){
			targetIndicator.transform.position = attackTarget.transform.position + new Vector3(0f,20f,0f);
			targetProjector.material = targetMaterials[1];
			targetProjector.enabled = true;
		} else if (path != null){
			targetIndicator.transform.position = path[path.Count-1] + new Vector3(0f,5f,0f);
			targetProjector.material = targetMaterials[0];
			targetProjector.enabled = true;
		} else {
			targetProjector.enabled = false;
		}
	}

	protected override void Start ()
	{
		keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		keepOffsetPos = keep.transform.position+(keep.transform.forward*5);
		tb = GameObject.Find ("Terrain").GetComponent<TerrainBuilder> ();
		selectionIndicator = transform.FindChild ("Selection Projector").gameObject;
		targetIndicator = transform.FindChild ("Target Projector").gameObject;
		base.Start ();
	}

	protected override void Update()
	{
		base.Update ();
		if(isSelected){
			updateTargetIndicator();
		}
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
