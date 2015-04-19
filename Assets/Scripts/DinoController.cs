using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DinoController : UnitController
{
	protected List<PlayerUnitController> playerUnitsNearby;
	protected List<DinoController> dinosNearby;
	
	protected Vector3 destination;

	void Awake()
	{
		playerUnitsNearby = new List<PlayerUnitController>();
		dinosNearby = new List<DinoController>();
		stateDelegate = Idle;
		destination = transform.position + new Vector3 (Random.Range (0, 100), 0, Random.Range (0, 100));
	}

	protected override void Start()
	{
		base.Start();
	}
	
	protected override void Update()
	{
		stateDelegate();
	}
	
	private void Idle()
	{
		setPath(navigationController.getPath(transform.position, destination));
	 	
	 	if (playerUnitsNearby.Count > 0)
	 	{
			stateDelegate = Startled;
	 	}
	}
	
	private void Startled()
	{
		// face towards the unit
		
		if (playerUnitsNearby.Count > dinosNearby.Count)
		{
			stateDelegate = Fleeing;
		}
		else
		{
			stateDelegate = Attacking;
		}
	}
	
	private void Attacking()
	{
		if (Atarget == null)
		{
			// get a target
		}
		else
		{
			// perform an attack
		}
	}
	
	private void Fleeing()
	{
		// Check if player is nearby
		if (playerUnitsNearby.Count == 0)
		{
			stateDelegate = Idle;
		}
		
		else
		{
			// Run away from the player unit
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("dino"))
		{
			dinosNearby.Add (other.GetComponent<DinoController>());
		}
		else if (other.CompareTag("unit"))
		{
			playerUnitsNearby.Add (other.GetComponent<PlayerUnitController>());
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("dino"))
		{
			dinosNearby.Remove (other.GetComponent<DinoController>());
		}
		else if (other.CompareTag("unit"))
		{
			playerUnitsNearby.Remove (other.GetComponent<PlayerUnitController>());
		}
	}
}
