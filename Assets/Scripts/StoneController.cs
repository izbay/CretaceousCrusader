﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoneController : UnitController {
	
	protected void Start(){

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
		unitHealth.size = health / 100f;
	}
	public override void setRails(bool railSetting){

	}
}
