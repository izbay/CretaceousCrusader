using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DinoController : UnitController
{

	// TODO keep separate lists for each unit type
	protected List<PlayerUnitController> playerUnitsNearby;
	
	// TODO keep separate lists for each dino type
	protected List<DinoController> dinosNearby;

	void Awake()
	{
		navigationController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
		playerUnitsNearby = new List<PlayerUnitController>();
		dinosNearby = new List<DinoController>();
	}

	protected override void Start()
	{
        pathRefreshRate = 1000;
		stateDelegate = Idle;
	}
	
	protected override void Update()
	{
		if(stateDelegate != null){
			stateDelegate();
		}
	}
	
	protected void Idle()
	{
		// get startled when a player unit comes nearby
		if (playerUnitsNearby.Count > 0)
		{
			stateDelegate = Startled;
		}
		
		// wander around
		if (navTarget == Vector3.zero)
		{
			navigationController.registerClick(this, transform.position + new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)));
				
			stateDelegate = Moving;
		} 
	}
	
	protected void Moving()
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
				path = navigationController.getPath (transform.position, path[path.Count - 1]);
				pathRefreshCount = 0;
			}
			else
			{
				List<Vector3> returnPath = navigationController.quickScanPath (transform.position, path[path.Count - 1]);
				if (!navigationController.pathIsInvalid (returnPath))
				{
					path = returnPath;
				}
				
				pathRefreshCount++;
			}
		}
		else
		{
			path=null;
			navTarget = Vector3.zero;
			setRails (false);
			stateDelegate=Idle;
		}
	}
	
	protected override void Seek ()
	{
		Debug.DrawLine (transform.position, navTarget, Color.magenta);
		
		float distance = Vector3.Distance (new Vector3(transform.position.x,0,transform.position.z),new Vector3(navTarget.x,0,navTarget.z));
		
		if (distance >= 1f || (attackTarget != null && distance >= attackRange))
		{
			
			Vector3 targetDir = new Vector3 (navTarget.x, 0, navTarget.z) - new Vector3 (transform.position.x, 0, transform.position.z);
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
			navTarget = Vector3.zero;
			setRails (false);
		}
	}
	
	
	private void Startled()
	{
		// catch if the player units have backed away before we get here
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
		if (attackTarget == null)
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
			navigationController.getPath(
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
		else if (other.CompareTag("lancer"))
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
		else if (other.CompareTag("lancer"))
		{
			playerUnitsNearby.Remove (other.GetComponent<PlayerUnitController>());
		}
	}
}
