using UnityEngine;
using System.Collections;

public class LargeDinoController : DinoController
{
	protected GameObject mapIcon;

	protected override void Start()
	{
		mapIcon = transform.FindChild("MapIndicator").gameObject;
		base.Start();
	}

	protected override void Update()
	{
		mapIcon.transform.rotation = Quaternion.Euler (90,0,0);
		base.Update();
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
			// wander around TODO add a random wait time or chance
			if (navTarget == Vector3.zero)
			{
				Vector3 newPos = this.transform.position + new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
				// clamp to terrain boundary
				newPos.x = Mathf.Clamp(newPos.x, 1, 2999);
				newPos.z = Mathf.Clamp(newPos.z, 1, 2999);
				navigationController.registerClick(this, newPos);
			}
			
			Move();
		}
	}
	
	protected override void StartledState()
	{
		// look at the player unit
		Vector3 targetDir = playerUnitsNearby[0].transform.position - transform.position;
		targetDir.y = 0;
		Quaternion targetRotation = Quaternion.LookRotation (targetDir);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
		stateDelegate = AttackingState;
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (keep != null && keep.totalUnits() != 0)
		{
			Application.LoadLevel("Win");
		}
	}
}
