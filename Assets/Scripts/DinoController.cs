using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DinoController : UnitController
{
	// TODO keep separate lists for each unit and dino type
	public List<PlayerUnitController> playerUnitsNearby;
	public List<DinoController> dinosNearby;
	public int foodValue;
	
	protected NestManager nest;

	public NestManager Nest
	{
		get
		{
			return nest;
		}
		set
		{
			nest = value;
		}
	}

	void Awake()
	{
		playerUnitsNearby = new List<PlayerUnitController>();
		dinosNearby = new List<DinoController>();
	}

	protected override void Start()
	{
		base.Start();
        pathRefreshRate = 1000;
		stateDelegate = IdleState;
	}
	
	protected override void IdleState()
	{
		// get startled when a player unit comes nearby
		if (playerUnitsNearby.Count > 0)
		{
			stateDelegate = StartledState;
		}
		else
		{
			// wander around TODO add a random wait time or chance so the dinos aren't always moving
			if (navTarget == Vector3.zero)
			{
				if(nest != null)
				{
					int biome;
					Vector3 newPos;
					do{
						newPos = nest.transform.position + new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
						// clamp to terrain boundary
						newPos.x = Mathf.Clamp(newPos.x, 1, 2999);
						newPos.z = Mathf.Clamp(newPos.z, 1, 2999);
						biome = tb.getBiomeAtWorldCoord(newPos);
					} while (biome == 0 || biome == 5);

					navigationController.registerClick(this, newPos);
					//				stateDelegate = Moving;
				}
			}
			if (navTarget != Vector3.zero || path != null)
			{
				Move ();
			}
		}
	}
	
	protected virtual void StartledState()
	{
//		Debug.Log ("STARTLED");
		// catch if the player units have backed away before we get here
		if (playerUnitsNearby.Count == 0)
		{
//			Debug.Log ("IDLE");
			stateDelegate = IdleState;
		}
			
		else
		{
			// look at the player unit
			Vector3 targetDir = playerUnitsNearby[0].transform.position - transform.position;
			targetDir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			
			// TODO vary the response based on player unit type (i.e. always attack farmers & quarriers, run from lancers)
			if (playerUnitsNearby.Count > dinosNearby.Count + 1)
			{
				stateDelegate = FleeingState;
			}
			
			else
			{
				stateDelegate = AttackingState;
			}
		}
	}
	
	protected override void AttackingState()
	{
//		Debug.Log ("ATTACKING");
		if (playerUnitsNearby.Count == 0)
		{
//			Debug.Log ("IDLE");
			attackTarget = null;
			stateDelegate = IdleState;
		}
		else
		{
			if (attackTarget == null)
			{
				// get a new target
				attackTarget = playerUnitsNearby[0];
			}
			else
			{
				// look at the target
				Vector3 targetDir = attackTarget.transform.position - transform.position;
				targetDir.y = 0;
				Quaternion targetRotation = Quaternion.LookRotation (targetDir);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
				
				if (TargetInRange())
				{
					if (ReadyToAttack())
					{
						Attack();
					}
				}
				
				else
				{
					navigationController.registerClick(this, attackTarget.transform.position);
					Move();
				}
			}
		}
	}
	
	protected virtual void FleeingState()
	{
//		Debug.Log ("FLEEING");
		// Check if player is nearby
		if (playerUnitsNearby.Count == 0)
		{
//			Debug.Log ("IDLE");
			stateDelegate = IdleState;
		}
		
		else
		{
			// Run away from the player unit
			navigationController.registerClick(this, 
				transform.position + (transform.position - playerUnitsNearby[0].transform.position).normalized * 100);
				
			Move();
		}
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (keep != null)
		{
			keep.addFood(foodValue);
		}
	}

	protected float nearbyUnitsPower(){
		float returnVal = 0;
		foreach(PlayerUnitController p in playerUnitsNearby){
			if(p is LancerController){
				returnVal += 1f;
			} else if(p is QuarrierController){
				returnVal += 0.33f;
			}
		}
		return returnVal;
	}

	protected float nearbyDinoPower(){
		float returnVal = 0;
		foreach(DinoController d in dinosNearby){
			if(d is LargeDinoController){
				returnVal += 10f;
			} else if(d is MedDinoController){
				returnVal += 1.33f;
			} else {
				returnVal += 0.33f;
			}
		}
		return returnVal;
	}
}
