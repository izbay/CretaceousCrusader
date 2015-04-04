using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitController : MonoBehaviour {

	public float turnSpeed;
	public float moveSpeed;


	private NavigationController navigationController;
	private Vector3 target = Vector3.zero;
	private Vector3 curr_pos;
	private bool onRails = false;
	private List<Vector3> path;
	private int counter = 0;

	// Use this for initialization
	void Start () {
		SavePosition ();
		navigationController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
	}
	
	// Update is called once per frame
	void Update () {
		// If a target exists, move to it.
		if (target != Vector3.zero) {	
			Seek ();
			SavePosition ();
		}

		// Check path with every update.

		if(path != null && onRails){
			setTarget(path[1]);
			if(counter == 100){
				path = navigationController.getPath (transform.position, path[path.Count-1]);
				counter = 0;
			} else {
				Vector3 start = transform.position;
				Vector3 end = path[path.Count-1];
				List<Vector3> returnPath = navigationController.quickScanPath (start, end);
				if(!navigationController.pathIsInvalid (returnPath)){
					path = returnPath;
				}
				counter++;
			}
		} else {
			path = null;
			setRails (false);
		}
	}

	void Seek () {
		//Debug.DrawLine (curr_pos, target, Color.magenta);

		float distance = Vector3.Distance (curr_pos,target);
		
		if(distance >= 1f) {

			Vector3 targetDir = new Vector3(target.x,0,target.z) - new Vector3(curr_pos.x,0,curr_pos.z);
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);

			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed);
			float angle = Vector3.Angle (targetDir, transform.forward);
			if (angle < 3){
				rigidbody.AddForce(transform.forward * moveSpeed *500);
			}
		} else {
			// We've reached the target. Clear out all memory of it and await further instructions.
			target = Vector3.zero;
			setRails (false);
		}
	}

	void SavePosition () {
		curr_pos = transform.position;
	}

	public void setPath (List<Vector3> path){
		this.path = path;
		setRails (true);
	}

	public void setTarget(Vector3 target){
		this.target = target;
	}

	public void setRails(bool railSetting){
		onRails = railSetting;
		Animator anim = transform.Find("AnimationContainer").GetComponent<Animator>();
		anim.SetBool ("onRails",railSetting);
	}
}
