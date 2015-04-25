using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoneController : UnitController {

	// TODO make this not a child of UnitController...
	protected override void Start ()
	{
		
	}

	//Take Damage
	public override void Hit(UnitController attacker){
		
		health -= attacker.attackStr;

		//Kill this Unit, first resetting all attacking units to having no target
		if (health < 1) {
			attacker.attackTarget=null;
			GameObject.Destroy(gameObject);
		}
	}
	public override void setRails(bool railSetting){

	}

    protected override void Update()
    {
        
    }
}
