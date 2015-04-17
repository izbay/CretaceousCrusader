using UnityEngine;
using System.Collections;

public class EnemyController : UnitController {
	
	protected override void Start(){

		base.Start();
		selectable = false;
	}

	public override void SavePosition () {

		curr_pos = transform.position;
	}
}
