using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DinoController : UnitController
{
	protected NavigationController navController;

	// TODO keep separate lists for each unit type
	protected List<PlayerUnitController> playerUnitsNearby;
	
	// TODO keep separate lists for each dino type
	protected List<DinoController> dinosNearby;
	
	protected Vector3 navTarget;

	protected delegate void StateDelegate();
	protected StateDelegate stateDelegate;
	
	private int pathRefreshRate = 300;
	private int pathRefreshCount;

	void Awake()
	{
		navController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
		playerUnitsNearby = new List<PlayerUnitController>();
		dinosNearby = new List<DinoController>();
	}

	protected override void Start()
	{
		stateDelegate = Idle;
	}
	
	protected override void Update()
	{
		stateDelegate();
	}
	
	private void Idle()
	{
//		navigationController.registerClick (this, Atarget.transform.position);
		
		// wander around
		setPath(navController.getPath(
			transform.position, transform.position + new Vector3 (Random.Range (0, 100), 0, Random.Range (0, 100))));
	 	
	 	// get startled when a player unit comes nearby
	 	if (playerUnitsNearby.Count > 0)
	 	{
			stateDelegate = Startled;
	 	}
	}
	
	private void Move()
	{
		if (navTarget != Vector3.zero)
		{	
			Seek ();
		}
		
		// Check path
		if (path != null && path.Count > 1 && onRails)
		{
			setTarget(path[1]);
			
			// Get a new path every time we hit the pathRefreshRate
			if (pathRefreshCount == pathRefreshRate)
			{
				path = navController.getPath (transform.position, path[path.Count - 1]);
				pathRefreshCount = 0;
			}
			else
			{
				List<Vector3> returnPath = navController.quickScanPath (transform.position, path[path.Count - 1]);
				if (!navController.pathIsInvalid (returnPath))
				{
					path = returnPath;
				}
				
				pathRefreshCount++;
			}
		}
		else
		{
			path = null;
			navTarget = Vector3.zero;
			setRails (false);
		}
	}
	
	protected override void Seek ()
	{
		Debug.DrawLine (transform.position, navTarget, Color.magenta);
		
		float distance = Vector3.Distance (transform.position,target);
		
		if (distance >= 1f || (Atarget != null && distance >= attackRange))
		{
			
			Vector3 targetDir = new Vector3 (target.x, 0, target.z) - new Vector3 (transform.position.x, 0, transform.position.z);
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);
			
			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			float angle = Vector3.Angle (targetDir, transform.forward);
			if (angle < 3)
			{
				rigidbody.AddForce (transform.forward * moveSpeed * 500);
			}
		}
		
		else
		{
			// We've reached the target.
			target = Vector3.zero;
			setRails (false);
		}
	}
	
	
	private void Startled()
	{
		// catch if the player units have backed away before get here
		if (playerUnitsNearby.Count == 0)
			stateDelegate = Idle;
			
		else
		{
			// look at the player unit
			transform.LookAt (playerUnitsNearby[0].transform);
			
//			Vector3 targetDir = new Vector3 (target.x, 0, target.z) - new Vector3 (transform.position.x, 0, transform.position.z);
//			Quaternion targetRotation = Quaternion.LookRotation (targetDir);
//			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			
			// TODO vary the response based on player unit type (i.e. always attack farmers & quarriers, run from lancers)
			if (playerUnitsNearby.Count > dinosNearby.Count)
			{
				stateDelegate = Fleeing;
			}
			
			else
			{
				stateDelegate = Attacking;
			}
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
			// if distance < attackRange
				// move towards the target
				
			// else
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
			navController.getPath(
				transform.position,
				transform.position + (transform.position - playerUnitsNearby[0].transform.position).normalized * 100);
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
