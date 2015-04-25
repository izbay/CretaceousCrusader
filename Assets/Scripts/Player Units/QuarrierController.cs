using UnityEngine;
using System.Collections;

public class QuarrierController : PlayerUnitController
{
	protected override void Start ()
	{
		base.Start ();
		classID = 1;
	}

	public void registerClick(StoneController unit)
	{
		attackTarget = unit;
		Move();
	}
	
	//Perform Attack
	public override void Attack()
	{
		if (attackTarget is StoneController)
		{
			if (currentCarry < maxCarry)
			{
				attackCharge = 0;
				anim.attackAnim();
				attackTarget.Hit (gameObject.GetComponent<UnitController> ());
				currentCarry += attackStr;
				if (currentCarry >= maxCarry)
				{
					if(!returning)
					{
						ReturnResources();
					}
				}
			}
		}
		else
		{
			base.Attack();
		}
	}
	
	protected override void Update()
	{
		base.Update ();
		if (returning)
		{
			if(Vector3.Distance(last_pos,keepOffsetPos)<20f)
			{
				UnloadResources();
			}
		}
	}
	
	public void ReturnResources()
	{
		ReturnToKeep ();
		returning = true;
	}
	
	public void UnloadResources()
	{
		navTarget = Vector3.zero;
		setRails (false);
		returning = false;
		keep.addRock(currentCarry);
		currentCarry = 0;

	}
}
