using UnityEngine;
using System.Collections;

public class FarmerController : PlayerUnitController
{
	public int wanderDistance;

	protected override void Start ()
	{
		base.Start ();
		classID = 0;
	}
	
	protected override void IdleState()
	{
		// Wander around the keep
		// wander around TODO add a random wait time or chance so they aren't always moving
		if (navTarget == Vector3.zero)
		{
			if(keep != null)
			{
				Vector3 newPos = keep.transform.position + new Vector3(Random.Range(-wanderDistance, wanderDistance), 0, Random.Range(-wanderDistance, wanderDistance));
				// clamp to terrain boundary
				newPos.x = Mathf.Clamp(newPos.x, 1, 2999);
				newPos.z = Mathf.Clamp(newPos.z, 1, 2999);
				navigationController.registerClick(this, newPos);
			}
		}
		if (navTarget != Vector3.zero || path != null)
		{
			Move ();
		}
	}
	// TODO find way to ignore navigation orders
}
