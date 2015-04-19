using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoneController : UnitController {
	
	new protected void Start(){

		selectable = false;
	}

	//Take Damage
	public override void Hit(UnitController attacker){
		
		health -= attacker.attackStr;

		//Kill this Unit, first resetting all attacking units to having no target
		if (health < 1) {
			attacker.Atarget=null;
			GameObject.Destroy(gameObject);
		}
	}
	public override void setRails(bool railSetting){

	}
	public override void SavePosition () {
		
		curr_pos = transform.position;
	}
}
