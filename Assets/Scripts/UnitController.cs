using UnityEngine;
using System.Collections;

public class UnitController : MonoBehaviour {

	public float turnSpeed;
	public float moveSpeed;

	private Vector3 target = Vector3.zero;
	private Vector3 curr_pos;
	private bool onRails = false;

	// Use this for initialization
	void Start () {
		SavePosition ();
	}
	
	// Update is called once per frame
	void Update () {
		// If a target exists, move to it.
		if (target != Vector3.zero) {	
			Seek ();
			SavePosition ();
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
				rigidbody.AddForce(transform.forward * moveSpeed * 120);
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

	public void setTarget(Vector3 target){
		this.target = target;
	}

	public void setRails(bool railSetting){
		onRails = railSetting;
		Animator anim = transform.Find("AnimationContainer").GetComponent<Animator>();
		anim.SetBool ("onRails",railSetting);
	}
	
	public bool isOnRails(){
		return onRails;
	}
}
