using UnityEngine;
using System.Collections;

public class PrincessController : FarmerController {
	public bool winFlag = false;
	protected float regenTick = 0f;
	protected float regenSpeed = 15f;
	protected float regenAmount = 2f;

	protected override void Start () {
		base.Start ();
		classID = 3;
	}

	protected override void Update() {
		base.Update ();

		if(regenTick > regenSpeed){
			health += regenAmount;
			if(health > maxHealth){
				health = maxHealth;
			}
			regenTick = 0f;
		} else {
			regenTick += Time.deltaTime;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!winFlag){
			GameObject.Find("L_Dino").GetComponent<LargeDinoController>().loseFlag = true;
			Application.LoadLevel("Lose");
		}
	}
}
