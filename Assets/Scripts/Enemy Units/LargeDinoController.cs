using UnityEngine;
using System.Collections;

public class LargeDinoController : DinoController
{

	protected override void Idle()
	{
		// get startled when a player unit comes nearby
		if (playerUnitsNearby.Count > 0)
		{
			stateDelegate = Startled;
		}
		
		// wander around TODO add a random wait time or chance
		if (navTarget == Vector3.zero)
		{
			Vector3 newPos = this.transform.position + new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
			newPos.x = Mathf.Clamp(newPos.x, 1, 2999);
			newPos.y = Mathf.Clamp(newPos.y, 1, 2999);
		
			navigationController.registerClick(this, newPos);
			stateDelegate = Moving;
		}
	}
	
	protected override void Startled()
	{
		transform.LookAt (playerUnitsNearby[0].transform);
		stateDelegate = Attacking;
	}
}
