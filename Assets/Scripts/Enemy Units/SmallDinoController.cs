using UnityEngine;
using System.Collections;

public class SmallDinoController : DinoController
{
	protected override void Startled()
	{
		Debug.Log ("Startled!");
		// catch if the player units have backed away before we get here
		if (playerUnitsNearby.Count == 0)
			stateDelegate = Idle;
		
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
				stateDelegate = Fleeing;
			}
			
			else
			{
				stateDelegate = Attacking;
			}
		}
	}

}
