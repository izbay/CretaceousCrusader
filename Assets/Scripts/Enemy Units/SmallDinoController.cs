using UnityEngine;
using System.Collections;

public class SmallDinoController : DinoController
{
	protected override void StartledState()
	{
		Debug.Log ("STARTLED");
		// catch if the player units have backed away before we get here
		if (playerUnitsNearby.Count == 0)
		{
		Debug.Log ("IDLE");
			stateDelegate = IdleState;
		}
		
		else
		{
			// look at the player unit
			Vector3 targetDir = playerUnitsNearby[0].transform.position - transform.position;
			targetDir.z = 0;
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			
			// TODO vary the response based on player unit type (i.e. always attack farmers & quarriers, run from lancers)
			if (playerUnitsNearby.Count + 2 > dinosNearby.Count)
			{
				stateDelegate = FleeingState;
			}
			
			else
			{
				// TODO set all nearby dinos to attack the same target (or all from the same nest)
				stateDelegate = AttackingState;
			}
		}
	}

}
