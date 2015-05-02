using UnityEngine;
using System.Collections;

public class LargeDinoController : DinoController
{
	public bool loseFlag = false;
	protected GameObject mapIcon;
	protected Vector3 keepPos;

	protected float regenTick = 0f;
	protected float regenSpeed = 20f;

	protected override void Start()
	{
		keepPos = GameObject.FindGameObjectWithTag("Player").transform.position;
		mapIcon = transform.FindChild("MapIndicator").gameObject;
		base.Start();
	}

	protected override void Update()
	{
		mapIcon.transform.rotation = Quaternion.Euler (90,0,0);

		if(regenTick > regenSpeed){
			health += (maxHealth / 20f);
			if(health > maxHealth){
				health = maxHealth;
			}
			regenTick = 0f;
		} else {
			regenTick += Time.deltaTime;
		}

		base.Update();
	}

	public override void Hit(UnitController attacker)
	{
		if(statTracker != null){
			statTracker.LdinoHealth = health * 100f / maxHealth;
		}
		base.Hit (attacker);
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
				Vector3 bestPos = Vector3.zero;
				float bestDist = Mathf.Infinity;

				for(int i=0; i<2; i++){
					Vector3 newPos;
					int biome;
					do{
						newPos = this.transform.position + new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
						// clamp to terrain boundary
						newPos.x = Mathf.Clamp(newPos.x, 1, 2999);
						newPos.z = Mathf.Clamp(newPos.z, 1, 2999);
						biome = tb.getBiomeAtWorldCoord(newPos);
					} while (biome == 0 || biome == 5);

					float dist = Vector3.Distance (keepPos, newPos);
					// 33% chance to not take the closest path.
					float chance = Random.Range (0,100);
					if(dist < bestDist ||chance < 33){
						bestDist = dist;
						bestPos = newPos;
					}
				}
				navigationController.registerClick(this, bestPos);
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
		if (!loseFlag && keep != null && keep.totalUnits() != 0)
		{
			GameObject Princess = GameObject.Find("Princess(Clone)");
			if(Princess != null)
				Princess.GetComponent<PrincessController>().winFlag = true;
			Application.LoadLevel("Win");
		}
	}
}
